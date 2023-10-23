using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSM))]
public class CharacterStateMachineEditor : Editor {
    private CharacterSM characterController;
    private SerializedProperty statesListProperty;
    private string stateListPropertyName = "states";

    public override void OnInspectorGUI() {
        characterController = (CharacterSM)target;
        List<State> states = characterController.states;

        int count = states.Count;
        for (int i = 0; i < count; i++) {
            if (states[i] != null) {
                //EditorGUILayout.PrefixLabel(states[i].GetType().ToString());
                Editor otherScriptEditor = CreateEditor(states[i]);
                otherScriptEditor.OnInspectorGUI();
            }
        }
        
        Undo.RecordObject(target, "States List Edit");
        statesListProperty = serializedObject.FindProperty(stateListPropertyName);
        EditorGUILayout.PropertyField(statesListProperty);
        serializedObject.ApplyModifiedProperties();
        //DrawDefaultInspector();
        if (GUI.changed)
            EditorUtility.SetDirty(target);

        //OutFitSelection();
    }

    /*
    private void OutFitSelection() {
        GUILayout.Label("Select Outfits");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("None")) {
            Undo.RecordObjects(characterController.outfits.SelectMany(outfit => outfit.items).ToArray(), 
                "Select Outfit None");
            SelectOutfit(characterController, null);
        }
        
        foreach (Outfit outfit in characterController.outfits)
            if (GUILayout.Button(outfit.items[0].name)) {
                Undo.RecordObjects(characterController.outfits.SelectMany(outfit => outfit.items).ToArray(), 
                    "Select Outfit");
                SelectOutfit(characterController, outfit);
            }
        EditorGUILayout.EndHorizontal();
    }
    
    private void SelectOutfit(CharController characterController, Outfit selectedOutfit) {
        foreach (Outfit outfit in characterController.outfits)
        foreach (GameObject item in outfit.items)
            item.SetActive(outfit == selectedOutfit);
    }
    */
}
