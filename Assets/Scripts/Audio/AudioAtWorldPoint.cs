using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAtWorldPoint : MonoBehaviour {
    private AudioSource audioSource;
    private Action<AudioAtWorldPoint> releaseAction;
    private float timeToRelease;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 point, float volume) {
        SetPosition(point);
        audioSource.volume = volume;
        audioSource.clip = clip;
        audioSource.Play();
        float clipLength = (float)clip.samples / clip.frequency;
        Invoke(nameof(ReturnToPool), clipLength);
    }
    
    private void SetPosition(Vector3 pos) => transform.position = pos;
    
    private void ReturnToPool() => releaseAction(this);

    public void SetRelease(Action<AudioAtWorldPoint> releaseAction) => this.releaseAction = releaseAction;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red.WithAlpha(.25f);
        Gizmos.DrawSphere(transform.position, .5f);
    }
}
