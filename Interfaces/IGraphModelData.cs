using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NewGraph {
    public interface IGraphModelData {
        public List<NodeModel> Nodes { get; }
#if UNITY_EDITOR
        public string GUID { get; }
        public List<NodeModel> UtilityNodes { get; }
        public SerializedObject SerializedGraphData { get; }
        public UnityEngine.Object BaseObject { get; }
        public bool ViewportInitiallySet { get; }
        public Vector3 ViewPosition { get; }
        public Vector3 ViewScale { get; }
        public void SetViewport(Vector3 position, Vector3 scale);
        public NodeModel AddNode(INode node, bool isUtilityNode);
        public NodeModel AddNode(NodeModel nodeItem);
        public void RemoveNode(NodeModel node);
        public void RemoveNodes(List<NodeModel> nodesToRemove);
        public void ForceSerializationUpdate();
        public void CreateSerializedObject();
        public SerializedProperty GetNodesProperty(bool isUtilityNode);
        public SerializedProperty GetTmpNameProperty();
        public SerializedProperty GetOriginalNameProperty();
        public SerializedProperty GetLastAddedNodeProperty(bool isUtilityNode);

#endif
    }
}
