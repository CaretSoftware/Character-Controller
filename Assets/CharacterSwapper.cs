using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;

public class CharacterSwapper : MonoBehaviour {
    public static CharacterSwapper Instance { get; private set; }
    public delegate void CycleCharacter(int increment);
    public static CycleCharacter cycleCharacter;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private List<MovementStateMachine> characterControllers;

    private int activeCharacter;
    
    private void Awake() { 
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start() {
        inputReader.CharacterSwapEvent += SwapCharacter;
        cycleCharacter?.Invoke(activeCharacter);
    }

    private void OnDestroy() => inputReader.CharacterSwapEvent -= SwapCharacter;

    private void SwapCharacter(int increment) {
        int mod = characterControllers.Count;
        activeCharacter += increment;
        activeCharacter = (activeCharacter % mod + mod) % mod; // supports both negative and positive modulus
        cycleCharacter?.Invoke(activeCharacter);
    }

    public static int GetCharacterIndex(MovementStateMachine characterController) {
        if (!Instance.characterControllers.Contains(characterController)) return -1;

        return Instance.characterControllers.IndexOf(characterController);
    }
}
