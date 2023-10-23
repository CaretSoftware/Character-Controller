using UnityEngine;

public abstract class State : ScriptableObject {
    
    public StateMachine stateMachine;

    public virtual void Enter() { }

    public virtual void Update() { }

    public virtual void LateUpdate() { }

    public virtual void FixedUpdate() { }

    public virtual void Exit() { }
}
