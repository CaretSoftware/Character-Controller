using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class VolumeSliderAddListeners : MonoBehaviour {
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    public delegate void SetSFXSliderInteractable(bool interactable);
    public delegate void SetMusicSliderInteractable(bool interactable);
    
    public static SetSFXSliderInteractable setSFXInteractable;
    public static SetMusicSliderInteractable setMusicInteractable;

    private void Start() {
        setSFXInteractable = SetInteractableSFX;
        setMusicInteractable = SetInteractableMusic;
        
        float volMaster = PlayerPrefs.GetFloat(SoundManager.preferencesVolumeMaster, masterVolumeSlider.value);
        masterVolumeSlider.value = volMaster;
        masterVolumeSlider.onValueChanged.AddListener( val => SoundManager.Instance.ChangeMasterVolume(val));
        masterVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeMaster , val));
        masterVolumeSlider.onValueChanged?.Invoke(volMaster);

        float volMusic = PlayerPrefs.GetFloat(SoundManager.preferencesVolumeMusic, musicVolumeSlider.value);
        musicVolumeSlider.value = volMusic;
        musicVolumeSlider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeMusicVolume(val));
        musicVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeMusic , val));
        musicVolumeSlider.onValueChanged?.Invoke(volMusic);

        float volSFX = PlayerPrefs.GetFloat(SoundManager.preferencesVolumeSFX, sfxVolumeSlider.value);
        sfxVolumeSlider.value = volSFX;
        sfxVolumeSlider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeSfxVolume(val));
        sfxVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeSFX ,val));
        sfxVolumeSlider.onValueChanged?.Invoke(volSFX);
    }

    private void OnDestroy() {
        if (masterVolumeSlider != null && masterVolumeSlider.onValueChanged != null)
            masterVolumeSlider.onValueChanged?.RemoveAllListeners();
        if (musicVolumeSlider != null && musicVolumeSlider.onValueChanged != null)
            musicVolumeSlider.onValueChanged?.RemoveAllListeners();
        if (sfxVolumeSlider != null && sfxVolumeSlider.onValueChanged != null)
            sfxVolumeSlider.onValueChanged?.RemoveAllListeners();
    }

    private void SetPref(string pref, float percent) => PlayerPrefs.SetFloat(pref, percent);

    private void SetInteractableSFX(bool interactable) => sfxVolumeSlider.interactable = interactable;

    private void SetInteractableMusic(bool interactable) => musicVolumeSlider.interactable = interactable;
}
