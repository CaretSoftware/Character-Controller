using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class TankAudioEvents : MonoBehaviour {
    [SerializeField] private AudioSource startSource;
    [SerializeField] private AudioSource idleSource;
    [SerializeField] private AudioClip tractorStart;
    [SerializeField] private float engineWindDownTime = .3f;
    [SerializeField] private float engineThrottlePitch = 1.2f;
    private Character.IInput input;
    private CharacterController characterController;
    private double tractorStartDuration;
    private double idleStartTime;
    
    public void TankHatchDown(AnimationEvent e) => SoundManager.PlaySound(Sound.ToiletSeatDown, .5f);
    
    public void TankHatchUp(AnimationEvent e) => SoundManager.PlaySound(Sound.ToiletSeatUp, .5f);

    public void TankStartEngine(AnimationEvent e) => TankMotorSound();

    public void TankStopEngine(AnimationEvent e) => tankOn = false;

    private void Awake() {
        tractorStartDuration = (double)tractorStart.samples / tractorStart.frequency;
        input = GetComponentInParent<Character.IInput>();
        characterController = GetComponentInParent<CharacterController>();
    }

    private bool tankOn;
    private void TankMotorSound() {
        tankOn = true;
        idleSource.pitch = 1f;
        idleSource.volume = idleVolume = 1f;
        startSource.volume = 1f;
        
        startSource.Play();
        idleStartTime = AudioSettings.dspTime + tractorStartDuration;
        // Queue tractorIdle to play when tractorStart ends
        idleSource.PlayScheduled(idleStartTime);
    }

    private float idleVolume;
    private void Update() {
        // Lower engine volume. Make time for startUpSound to play
        if (!tankOn && idleVolume > 0f) {
            idleVolume -= Time.deltaTime * 1f / engineWindDownTime;
            idleSource.volume = idleVolume;
            startSource.volume = idleVolume;
        }
        if (!tankOn && idleVolume <= 0f && idleSource.isPlaying) {
            idleSource.Stop();
        }

        if (tankOn) {
            Vector3 velocity = characterController.velocity;
            velocity.y = 0f;
            float vel = velocity == Vector3.zero ? 0f : 1f;
            idleSource.pitch = Mathf.Lerp(1f, engineThrottlePitch ,  Mathf.Min(vel, input.Axis.magnitude));
        }
    }
}
