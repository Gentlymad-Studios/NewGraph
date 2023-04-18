using UnityEditor.Callbacks;
using UnityEditor;
using System;

namespace NewGraph {
    [CustomEditor(typeof(ScriptableGraphModel))]
    public class ScriptableGraphModelEditor : GraphModelEditorBase {

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line) {
            IGraphModelData baseGraphModel = EditorUtility.InstanceIDToObject(instanceID) as IGraphModelData;
            if (baseGraphModel != null) {
                baseGraphModel.CreateSerializedObject();
                OpenGraph(baseGraphModel);
                return true;
            }
            return false;
        }
    }
}
