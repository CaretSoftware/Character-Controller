using UnityEngine;

namespace Character {
    public class FootDust : MonoBehaviour {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private ParticleSystem dustParticleSystem;
        [SerializeField] private ParticleSystem landParticleSystem;
        [SerializeField, Range(0f, 1f)] private float footStepVolume = .75f; 
        [SerializeField, Range(0f, 1f)] private float footHorizontalOffset = .25f; 
        [SerializeField, Range(0f, 1f)] private float landingMaxInterval = .25f; 
        private bool groundedLastFrame = true;
        private float timeLastLanding;
        
        private void Awake() {
            if (characterController == null)
                characterController = GetComponentInParent<CharacterController>();
        }

        private void FixedUpdate() {
            bool grounded = characterController.isGrounded;

            bool landed = grounded && !groundedLastFrame && Time.time > timeLastLanding + landingMaxInterval;//  characterController.velocity.y < -.05f * Time.deltaTime; 
        
            if (Time.timeScale > .95f || (!groundedLastFrame && Time.timeScale < .95f))
                groundedLastFrame = grounded;
        
            if (landed) {
                timeLastLanding = Time.time;
                Vector3 pos = transform.position;
                SoundManager.PlaySoundAtPoint(Sound.FootSteps, pos);
                SoundManager.PlaySoundAtPoint(Sound.FootSteps, pos);
                landParticleSystem.Play();
            }
        }

        public void OnFootfall(AnimationEvent e) {
            Vector3 position = transform.position;
            if (!characterController.isGrounded)
                return;
        
            AchievementManager.stepCountDelegate?.Invoke(1);
            if (e.intParameter == 1) {
                Vector3 right = Vector3.right * footHorizontalOffset; 
                dustParticleSystem.transform.localPosition = right;
                dustParticleSystem.Play();
                SoundManager.PlaySoundAtPoint(Sound.FootSteps, position, footStepVolume);
            } else {
                Vector3 left = Vector3.left * footHorizontalOffset; 
                dustParticleSystem.transform.localPosition = left;
                dustParticleSystem.Play();
                SoundManager.PlaySoundAtPoint(Sound.FootSteps, position, footStepVolume);
            }
        }
    }
}
