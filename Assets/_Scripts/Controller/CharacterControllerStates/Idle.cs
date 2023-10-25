using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]

[CreateAssetMenu(menuName = "States/Character/Idle")]
public class Idle : Grounded {
    [SerializeField] private float timeToBored = 10f;
    private float timeIdling;

    private void OnValidate() {
        foreach (CharacterState characterState in children) {
            Idle copy = Cast<Idle>(characterState);
            if (copy.timeToBored != timeToBored)
                copy.timeToBored = timeToBored;
        }
    }

    public override void Enter() {
        // TODO Set Animation to idle
        setHorizontalVelocity?.Invoke(Vector3.zero);
        timeIdling = 0f;
    }

    public override void Update() {
        timeIdling += Time.deltaTime;
        if (timeIdling >= timeToBored) {
            Debug.Log("Bored");
            timeIdling = 0f;
        }
        base.Update();
    }

    public override void LateUpdate() { }

    public override void FixedUpdate() { }

    public override void Exit() { }
}
