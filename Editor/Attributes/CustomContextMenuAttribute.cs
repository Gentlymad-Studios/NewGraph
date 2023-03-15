using System;

namespace NewGraph {
    /// <summary>
    /// Attribute that can be attached to a class that inherits from ContextMenu to change & customize the main context menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,  AllowMultiple = false)]
    public class CustomContextMenuAttribute : Attribute {

        public CustomContextMenuAttribute() { }
    }
}
