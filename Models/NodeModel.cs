using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace NewGraph {
    /// <summary>
    /// Our data model for Nodes split into ar minimalistic runtime part and editor specific extensions.
    /// See #if UNITY_EDITOR sections for all editor specific parts of this class.
    /// </summary>
    [Serializable]
    public class NodeModel {
        /// <summary>
        /// The actual node data.
        /// </summary>
        [SerializeReference]
        public INode nodeData;

#if UNITY_EDITOR
        // FROM HERE BE DRAGONS...
        [SerializeField]
        private float nodeX, nodeY;
        [SerializeField]
        private string name;
        [NonSerialized]
        private bool dataIsSet = false;
        public bool isUtilityNode = false;
        public const string nameIdentifier = nameof(name);
        [NonSerialized]
        private SerializedProperty serializedProperty;
        [NonSerialized]
        private SerializedProperty nodeDataSerializedProperty;
        [NonSerialized]
        private SerializedProperty nameProperty;
        [NonSerialized]
        private SerializedProperty nodeXProperty;
        [NonSerialized]
        private SerializedProperty nodeYProperty;
        [NonSerialized]
        private static Dictionary<Type, NodeAttribute> nodeInfo = new Dictionary<Type, NodeAttribute>();
        [NonSerialized]
        public Type nodeType;
        [NonSerialized]
        public NodeAttribute nodeAttribute;

        public NodeModel(INode nodeData) {
            this.nodeData = nodeData;
            Initialize();
        }

        public Vector2 GetPosition() {
            return new Vector2(nodeX, nodeY);
        }

        public string GetName() {
            return name;
        }

        public static NodeAttribute GetNodeAttribute(Type type) {
            if (!nodeInfo.ContainsKey(type)) {
                nodeInfo.Add(type, Attribute.GetCustomAttribute(type, typeof(NodeAttribute)) as NodeAttribute);
            }
            return nodeInfo[type];
        }

        public void Initialize() {
            nodeType = nodeData.GetType();
            nodeAttribute = GetNodeAttribute(nodeType);
        }

        public string SetName(string name) {
            this.name = name;
            if (dataIsSet) {
                nameProperty.serializedObject.Update();
            }
            return name;
        }

        public void SetPosition(float positionX, float positionY) {
            if (dataIsSet) {
                if (positionX != nodeX || positionY != nodeY) {
                    nodeXProperty.floatValue = positionX;
                    nodeYProperty.floatValue = positionY;
                    nodeXProperty.serializedObject.ApplyModifiedProperties();
                }
            } else {
                nodeX = positionX;
                nodeY = positionY;
            }
        }

        public SerializedProperty GetSerializedProperty() {
            return serializedProperty;
        }

        public SerializedProperty GetSpecificSerializedProperty() {
            return nodeDataSerializedProperty;
        }

        public SerializedProperty GetNameSerializedProperty() {
            return nameProperty;
        }

        public void SetData(SerializedProperty serializedProperty) {
            this.serializedProperty = serializedProperty;
            nodeDataSerializedProperty = serializedProperty.FindPropertyRelative(nameof(nodeData));
            nameProperty = serializedProperty.FindPropertyRelative(nameof(name));
            nodeXProperty = serializedProperty.FindPropertyRelative(nameof(nodeX));
            nodeYProperty = serializedProperty.FindPropertyRelative(nameof(nodeY));
            dataIsSet = true;
        }
#endif
    }
}
