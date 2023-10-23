using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Grounded))]
public class GroundedStateEditor : CharacterStateEditor {

    private void OnEnable() {
        targetName = target.name;
    }

    protected override void DisplayFields() { }
}
