
using UnityEngine;
using UnityEngine.Serialization;

public class ToggleAudio : MonoBehaviour {
    [SerializeField] private bool toggleMusic, toggleSfx;
    [SerializeField] private GameObject[] toggleGroup;
    
    public void Toggle() {
        if (toggleMusic)
            SoundManager.Instance.ToggleMusic();

        if (toggleSfx)
            SoundManager.Instance.ToggleSfx();

        ToggleActive(toggleGroup);
    }

    private void ToggleActive(GameObject[] toggleGroup) {
        for (int i = 0; i < toggleGroup.Length; i++)
            toggleGroup[i].SetActive(!toggleGroup[i].activeInHierarchy);
    }
}
