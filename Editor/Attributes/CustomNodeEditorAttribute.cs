using System;

namespace NewGraph {
    /// <summary>
    /// This attribute needs to be attached to a class that derives from NodeEditor.
    /// This is the glue needed to automatically inject/create the defined editor for a specific node type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute {
        /// <summary>
        /// This field is required and should contain the nodeType the node editor belongs to.
        /// </summary>
        public Type nodeType;
        /// <summary>
        /// This is an optional field that defaults to false. It can be used to re-use the same NodeEditor on derived nodes.
        /// </summary>
        public bool editorForChildClasses = false;
        public CustomNodeEditorAttribute(Type nodeType, bool editorForChildClasses=false) {
            this.nodeType = nodeType;
            this.editorForChildClasses= editorForChildClasses;
        }
    }
}
