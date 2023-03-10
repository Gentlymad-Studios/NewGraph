using System;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// Node attribute. Should be assigned to every class implementing INode that should show up as a Node in a graph.
    /// Make sure to implement the INode interface!
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute {
        /// <summary>
        /// Color of the node as a hex string, like #FFFFFFFF. Be aware: The last two characters are for alpha values!
        /// </summary>
        public Color color = default;

        /// <summary>
        /// A custom name for the node
        /// </summary>
        public string nodeName = null;

        /// <summary>
        /// The maximum amount of allowed connections to the input port of this node.
        /// </summary>
        public Capacity inputPortCapacity = Capacity.Multiple;

        /// <summary>
        /// The subcategories this node should appear under in the creation menu.
        /// </summary>
        public string categories = "";

        /// <summary>
        /// A custom name for the input port.
        /// </summary>
        public string inputPortName = null;

        /// <summary>
        /// Node attribute. Should be assigned to every class implementing INode that should show up as a Node in a graph.
        /// Make sure to implement the INode interface!
        /// </summary>
        /// <param name="color">Color of the node as a hex string, like #FFFFFFFF. Be aware: The last two characters are for alpha values!</param>
        /// <param name="categories">The subcategories this node should appear under in the creation menu.</param>
        /// <param name="inputPortName">A custom name for the input port.</param>
        /// <param name="inputPortCapacity">The maximum amount of allowed connections to the input port of this node.</param>
        /// <param name="nodeName">A custom name for the node.</param>
        public NodeAttribute(string color = null, string categories = "", string inputPortName = null, Capacity inputPortCapacity = Capacity.Multiple, string nodeName = null) {
            if (color != null) {
                ColorUtility.TryParseHtmlString(color, out this.color);
            }
            if (nodeName != null) {
                this.nodeName = nodeName;
            }
            this.inputPortName = inputPortName;
            this.inputPortCapacity = inputPortCapacity;
            this.categories = categories;
        }


        public string GetName(Type nodeType) {
            if (!string.IsNullOrEmpty(nodeName)) {
                return nodeName;
            }
            return nodeType.Name;
        }
    }
}
