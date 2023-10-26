using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grounded))]
public class GroundedStateEditor : CharacterStateEditor {

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
    }
}
