using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Character {
    [CustomEditor(typeof(OutfitSelector))]
    public class OutfitSelectorEditor : Editor {
        private SerializedProperty outfits; 
        private SerializedProperty jumpBufferTime;

        private void OnEnable() => outfits = serializedObject.FindProperty("outfits");

        public override void OnInspectorGUI() => OutFitSelection();

        private void OutFitSelection() {
            OutfitSelector outfitSelector = target as OutfitSelector;
            if (outfitSelector.outfits == null || outfitSelector.outfits.Length == 0) {
                base.DrawDefaultInspector();
                return;
            }
                
            GUILayout.Label("Select Outfits");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("None")) {
                Undo.RecordObjects(outfitSelector.outfits.SelectMany(outfit => outfit.items).ToArray(), 
                    "Select Outfit None");
                SelectOutfit(outfitSelector, null);
            }

            int itemNo = 0;
            foreach (Outfit outfit in outfitSelector.outfits) {
                itemNo++;
                if (itemNo % 5 == 0) {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                if (outfit != null && outfit.items != null && outfit.items[0] != null && outfit.items.Length > 0 
                        && GUILayout.Button(outfit.items[0].name)) {
                        
                    Undo.RecordObjects(outfitSelector.outfits.SelectMany(outfit => outfit.items).ToArray(), 
                        "Select Outfit");
                    SelectOutfit(outfitSelector, outfit);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            base.DrawDefaultInspector();
        }
                
        private void SelectOutfit(OutfitSelector characterController, Outfit selectedOutfit) {
            foreach (Outfit outfit in characterController.outfits)
            foreach (GameObject item in outfit.items)
                item.SetActive(outfit == selectedOutfit);
        }
    }
}
