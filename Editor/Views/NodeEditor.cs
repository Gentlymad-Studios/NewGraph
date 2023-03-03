using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NewGraph {
    public abstract class NodeEditor {
        [NonSerialized]
        private static Dictionary<Type, Type> editorLookup = null;
        public static Dictionary<Type, Type> EditorLookup {
            get {
                if (editorLookup == null) {
                    editorLookup = new Dictionary<Type, Type>();
                    SetupEditorLookup();
                }
                return editorLookup;
            }
        }

        private static void SetupEditorLookup() {
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomNodeEditorAttribute>();
            foreach (Type type in types) {
                Debug.Log(type);
                if (type.ImplementsOrInherits(typeof(NodeEditor))) {
                    CustomNodeEditorAttribute customNodeEditorAttribute = type.GetAttribute<CustomNodeEditorAttribute>();
                    if (customNodeEditorAttribute.nodeType.ImplementsOrInherits(typeof(INode))) {
                        if (!editorLookup.ContainsKey(customNodeEditorAttribute.nodeType)) {
                            editorLookup.Add(customNodeEditorAttribute.nodeType, type);
                        }
                    }
                }
            }
        }

        public static NodeEditor CreateEditor(Type nodeType) {
            if (EditorLookup.ContainsKey(nodeType)) {
                return Activator.CreateInstance(EditorLookup[nodeType]) as NodeEditor;
            }
            return null;
        }

        public abstract void Initialize(NodeController nodeController);
    }
}
