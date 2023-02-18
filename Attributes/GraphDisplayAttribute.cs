using System;

namespace NewGraph {
    /// <summary>
    /// Display attribute to transform/ control where fields show up in a graph.
    /// Want to display data directly in the graph? Use: DisplayType.NodeView
    /// Want to display in the inspector? Use: DisplayType.Inspector
    /// You get the idea...
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphDisplayAttribute : Attribute {

        public DisplayType displayType = DisplayType.Inspector;
        public Editability editability = Editability.BothViews;
        public GraphDisplayAttribute(DisplayType displayType = DisplayType.Unspecified, Editability editability = Editability.Unspecified) {
            if (displayType != DisplayType.Unspecified) {
                this.displayType = displayType;
            }

            if (editability != Editability.Unspecified) {
                this.editability = editability;
            }
        }
    }
}
