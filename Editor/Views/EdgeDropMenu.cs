using OdinSerializer.Utilities;
using System;
using UnityEditor;

namespace NewGraph {
    /// <summary>
    /// The EdgeDropMenu is being used after dropping an edge into empty space in the graph.
    /// This can be inherited by a custom class that has the [CustomEdgeDropMenu] attribute.
    /// Beware: This class inherits from GraphSearchWindowProvider and therefore is a ScriptableObject by nature.
    /// </summary>
    public class EdgeDropMenu : ContextMenu {
        /// <summary>
        /// the port this menu should operate on. This is required to be updated fromt he outside.
        /// </summary>
        public PortView port;

        /// <summary>
        /// Instantiate a custom or default edge drop menu
        /// </summary>
        /// <param name="graphController">The graph controller this edge drop menu should operate on.</param>
        /// <returns>The instance of the newly created edge drop menu.</returns>
        public static EdgeDropMenu CreateEdgeDropMenu(GraphController graphController) {
            // get all types that have the [CustomEgeDropMenu] attribute
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomEdgeDropMenuAttribute>();
            EdgeDropMenu menu;

            foreach (Type type in types) {
                // is the detected type actually inheriting from ContextMenu?
                if (type.ImplementsOrInherits(typeof(EdgeDropMenu))) {
                    // create an instance of the custom ContextMenu type!
                    menu = CreateInstance(type) as EdgeDropMenu;
                    menu.Initialize(graphController);
                    return menu;
                }
            }

            // no custom editor found or ineligible
            menu = CreateInstance<EdgeDropMenu>();
            menu.Initialize(graphController);
            return menu;
        }

        /// <summary>
        /// Add all node entries.
        /// </summary>
        protected override void AddNodeEntries() {
            PortView currentPort = port;
            foreach (Type type in currentPort.connectableTypes) {
                if (nodeTypeToCreationLabel.ContainsKey(type)) {
                    void CreateNodeAndConnect() {
                        NodeView nodeView = graphController.CreateNewNode(type, false);
                        if (nodeView.inputPort != null) {
                            graphController.ConnectPorts(currentPort, nodeView.inputPort);
                        }
                    }
                    AddNodeEntry(nodeTypeToCreationLabel[type], (obj) => CreateNodeAndConnect());
                }
            }
        }

        /// <summary>
        /// The edge drop menu node entries should always be enabled.
        /// </summary>
        /// <returns></returns>
        public override bool DefaultNodeEnabledCheck() {
            return true;
        }

        /// <summary>
        /// The edge drop menu entries should always be enabled.
        /// </summary>
        /// <returns></returns>
        public override bool DefaultEnabledCheck() {
            return true;
        }

        /// <summary>
        /// At least on default, the edge drop menu does not have a command panel.
        /// </summary>
        protected override void AddCommands() {}
    }
}
