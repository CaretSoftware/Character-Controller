using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Idle")]
public class Idle : Grounded {
    private static readonly int ForwardVelocity = Animator.StringToHash("ForwardVelocity");
    
    [SerializeField] private float timeToBored = 10f;
    private float timeIdling;

    private void OnValidate() {
        foreach (CharacterState characterState in instanceCopies) {
            Idle copy = Cast<Idle>(characterState);
            if (copy == null) continue;
            if (copy.timeToBored != timeToBored)
                copy.timeToBored = timeToBored;
        }
    }

    public override void Enter() {
        animator.SetFloat(ForwardVelocity, 0f);
        setHorizontalVelocity?.Invoke(Vector3.zero);
        timeIdling = 0f;
    }
    
    public override void Update() {
        timeIdling += Time.deltaTime;
        if (timeIdling >= timeToBored) {
            animator.SetBool(Bored, true);
            timeIdling = 0f;
        }

        //float verticalVelocity = movementStateMachine.PlatformVelocity.y >= 0f
        //        ? movementStateMachine.PlatformVelocity.y
        //        : -.1f;
        //characterController.Move(movementStateMachine.PlatformVelocity);

        base.Update();
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() {
        animator.SetBool(Bored, false);
    }
}
