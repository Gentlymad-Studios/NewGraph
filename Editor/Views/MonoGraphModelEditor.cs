using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace NewGraph {
    [CustomEditor(typeof(MonoGraphModel))]
    public class MonoGraphModelEditor : GraphModelEditorBase {
        protected override void CreateCustomizedListView(VisualElement inspector) {}
    }
}

