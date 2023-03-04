using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace NewGraph {
    /// <summary>
    /// This NodeEditor class allows you to mode/customize the appearance of a normal node.
    /// This is a very similar implementation to CustomEditor for MonoBehaviours.
    /// 1. Create a new class in an editor environment that inherits from NodeEditor.
    /// 2. Add the CustomNodeEditorAttribute to the class and specify the node type, that your defined editor should operate/mod.
    /// </summary>
    public abstract class NodeEditor {
        /// <summary>
        /// This is a lookup/cache of all custom node editor and the node type they are bound to.
        /// key=NodeType
        /// value=NodeEditorType
        /// </summary>
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

        /// <summary>
        /// Assemble and setup our lookup, so it's easy for us to find and create a specific Node editor.
        /// </summary>
        private static void SetupEditorLookup() {
            // fast access to all types with a CustomNodeEditorAttribute via TypeCache.
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomNodeEditorAttribute>();
            foreach (Type type in types) {
                // make sure the type actually inherits from NodeEditor
                if (type.ImplementsOrInherits(typeof(NodeEditor))) {
                    CustomNodeEditorAttribute customNodeEditorAttribute = type.GetAttribute<CustomNodeEditorAttribute>();

                    // make sure that the defined type is implementing INode
                    if (customNodeEditorAttribute.nodeType.ImplementsOrInherits(typeof(INode))) {
                        // check for conflicts
                        if (!editorLookup.ContainsKey(customNodeEditorAttribute.nodeType)) {
                            editorLookup.Add(customNodeEditorAttribute.nodeType, type);

                            // In case child classes should also receive this NodeEditor add all derived types to the lookup
                            if (customNodeEditorAttribute.editorForChildClasses) {
                                TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(customNodeEditorAttribute.nodeType);
                                foreach (Type derivedType in derivedTypes) {
                                    if (!editorLookup.ContainsKey(derivedType)) {
                                        editorLookup.Add(derivedType, type);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a node editor for a specific node type.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public static NodeEditor CreateEditor(Type nodeType) {
            if (EditorLookup.ContainsKey(nodeType)) {
                return Activator.CreateInstance(EditorLookup[nodeType]) as NodeEditor;
            }
            return null;
        }

        /// <summary>
        /// Called when the editor is created and populated.
        /// </summary>
        /// <param name="nodeController"></param>
        public abstract void Initialize(NodeController nodeController);
    }
}
