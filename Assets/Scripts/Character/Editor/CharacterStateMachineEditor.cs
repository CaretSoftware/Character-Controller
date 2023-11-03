using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Character {
    [CustomEditor(typeof(MovementStateMachine))]
    public class CharacterStateMachineEditor : Editor {
        private MovementStateMachine movementStateMachineController;
        private SerializedProperty statesListProperty;
        private ReorderableList reorderableList;

        private void OnEnable() {
            statesListProperty = serializedObject.FindProperty("states");

            reorderableList = new ReorderableList(serializedObject, statesListProperty, true, true, true, true);

            reorderableList.onSelectCallback = (ReorderableList list) => {
                SerializedProperty stateElement = statesListProperty?.GetArrayElementAtIndex(list.index);
                if (stateElement is {
                        propertyType: SerializedPropertyType.ObjectReference, objectReferenceValue: CharacterState state
                    })
                    CharacterStateEditor.stateSelected?.Invoke(state);
            };
            reorderableList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    SerializedProperty stateElement = statesListProperty?.GetArrayElementAtIndex(index);
                    var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        element,
                        GUIContent.none);
                };
            reorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Character States"); };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (statesListProperty.isArray) {
                for (int i = 0; i < statesListProperty.arraySize; i++) {
                    SerializedProperty stateElement = statesListProperty.GetArrayElementAtIndex(i);
                    if (stateElement.objectReferenceValue != null) {
                        Editor stateEditor = CreateEditor(stateElement.objectReferenceValue);
                        stateEditor.OnInspectorGUI();
                    }
                }
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector(); // TODO
            
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
}
