using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioObjectPool))]
public class SoundManager : MonoBehaviour {
    private static Dictionary<Sound, AudioClip[]> soundToAudioClip = new Dictionary<Sound, AudioClip[]>();
    private static System.Random rnd = new System.Random();
    public static SoundManager Instance;
    
    [System.Serializable]
    public class SoundAsClip {
        public Sound sound;
        public AudioClip[] audioClips;
    }
    
    private float volumeBeforeMuteSFX;
    private float volumeBeforeMuteMusic;
    private bool muteMusic;
    private bool muteSFX;
    private float mindB = -80f;
    private float maxdBMaster = 0f;
    private float maxdBSFX = -10f;
    private float maxdBMusic = 10f;
    
    // TODO options to change music tracks, queue up fade to the next
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource undampenedSFXSource;
    [SerializeField] private AudioSource unscaledSFXSource;
    [SerializeField] private AudioObjectPool audioObjectPool;
    [SerializeField] private AudioMixer audioMixer;
    
    private string volumeMasterParameterString = "volumeMaster";
    private string volumeSFXParameterString = "volumeSFX";
    private string volumeMusicParameterString = "volumeMusic";
    private string volumeUnscaledSFXParameterString = "volumeUnscaledSFX";
    private string volumeUndampedSFXParameterString = "volumeUndampedSFX";
    private string lowPassCutoffFreqMusicParameterString = "musicLowPassCutoffFreq";
    private string lowPassCutoffFreqSFXParameterString = "sfxLowPassCutoffFreq";

    // Player Prefs strings
    public static readonly string preferencesVolumeMaster = "preferencesVolumeMaster";
    public static readonly string preferencesVolumeSFX = "preferencesVolumeSFX";
    public static readonly string preferencesVolumeMusic = "preferencesVolumeMusic";
    
