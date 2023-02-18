using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {
    [CustomEditor(typeof(GraphSettings))]
    public class GraphSettingsEditor : Editor {

        public override VisualElement CreateInspectorGUI() {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement inspector = new VisualElement();

            UIElementsHelper.CreateGenericUI<GraphSettings>(serializedObject, inspector, ValueChanged);

            // Return the finished inspector UI
            return inspector;
        }

        private void ValueChanged(SerializedPropertyChangeEvent evt) {
            if (GraphSettings.Instance != null) {
                GraphSettings.Instance.NotifyValueChanged(evt);
            }
        }
    }
}
