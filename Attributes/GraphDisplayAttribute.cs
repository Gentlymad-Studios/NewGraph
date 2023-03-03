using System;

namespace NewGraph {
    /// <summary>
    /// Display attribute to transform/ control where fields show up in a graph.
    /// Want to display data directly in the graph? Use: DisplayType.NodeView
    /// Want to display in the inspector? Use: DisplayType.Inspector
    /// Don't want a foldout? Set: createGroup=false;
    /// You get the idea...
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphDisplayAttribute : Attribute {

        public DisplayType displayType = DisplayType.Inspector;
        public Editability editability = Editability.BothViews;
        public bool createGroup = true;

        public GraphDisplayAttribute(DisplayType displayType = DisplayType.Unspecified, Editability editability = Editability.Unspecified, bool createGroup = true) {
            if (displayType != DisplayType.Unspecified) {
                this.displayType = displayType;
            }

            if (editability != Editability.Unspecified) {
                this.editability = editability;
            }

            this.createGroup = createGroup;
        }
    }
}
