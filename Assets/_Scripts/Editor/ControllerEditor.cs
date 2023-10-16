using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Controller {
        
    [CustomEditor(typeof(CharController))]
    public class ControllerEditor : Editor {
        public override void OnInspectorGUI() {
            CharController characterController = (CharController)target;

            DrawDefaultInspector();

            GUILayout.Label("Select Outfits");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("None")) {
                Undo.RecordObjects(characterController.outfits.SelectMany(outfit => outfit.items).ToArray(), 
                    "Select None");
                SelectOutfit(characterController, null);
            }
            
            foreach (Outfit outfit in characterController.outfits)
                if (GUILayout.Button(outfit.items[0].name)){
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
