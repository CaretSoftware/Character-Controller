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
    
    private int activeCharacter;

    private void Awake() { 
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        inputReader.CharacterSwapEvent += SwapCharacter;
    }

    private void Start() => SwapCharacter(0);

    private void OnDestroy() => inputReader.CharacterSwapEvent -= SwapCharacter;

    private void SwapCharacter(int increment) {
        int mod = characterControllers.Count;
        activeCharacter += increment;
        activeCharacter = (activeCharacter % mod + mod) % mod; // negative modulus
        cycleCharacter?.Invoke(activeCharacter);
        swapCameraTarget?.Invoke(characterControllers[activeCharacter] != null 
                ? characterControllers[activeCharacter].transform : oldCharacterController);
    }

    public static int GetCharacterIndex(MovementStateMachine characterController) {
        if (!Instance.characterControllers.Contains(characterController)) 
            return Instance.characterControllers.Count ^1;

        return Instance.characterControllers.IndexOf(characterController);
    }
}
