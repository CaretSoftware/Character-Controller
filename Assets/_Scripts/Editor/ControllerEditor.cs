using UnityEditor;
using UnityEngine;

namespace Controller {
        
    [CustomEditor(typeof(CharController))]
    public class ControllerEditor : Editor {
        public override void OnInspectorGUI() {
            CharController characterController = (CharController)target;

            DrawDefaultInspector();

            GUILayout.Label("Select Outfits");
            
            if (GUILayout.Button("None")) 
                SelectOutfit(characterController, null);
            
            foreach (Outfit outfit in characterController.outfits)
                if (GUILayout.Button(outfit.items[0].name))
                    SelectOutfit(characterController, outfit);
        }
        
        private void SelectOutfit(CharController characterController, Outfit selectedOutfit) {
            foreach (Outfit outfit in characterController.outfits)
                foreach (GameObject item in outfit.items)
                    item.SetActive(outfit == selectedOutfit);
        }
    }
}
