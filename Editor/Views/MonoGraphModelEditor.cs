using UnityEditor;
using UnityEngine.UIElements;

namespace NewGraph {
    [CustomEditor(typeof(MonoGraphModel))]
    public class MonoGraphModelEditor : GraphModelEditorBase {
        protected override void CreateGUI(VisualElement inspector) {}
    }
}

