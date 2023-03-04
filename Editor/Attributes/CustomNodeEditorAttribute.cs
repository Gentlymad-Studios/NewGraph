using System;

namespace NewGraph {

    public class CustomNodeEditorAttribute : Attribute {
        public Type nodeType;
        public bool editorForChildClasses = false;
        public CustomNodeEditorAttribute(Type nodeType, bool editorForChildClasses=false) {
            this.nodeType = nodeType;
            this.editorForChildClasses= editorForChildClasses;
        }
    }
}
