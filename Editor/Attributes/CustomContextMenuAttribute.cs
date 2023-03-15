using System;

namespace NewGraph {
    [AttributeUsage(AttributeTargets.Class,  AllowMultiple = false)]
    public class CustomContextMenuAttribute : Attribute {

        public CustomContextMenuAttribute() { }
    }
}
