using UnityEngine;

public class BubbleExhaustEvent : MonoBehaviour {
    [SerializeField] private ParticleSystem myParticleSystem;
    
    public void StartExhaust(AnimationEvent e) {
        if (e.intParameter == 1)
            myParticleSystem.Play();
        else
            myParticleSystem.Stop();
    }
}
