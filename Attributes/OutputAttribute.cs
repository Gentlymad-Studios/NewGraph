using System;

namespace NewGraph {
    /// <summary>
    /// Output port attribute, this needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : PortAttribute {
        /// <summary>
        /// Output port attribute, this needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
        /// </summary>
        public OutputAttribute() : base(Capacity.Single, PortDirection.Output) { }
    }
}
