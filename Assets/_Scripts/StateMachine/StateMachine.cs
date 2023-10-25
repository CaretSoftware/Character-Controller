using System;
using UnityEngine;
using System.Collections.Generic;

public class StateMachine : MonoBehaviour {

    protected State currentState;
    protected State queuedState;
    protected State lastState;

    protected Dictionary<Type, State> _states = new Dictionary<Type, State>();
    [SerializeField] protected List<State> states;
    
    protected virtual void Awake() {
        foreach (State state in states) {
            state.stateMachine = this;
            _states.Add(state.GetType(), state);
            currentState ??= state;
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
