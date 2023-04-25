using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NewGraph {

    [Serializable]
    public class GraphModelBase {

        /// <summary>
        /// List of all the nodes we want to work on.
        /// </summary>
        [SerializeReference, HideInInspector]
        public List<NodeModel> nodes = new List<NodeModel>();

#if UNITY_EDITOR
        // FROM HERE BE DRAGONS...
        [SerializeReference, HideInInspector]
        public List<NodeModel> utilityNodes = new List<NodeModel>();

        [NonSerialized]
        public SerializedObject serializedGraphData;

        [NonSerialized]
        public UnityEngine.Object baseObject;

        [NonSerialized]
        private SerializedProperty nodesProperty = null;
        
        [NonSerialized]
        private SerializedProperty utilityNodesProperty = null;

        [SerializeField, HideInInspector]
		private string tmpName;
        private SerializedProperty tmpNameProperty = null;
        private SerializedProperty originalNameProperty = null;

        [NonSerialized]
        private string basePropertyPath = null;

        private SerializedProperty GetProperty(string propertyToSearch) {
            return serializedGraphData.FindProperty(basePropertyPath + "." + propertyToSearch);
        }

        [SerializeField, HideInInspector]
		private bool viewportInitiallySet = false;
        public bool ViewportInitiallySet => viewportInitiallySet;

        private SerializedProperty viewportInitiallySetProperty = null;
        private SerializedProperty ViewportInitiallySetProperty {
            get {
                if (viewportInitiallySetProperty == null) {
                    viewportInitiallySetProperty = GetProperty(nameof(viewportInitiallySet));
                }
                return viewportInitiallySetProperty;
            }
        }

        [SerializeField, HideInInspector]
        private Vector3 viewPosition;
        public Vector3 ViewPosition => viewPosition;

        [NonSerialized]
        private SerializedProperty viewPositionProperty = null;
        private SerializedProperty ViewPositionProperty {
            get {
                if (viewPositionProperty == null) {
                    viewPositionProperty = GetProperty(nameof(viewPosition));
                }
                return viewPositionProperty;
            }
        }

        [SerializeField, HideInInspector]
        private Vector3 viewScale;
        public Vector3 ViewScale => viewScale;

        [NonSerialized]
        private SerializedProperty viewScaleProperty = null;
        private SerializedProperty ViewScaleProperty {
            get {
                if (viewScaleProperty == null) {
                    viewScaleProperty = GetProperty(nameof(viewScale));
                }
                return viewScaleProperty;
            }
        }

        public void SetViewport(Vector3 position, Vector3 scale) {
            if (position != viewPosition || scale != viewScale) {
                ViewportInitiallySetProperty.boolValue = true;
                ViewScaleProperty.vector3Value = scale;
                ViewPositionProperty.vector3Value = position;
                serializedGraphData.ApplyModifiedProperties();
            }
        }

        public NodeModel AddNode(INode node, bool isUtilityNode, UnityEngine.Object scope) {
            NodeModel baseNodeItem = new NodeModel(node);
            baseNodeItem.isUtilityNode = isUtilityNode;
            if (!isUtilityNode) {
                nodes.Add(baseNodeItem);
            } else {
                utilityNodes.Add(baseNodeItem);
            }
            ForceSerializationUpdate(scope);
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

        public void RemoveNodes(List<NodeModel> nodesToRemove, UnityEngine.Object scope) {
            if (nodesToRemove.Count > 0) {
                Undo.RecordObject(scope, "Remove Nodes");
                foreach (NodeModel node in nodesToRemove) {
                    RemoveNode(node);
                }
            }
        }

        public void ForceSerializationUpdate(UnityEngine.Object scope) {
            serializedGraphData.Update();
            EditorUtility.SetDirty(scope);
            serializedGraphData.ApplyModifiedProperties();
        }

        public void CreateSerializedObject(UnityEngine.Object scope, string rootFieldName) {
            basePropertyPath= rootFieldName;
            baseObject = scope;
            serializedGraphData = new SerializedObject(scope);
            nodesProperty = null;
            utilityNodesProperty = null;
            viewPositionProperty = null;
            viewScaleProperty = null;
            viewportInitiallySetProperty = null;
        }

        public SerializedProperty GetOriginalNameProperty() {
            if (originalNameProperty == null) {
                originalNameProperty = serializedGraphData.FindProperty("m_Name");
            }
            return originalNameProperty;
        }

        public SerializedProperty GetTmpNameProperty() {
            if (tmpNameProperty == null) {
                tmpNameProperty = GetProperty(nameof(tmpName));
            }
            return tmpNameProperty;
        }

        public SerializedProperty GetNodesProperty(bool isUtilityNode) {
            if (!isUtilityNode) {
                if (nodesProperty == null) {
                    nodesProperty = GetProperty(nameof(nodes));
                }
                return nodesProperty;
            }

            if (utilityNodesProperty == null) {
                utilityNodesProperty = GetProperty(nameof(utilityNodes));
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
