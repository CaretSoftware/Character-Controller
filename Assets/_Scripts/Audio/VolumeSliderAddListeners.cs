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
        masterVolumeSlider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeMasterVolume(masterVolumeSlider.value));
        masterVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeMaster ,masterVolumeSlider.value));
        masterVolumeSlider.onValueChanged?.Invoke(volMaster);

        float volMusic = PlayerPrefs.GetFloat(SoundManager.preferencesVolumeMusic, musicVolumeSlider.value);
        musicVolumeSlider.value = volMusic;
        musicVolumeSlider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeMusicVolume(musicVolumeSlider.value));
        musicVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeMusic ,musicVolumeSlider.value));
        musicVolumeSlider.onValueChanged?.Invoke(volMusic);

        float volSFX = PlayerPrefs.GetFloat(SoundManager.preferencesVolumeSFX, sfxVolumeSlider.value);
        sfxVolumeSlider.value = volSFX;
        sfxVolumeSlider.onValueChanged.AddListener(val => SoundManager.Instance.ChangeSfxVolume(sfxVolumeSlider.value));
        sfxVolumeSlider.onValueChanged.AddListener(val => SetPref(SoundManager.preferencesVolumeSFX ,sfxVolumeSlider.value));
        sfxVolumeSlider.onValueChanged?.Invoke(volSFX);
    }

    private void OnDestroy() {
        masterVolumeSlider.onValueChanged?.RemoveAllListeners();
        musicVolumeSlider.onValueChanged?.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged?.RemoveAllListeners();
    }

    private void SetPref(string pref, float percent) => PlayerPrefs.SetFloat(pref, percent);

    private void SetInteractableSFX(bool interactable) => sfxVolumeSlider.interactable = interactable;

    private void SetInteractableMusic(bool interactable) => musicVolumeSlider.interactable = interactable;
}
