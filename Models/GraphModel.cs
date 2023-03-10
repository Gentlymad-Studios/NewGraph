using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System;
#endif

namespace NewGraph {
    /// <summary>
    /// Our graph data model split into are minimalistic runtime part and editor specific extensions.
    /// See everything under #if UNITY_EDITOR for all editor specific parts of this class.
    /// </summary>
    [CreateAssetMenu(fileName =nameof(GraphModel), menuName = nameof(GraphModel), order = 1)]
    public class GraphModel : ScriptableObject {
        /// <summary>
        /// List of all the nodes we want to work on.
        /// </summary>
        [SerializeReference]
        public List<NodeModel> nodes = new List<NodeModel>();
#if UNITY_EDITOR
        // FROM HERE BE DRAGONS...
        [NonSerialized]
        public SerializedObject serializedGraphData;
        [NonSerialized]
        private SerializedProperty nodesProperty = null;
        [NonSerialized]
        private SerializedProperty utilityNodesProperty = null;

        [HideInInspector, SerializeField]
        private string tmpNameProperty;

        [SerializeField]
        private bool viewportInitiallySet = false;
        public bool ViewportInitiallySet => viewportInitiallySet;
        private SerializedProperty viewportInitiallySetProperty = null;
        private SerializedProperty ViewportInitiallySetProperty {
            get {
                if (viewportInitiallySetProperty == null) {
                    viewportInitiallySetProperty = serializedGraphData.FindProperty(nameof(viewportInitiallySet));
                }
                return viewportInitiallySetProperty;
            }
        }

        [SerializeField]
        private Vector3 viewPosition;
        public Vector3 ViewPosition => viewPosition;
        [NonSerialized]
        private SerializedProperty viewPositionProperty = null;
        private SerializedProperty ViewPositionProperty {
            get {
                if (viewPositionProperty == null) {
                    viewPositionProperty = serializedGraphData.FindProperty(nameof(viewPosition));
                }
                return viewPositionProperty;
            }
        }

        [SerializeField]
        private Vector3 viewScale;
        public Vector3 ViewScale => viewScale;
        [NonSerialized]
        private SerializedProperty viewScaleProperty = null;
        private SerializedProperty ViewScaleProperty {
            get {
                if (viewScaleProperty == null) {
                    viewScaleProperty = serializedGraphData.FindProperty(nameof(viewScale));
                }
                return viewScaleProperty;
            }
        }


        [SerializeReference]
        public List<NodeModel> utilityNodes = new List<NodeModel>();

        public void SetViewport(Vector3 position, Vector3 scale) {
            if (position != viewPosition || scale != viewScale) {
                ViewportInitiallySetProperty.boolValue = true;
                ViewScaleProperty.vector3Value = scale;
                ViewPositionProperty.vector3Value = position;
                serializedGraphData.ApplyModifiedProperties();
            }
        }

        public NodeModel AddNode(INode node, bool isUtilityNode) {
            NodeModel baseNodeItem = new NodeModel(node);
            baseNodeItem.isUtilityNode = isUtilityNode;
            if (!isUtilityNode) {
                nodes.Add(baseNodeItem);
            } else {
                utilityNodes.Add(baseNodeItem);
            }
            ForceSerializationUpdate();
            return baseNodeItem;
        }

        public NodeModel AddNode(NodeModel nodeItem) {
            if (!nodeItem.isUtilityNode) {
                nodes.Add(nodeItem);
            } else {
                utilityNodes.Add(nodeItem);
            }
            return nodeItem;
        }

        public void RemoveNode(NodeModel node) {
            if (!node.isUtilityNode) {
                nodes.Remove(node);
            } else {
                utilityNodes.Remove(node);
            }
        }

        public void RemoveNodes(List<NodeModel> nodesToRemove) {
            if (nodesToRemove.Count > 0) {
                Undo.RecordObject(this, "Remove Nodes");
                foreach (NodeModel node in nodesToRemove) {
                    RemoveNode(node);
                }
            }
        }

        public void ForceSerializationUpdate() {
            serializedGraphData.Update();
            EditorUtility.SetDirty(this);
            serializedGraphData.ApplyModifiedProperties();
        }

        public void CreateSerializedObject() {
            serializedGraphData = new SerializedObject(this);
            nodesProperty = null;
            utilityNodesProperty = null;
            viewPositionProperty = null;
            viewScaleProperty = null;
            viewportInitiallySetProperty = null;
        }

        public SerializedProperty GetNodesProperty(bool isUtilityNode) {
            if (!isUtilityNode) {
                if (nodesProperty == null) {
                    nodesProperty = serializedGraphData.FindProperty(nameof(nodes));
                }
                return nodesProperty;
            }

            if (utilityNodesProperty == null) {
                utilityNodesProperty = serializedGraphData.FindProperty(nameof(utilityNodes));
            }
            return utilityNodesProperty;
        }

        public SerializedProperty GetLastAddedNodeProperty(bool isUtilityNode) {
            if (isUtilityNode) {
                return GetNodesProperty(true).GetArrayElementAtIndex(utilityNodes.Count - 1);
            }
            return GetNodesProperty(false).GetArrayElementAtIndex(nodes.Count - 1);
        }
#endif
    }

}
