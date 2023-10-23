using System;
using UnityEngine;
using System.Collections.Generic;

public class StateMachine : MonoBehaviour {

    protected State currentState;
    protected State queuedState;
    protected State lastState;

    protected Dictionary<Type, State> _states = new Dictionary<Type, State>();
    [SerializeField] public List<State> states;
    
    private void Awake() {
        foreach (State state in states) {
            State instance = state;
            instance.stateMachine = this;
            _states.Add(instance.GetType(), instance);
            currentState ??= instance;
        }

        queuedState = currentState;
        currentState?.Enter();
    }

    private void Update() {
        if (currentState != queuedState) {
            currentState.Exit();
            lastState = currentState;
            currentState = queuedState;
            currentState.Enter();
        }

        currentState.Update();
    }
    
    private void LateUpdate() => currentState.LateUpdate();

    private void FixedUpdate() => currentState.FixedUpdate();

    public void TransitionTo<T>() where T : State => queuedState = _states[typeof(T)];
}
