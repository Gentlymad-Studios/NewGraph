using System;
using System.Collections.Generic;

namespace NewGraph {
    /// <summary>
    /// Port Base attribute. This needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
    /// You'll mostly want to use the Port attribute instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortBaseAttribute : Attribute {
        public string name = null;
        public Capacity capacity = Capacity.Single;
        public PortDirection direction = PortDirection.Input;
        public ConnectionPolicy connectionPolicy = ConnectionPolicy.IdenticalOrSubclass;
        public Func<Type, Type, bool> isValidConnectionCheck = null;

        private static Dictionary<ConnectionPolicy, Func<Type, Type, bool>> connectionPolicybehaviors = new Dictionary<ConnectionPolicy, Func<Type, Type, bool>>() {
            { ConnectionPolicy.Identical, (input, output) => input == output },
			{ ConnectionPolicy.IdenticalOrSubclass, (input, output) => input == output || input.IsSubclassOf(output) || output.IsAssignableFrom(input) },
		};

        /// <summary>
        /// Port Base attribute. This needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
        /// You'll mostly want to use the Port attribute instead.
        /// </summary>
        /// <param name="capacity">How many connections are allowed.</param>
        /// <param name="direction">What port direction do we want to display?</param>
        /// <param name="connectionPolicy">What connections are allowed only to the matching class or subclasses as well?</param>
        public PortBaseAttribute(string name = null, Capacity capacity = Capacity.Unspecified, PortDirection direction = PortDirection.Unspecified, ConnectionPolicy connectionPolicy = ConnectionPolicy.Unspecified) {
            if (capacity != Capacity.Unspecified) {
                this.capacity = capacity;
            }
            if (direction != PortDirection.Unspecified) {
                this.direction = direction;
            }

            if (connectionPolicy != ConnectionPolicy.Unspecified) {
                this.connectionPolicy = connectionPolicy;
            }

            this.name = name;
            isValidConnectionCheck = connectionPolicybehaviors[this.connectionPolicy];
        }
    }
}
