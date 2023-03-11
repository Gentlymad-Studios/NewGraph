using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace NewGraph {
    /// <summary>
    /// Our data model for Nodes split into a minimalistic runtime part and editor specific extensions.
    /// See #if UNITY_EDITOR sections for all editor specific parts of this class.
    /// The compilation tags allow us to strip away any unwated data for a runtime scenario.
    /// Source: https://docs.unity3d.com/2022.2/Documentation/Manual/script-Serialization.html
    /// Discussion: https://forum.unity.com/threads/serialize-fields-only-in-editor.433422/
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
        
        /// <summary>
        /// node position
        /// </summary>
        [SerializeField]
        private float nodeX, nodeY;
        
        /// <summary>
        /// Node name
        /// </summary>
        [SerializeField]
        private string name;
        public const string nameIdentifier = nameof(name);
        
        /// <summary>
        /// Is this node a utility node?
        /// </summary>
        public bool isUtilityNode = false;

        /// <summary>
        /// standard .isExpanded behavior does not work for us, especially if there are two views.
        /// So we need to manage the expanded states of our "groups"/foldouts ourselves.
        /// </summary>
        [Serializable]
        public class FoldoutState {
            public int relativePropertyPathHash;
            [NonSerialized]
            public bool used = false;
            public bool isExpanded = true;
        }

        /// <summary>
        /// Serialied list of all foldout states
        /// </summary>
        [SerializeField]
        private List<FoldoutState> foldouts = new List<FoldoutState>();
        
        /// <summary>
        /// Dictionary is needed as we might have many foldouts based on how deep the class structure goes
        /// </summary>
        private Dictionary<int, FoldoutState> foldoutsLookup = null;
        private Dictionary<int, FoldoutState> FoldoutsLookup {
            get {
                if (foldoutsLookup == null) {
                    foldoutsLookup = new Dictionary<int, FoldoutState>();
                    foreach (FoldoutState foldoutState in foldouts) {
                        foldoutsLookup.Add(foldoutState.relativePropertyPathHash, foldoutState);
                    }
                }
                return foldoutsLookup;
            }
        }

        [NonSerialized]
        private bool dataIsSet = false;
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

        /// <summary>
        /// Get a foldout state based on a hash or create it if not present
        /// </summary>
        /// <param name="pathHash"></param>
        /// <param name="defaultState"></param>
        /// <returns></returns>
        public FoldoutState GetOrCreateFoldout(int pathHash, bool defaultState=true) {
            if (!FoldoutsLookup.ContainsKey(pathHash)) {
                //serializedProperty.serializedObject.Update();

                FoldoutState foldoutState = new FoldoutState() { relativePropertyPathHash = pathHash, isExpanded = defaultState };
                FoldoutsLookup.Add(pathHash, foldoutState);
                foldouts.Add(foldoutState);

                //EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
                //serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            return FoldoutsLookup[pathHash];
        }

        /// <summary>
        /// Remove unused states so we dont pollute this if the class structure changes.
        /// </summary>
        public void CleanupFoldoutStates() {
            //serializedProperty.serializedObject.Update();
            
            for (int i=foldouts.Count-1; i>=0; i--) {
                FoldoutState state = foldouts[i];
                if (!state.used || state.relativePropertyPathHash == default) {
                    foldouts.RemoveAt(i);
                }
            }

            //EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
            //serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
