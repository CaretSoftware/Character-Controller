using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Inactive))]
public class InactiveStateEditor : CharacterStateEditor {

    protected override void OnEnable() {
        base.OnEnable();
        targetName = target.name;
    }

    protected override void DisplayFields(bool selected) {
        base.DisplayFields(selected);
    }
}