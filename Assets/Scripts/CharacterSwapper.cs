using System.Collections.Generic;
using Character;
using UnityEngine;

public class CharacterSwapper : MonoBehaviour {
    public static CharacterSwapper Instance { get; private set; }
    public delegate void CycleCharacter(int increment);
    public static CycleCharacter cycleCharacter;
    public delegate void SwapCameraTarget(Transform transform);
    public static SwapCameraTarget swapCameraTarget;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private List<MovementStateMachine> characterControllers;
    [SerializeField] private Transform oldCharacterController;
    [SerializeField] private Transform arrowTransform;
    private int activeCharacter;
    
    private void Awake() { 
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        inputReader.CharacterSwapEvent += SwapCharacter;
    }

    private void Start() {
        ShowCursor(false);
        SwapCharacter(0);
    }

    private void ShowCursor(bool show) {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    private void OnDestroy() => inputReader.CharacterSwapEvent -= SwapCharacter;

    private void SwapCharacter(int increment) {
        int mod = characterControllers.Count;
        activeCharacter += increment;
        activeCharacter = (activeCharacter % mod + mod) % mod; // negative modulus
        cycleCharacter?.Invoke(activeCharacter);
        Transform character = characterControllers[activeCharacter] != null
            ? characterControllers[activeCharacter].transform
            : oldCharacterController;
        swapCameraTarget?.Invoke(character);

        ShowCursor(activeCharacter == 1);
        DisplayText(character.name);
        ShowArrow(character);
        CancelInvoke();
        Invoke(nameof(HideArrow), 2f);
    }

    public static int GetCharacterIndex(MovementStateMachine characterController) {
        if (!Instance.characterControllers.Contains(characterController)) 
            return Instance.characterControllers.Count ^1;

        return Instance.characterControllers.IndexOf(characterController);
    }

    private void ShowArrow(Transform characterTransform) {
        arrowTransform.parent = characterTransform; 
        arrowTransform.localPosition = Vector3.zero;
    }
    
    private void HideArrow() {
        arrowTransform.parent = null;
        arrowTransform.position = Vector3.down * 100;
    }

    private void DisplayText(string text) => UIText.updateText?.Invoke(text);
}
