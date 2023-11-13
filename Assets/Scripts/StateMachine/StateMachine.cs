using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StateMachine : MonoBehaviour {

    protected State currentState;
    protected State queuedState;
    protected List<State> stateHistory = new List<State>();

    protected Dictionary<Type, State> _states = new Dictionary<Type, State>();
    [SerializeField] protected List<State> states;
    
    protected virtual void Awake() {
        foreach (State state in states) {
            state.stateMachine = this;
            _states.Add(state.GetType(), state);
            currentState ??= state;
        }

        queuedState = currentState;
        if (currentState != null)
            currentState.Enter();
        
        stateHistory.Add(currentState);
    }

    protected virtual void Update() {
        if (currentState != queuedState) {
            currentState.Exit();
            currentState = queuedState;
            currentState.Enter();
            
            if (stateHistory.Count > 7)
                stateHistory.RemoveAt(0);
            stateHistory.Add(currentState);
        }

        currentState.Update();
    }
    
    private void LateUpdate() => currentState.LateUpdate();

    private void FixedUpdate() => currentState.FixedUpdate();

    public void TransitionTo<T>() where T : State => queuedState = _states[typeof(T)];
    
    public void TransitionTo(Type type) => queuedState = _states[type];
}
