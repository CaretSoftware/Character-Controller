using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AudioObjectPool : MonoBehaviour {
    private const int MaxSize = 50;
    public List<GameObject> pooledObjects;
    private ObjectPool<AudioAtWorldPoint> pool;
    
    [SerializeField] private AudioAtWorldPoint audioObject;
    [SerializeField] private int amountToPool = 30;
    [SerializeField] private bool collectionCheck = true;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        pool = new ObjectPool<AudioAtWorldPoint>(() => {
            return Instantiate(audioObject);
        }, actionOnGet: audioAtPoint => {
            audioAtPoint.gameObject.SetActive(true);
            audioAtPoint.SetRelease(ReturnToPool);
        }, actionOnRelease: audioAtPoint => {
            audioAtPoint.gameObject.SetActive(false);
        }, actionOnDestroy: audioAtPoint => {
            Destroy(audioAtPoint.gameObject);
        }, collectionCheck, amountToPool, MaxSize);
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume) {
        AudioAtWorldPoint audioAtPoint = pool.Get();
        audioAtPoint.PlayClipAtPoint(clip, position, volume);
    }

    private void ReturnToPool(AudioAtWorldPoint audioAtPoint) {
        pool.Release(audioAtPoint);
    }
}
