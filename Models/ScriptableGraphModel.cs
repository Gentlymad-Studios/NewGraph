using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NewGraph {

    /// <summary>
    /// Our graph data model for scriptable object based graphs
    /// </summary>
    [CreateAssetMenu(fileName = nameof(ScriptableGraphModel), menuName = nameof(ScriptableGraphModel), order = 1)]
    public class ScriptableGraphModel : ScriptableObject, IGraphModelData {
        [SerializeField]
        private GraphModelBase baseModel = new GraphModelBase();

        public List<NodeModel> Nodes => baseModel.nodes;

#if UNITY_EDITOR
        public List<NodeModel> UtilityNodes => baseModel.utilityNodes;

        public SerializedObject SerializedGraphData {
            get {
                if (baseModel.serializedGraphData == null) {
                    CreateSerializedObject();
                }
                return baseModel.serializedGraphData;
            }
        }

        public bool ViewportInitiallySet => baseModel.ViewportInitiallySet;

        public Vector3 ViewPosition => baseModel.ViewPosition;

        public Vector3 ViewScale => baseModel.ViewScale;

        public Object BaseObject {
            get {
                if (baseModel.baseObject == null) {
                    CreateSerializedObject();
                }
                return baseModel.baseObject;
            }
        }

        [System.NonSerialized]
        private string assetHash = null;
        private string AssetHash {
            get {
                if (assetHash == null) {
                    assetHash = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
                }
                return assetHash;
            }
        }
        public string GUID => AssetHash;

        public NodeModel AddNode(INode node, bool isUtilityNode) {
            return baseModel.AddNode(node, isUtilityNode, this);
        }

        public NodeModel AddNode(NodeModel nodeItem) {
            return baseModel.AddNode(nodeItem);
        }

        public void CreateSerializedObject() {
            baseModel.CreateSerializedObject(this, nameof(baseModel));
        }

        public void ForceSerializationUpdate() {
            baseModel.ForceSerializationUpdate(this);
        }

        public SerializedProperty GetLastAddedNodeProperty(bool isUtilityNode) {
            return baseModel.GetLastAddedNodeProperty(isUtilityNode);
        }

        public SerializedProperty GetNodesProperty(bool isUtilityNode) {
            return baseModel.GetNodesProperty(isUtilityNode);
        }

        public SerializedProperty GetOriginalNameProperty() {
            return baseModel.GetOriginalNameProperty();
        }

        public SerializedProperty GetTmpNameProperty() {
            return baseModel.GetTmpNameProperty();
        }

        public void RemoveNode(NodeModel node) {
            baseModel.RemoveNode(node);
        }

        public void RemoveNodes(List<NodeModel> nodesToRemove) {
            baseModel.RemoveNodes(nodesToRemove, this);
        }

        public void SetViewport(Vector3 position, Vector3 scale) {
            baseModel.SetViewport(position, scale);
        }

        public static IGraphModelData GetGraphData(string GUID) {
            if (!string.IsNullOrWhiteSpace(GUID)) {
                string assetPath = AssetDatabase.GUIDToAssetPath(GUID);
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    return AssetDatabase.LoadAssetAtPath<ScriptableGraphModel>(assetPath);
                }
            }
            return null;
        }
#endif
    }
}
