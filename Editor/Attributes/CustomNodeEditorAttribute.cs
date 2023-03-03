using System;

namespace NewGraph {

    public class CustomNodeEditorAttribute : Attribute {
        public Type nodeType;

        public CustomNodeEditorAttribute(Type nodeType) {
            this.nodeType = nodeType;
        }
    }
}
