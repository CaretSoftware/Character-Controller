using UnityEngine;

[CreateAssetMenu(menuName = "States/Inactive")]
public class Inactive : CharacterState {

    public override void Enter() { }

    public override void Update() {
        if (movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo(movementStateMachine.InitialState.GetType());
    }

    public override void Exit() { }
}
