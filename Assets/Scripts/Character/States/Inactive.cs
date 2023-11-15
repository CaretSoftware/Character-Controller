using UnityEngine;

[CreateAssetMenu(menuName = "States/Inactive")]
public class Inactive : CharacterState {
    private readonly int tankEnteredHash = Animator.StringToHash("TankEntry");
    
    public override void Enter() {
        if (!animator.HasState(0, tankEnteredHash))
            animator.speed = 0f;
        characterController.Move(Vector3.zero);
    }

    public override void Update() {
        if (movementStateMachine.CharacterActive)
            movementStateMachine.TransitionTo(movementStateMachine.InitialState.GetType());
    }

    public override void Exit() => animator.speed = 1f;
}
