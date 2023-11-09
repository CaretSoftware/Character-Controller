using UnityEngine;

[CreateAssetMenu(menuName = "States/Tank/Off")]
public class TankOff : CharacterState {
    public delegate void TankEnter();
    public static TankEnter tankEnter;

    public override void Enter() => tankEnter += EnterTankMovement;

    public override void Exit() => tankEnter -= EnterTankMovement;

    private void EnterTankMovement() => movementStateMachine.TransitionTo<TankMovement>();
}
