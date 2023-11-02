using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Character {
        
    [CustomEditor(typeof(CharController))]
    public class ControllerEditor : Editor {
        private CharController characterController;
        
        public override void OnInspectorGUI() {
            characterController = (CharController)target;

            DrawDefaultInspector();

            OutFitSelection();
        }

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
    }
}
