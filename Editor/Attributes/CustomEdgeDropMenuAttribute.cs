using System;

namespace NewGraph {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomEdgeDropMenuAttribute : Attribute {

        public CustomEdgeDropMenuAttribute() { }
    }
}