    [Header("Sound Bank")]
    public SoundAsClip[] soundClips;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundToAudioClipDictionary();
        } else {
            Destroy(gameObject);
        }
    }

    private Func<float, float> dampenVolumeFunc = (x) => x;
    private float sfxDampenedVolume = 1f;
    private float musicDampenedVolume = 1f;
    private float dampenDuration = 1f;
    private float sfxVolumeBeforeDampening = 1f;
    private float musicVolumeBeforeDampening = 1f;
    private float time;
    private bool dampen;
    private void Update() {
        sfxSource.pitch = Time.timeScale;

        if (Input.GetKeyDown(KeyCode.T))
            DampenAudioTemporarily(0f, 2f, Ease.InExpo);

        if (dampen) {
            time = (time >= 1f) ? 1f : time + Time.deltaTime * (1f / dampenDuration);
            dampen = time != 1f;

            ChangeSfxVolume(Mathf.Lerp(sfxDampenedVolume, sfxVolumeBeforeDampening, dampenVolumeFunc(time)), true);
            ChangeMusicVolume(Mathf.Lerp(musicDampenedVolume, musicVolumeBeforeDampening, dampenVolumeFunc(time)), true);
        }
    }

    private void InitializeSoundToAudioClipDictionary() {
        foreach (var soundAsClip in soundClips) {
            soundToAudioClip.Add(soundAsClip.sound, soundAsClip.audioClips);
        }
    }
    
    public static void PlaySoundAtPoint(Sound sound, Vector3 point, float volume = 1f) {
        volume *= Instance.sfxSource.volume;
        AudioClip clip = GetAudioClip(sound);
        Instance.audioObjectPool.PlayClipAtPoint(clip, point, volume);
    }

    public static void PlaySound(Sound sound, float volume = 1f, bool unscaledTime = false, bool undampened = false) {
        AudioClip clip = GetAudioClip(sound);
        Instance.PlaySound(clip, volume, unscaledTime, undampened);
    }

    private void PlaySound(AudioClip clip, float volume = 1f, bool unscaledTime = false, bool undampened = false) {
        if (clip == null) {
            Debug.LogWarning($"<b>{nameof(SoundManager)} {nameof(PlaySound)} AudioClip <i>clip</i> <color=red>null</color> <i>Assign in inspector?</i></b>", this);
            return;
        }
        if (unscaledTime)
            unscaledSFXSource.PlayOneShot(clip, volume);
        else {
            if (undampened)
                undampenedSFXSource.PlayOneShot(clip, volume);
            else
                sfxSource.PlayOneShot(clip, volume);
        }
    }

    private static AudioClip GetAudioClip(Sound sound) {
        soundToAudioClip.TryGetValue(sound, out AudioClip[] clips);

        AudioClip clip = GetAudioClip(clips);
        if (clip == null) {
            Debug.LogError($"<b>{nameof(SoundManager)} {nameof(GetAudioClip)}</b> <color=red>No such Sound found</color> <b>¯\\_(ツ)_/¯</b>", Instance.gameObject);
            return null;
        }
        return clip;
    }

    private static AudioClip GetAudioClip(AudioClip[] clips) {
        if (clips == null) {
            Debug.LogError($"<b>{nameof(SoundManager)} {nameof(GetAudioClip)} Sound Array</b> <color=red>null</color> <b>¯\\_(ツ)_/¯</b>", Instance.gameObject);
            return null;
        }

        // Keep the 0th position for the last played clip
        int maxIndex = clips.Length - 1;
        int startIndex = Math.Min(1, maxIndex);   // try get the second element (index 1), or if only one object (index 0)
        int randIndex = rnd.Next(startIndex, maxIndex);
        AudioClip clip = clips[randIndex];
        AudioClip temp = clips[0];
        clips[0] = clip;            // move this clip to first position so that it is not randomly played next time
        clips[randIndex] = temp;    // swap positions with the last one
        
        return clip;
    }
    
    public static void DampenAudioTemporarily(float percentVolume, float duration, Func<float, float> func = null) {
        percentVolume = percentVolume <= 1f ? percentVolume : 1f;   // don't increase volume
        Instance.dampen = true;
        Instance.time = 0f;
        Instance.sfxDampenedVolume = percentVolume * Instance.sfxVolumeBeforeDampening;
        Instance.musicDampenedVolume = percentVolume * Instance.musicVolumeBeforeDampening;
        Instance.dampenDuration = duration;
        Instance.dampenVolumeFunc = func ?? ((float x) => x);       // linear increase if no parameter given
    }

    public void ChangeMasterVolume(float percentage) {
        float dB = Mathf.Lerp(mindB, maxdBMaster, Ease.OutExpo(percentage));
        audioMixer.SetFloat(volumeMasterParameterString, dB);
    }
    
    public void ChangeMusicVolume(float percentage, bool dampedAlteration = false) {
        float dB = Mathf.Lerp(mindB, maxdBMusic, Ease.OutExpo(percentage));
        audioMixer.SetFloat(volumeMusicParameterString, dB);

        if (!dampedAlteration)
            musicVolumeBeforeDampening = percentage;
    }

    public void ChangeSfxVolume(float percentage, bool dampedAlteration = false) {
        float dB = Mathf.Lerp(mindB, maxdBSFX, Ease.OutExpo(percentage));
        audioMixer.SetFloat(volumeSFXParameterString, dB);
        audioMixer.SetFloat(volumeUnscaledSFXParameterString, dB);

        if (!dampedAlteration) {
            audioMixer.SetFloat(volumeUndampedSFXParameterString, dB);
            sfxVolumeBeforeDampening = percentage;
        }
    }

    public void ChangeLowPassCutoffFrequency(float value) {
        audioMixer.SetFloat(lowPassCutoffFreqMusicParameterString, value);
        audioMixer.SetFloat(lowPassCutoffFreqSFXParameterString, value);
    }

    public void ToggleSfx() {
        muteSFX = !muteSFX;
        
        VolumeSliderAddListeners.setSFXInteractable?.Invoke(!muteSFX);

        if (muteSFX) {
            audioMixer.GetFloat(volumeSFXParameterString, out float value);
            float percentage = Mathf.InverseLerp(mindB, maxdBSFX, value);
            volumeBeforeMuteSFX = percentage;
            ChangeSfxVolume(0f);
        } else {
            ChangeSfxVolume(volumeBeforeMuteSFX);
        }
    }

    public void ToggleMusic() {
        muteMusic = !muteMusic;

        VolumeSliderAddListeners.setMusicInteractable?.Invoke(!muteMusic);
        
        if (muteMusic) {
            audioMixer.GetFloat(volumeMusicParameterString, out float value);
            float percentage = Mathf.InverseLerp(mindB, maxdBMusic, value);
            volumeBeforeMuteMusic = percentage;
            ChangeMusicVolume(mindB);
        } else {
            ChangeMusicVolume(volumeBeforeMuteMusic);
        }
    }
}

