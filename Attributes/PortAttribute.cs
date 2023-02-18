using System;
using System.Collections.Generic;

namespace NewGraph {
    /// <summary>
    /// Base port attribute. This needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
    /// You'll mostly want to use the Output attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortAttribute : Attribute {
        public Capacity capacity = Capacity.Single;
        public PortDirection direction = PortDirection.Input;
        public Func<Type, Type, bool> isValidConnectionCheck = null;

        private static Dictionary<ConnectionPolicy, Func<Type, Type, bool>> connectionPolicybehaviors = new Dictionary<ConnectionPolicy, Func<Type, Type, bool>>() {
            { ConnectionPolicy.Identical, (input, output) => input == output },
            { ConnectionPolicy.IdenticalOrSubclass, (input, output) => input == output || input.IsSubclassOf(output) },
        };

        public PortAttribute(Capacity capacity = Capacity.Unspecified, PortDirection direction = PortDirection.Unspecified, ConnectionPolicy connectionPolicy = ConnectionPolicy.Unspecified) {
            if (capacity != Capacity.Unspecified) {
                this.capacity = capacity;
            }
            if (direction != PortDirection.Unspecified) {
                this.direction = direction;
            }
            if (connectionPolicy == ConnectionPolicy.Unspecified) {
                connectionPolicy = ConnectionPolicy.IdenticalOrSubclass;
            }
            isValidConnectionCheck = connectionPolicybehaviors[connectionPolicy];
        }
    }
}
