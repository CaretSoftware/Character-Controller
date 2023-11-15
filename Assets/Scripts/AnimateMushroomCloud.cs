using System;
using System.Collections;
using UnityEngine;

public class AnimateMushroomCloud : MonoBehaviour {
    private static readonly int Explosion = Animator.StringToHash("Explode");
    private static Coroutine dimLightsCoroutine;
    
    [SerializeField] private Animator myAnimator;
    [SerializeField] private float setInactiveTime = 2.3f;
    [SerializeField] private float dimmedIntensity = 0f;
    [SerializeField] private float duration = 1f;
    private Light directionalLight;
    private float defaultIntensity;
    
    private void Awake() {
        directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        defaultIntensity = directionalLight.intensity;
    }

    public void Explode() {
        CancelInvoke();
        this.gameObject.SetActive(true);
        myAnimator.SetTrigger(Explosion);
        SoundManager.DampenAudioTemporarily(.2f, 3f, Ease.InExpo);
        SoundManager.PlaySound(Sound.SonicTinnitus, 1f, false, true);
        Invoke(nameof(SetInactive), setInactiveTime);
        if (dimLightsCoroutine != null)
            StopCoroutine(dimLightsCoroutine);
        dimLightsCoroutine = StartCoroutine(DimDirectionalLight());
    }

    private IEnumerator DimDirectionalLight() {
        float t = 0f;
        while (t < 1f) {
            directionalLight.intensity = Mathf.Lerp(dimmedIntensity, defaultIntensity, Ease.InExpo(t));
            t += Time.deltaTime * (1f / duration);
            yield return null;
        }
        directionalLight.intensity = defaultIntensity;
    }
    
    private void SetInactive() => this.gameObject.SetActive(false);
}
