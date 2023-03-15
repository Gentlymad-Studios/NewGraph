using OdinSerializer.Utilities;
using System;
using UnityEditor;

namespace NewGraph {
    public class EdgeDropMenu : ContextMenu {

        public PortView port;
        
        public static EdgeDropMenu CreateEdgeDropMenu(GraphController graphController) {
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomEdgeDropMenuAttribute>();
            EdgeDropMenu menu;

            foreach (Type type in types) {
                if (type.ImplementsOrInherits(typeof(EdgeDropMenu))) {
                    menu = CreateInstance(type) as EdgeDropMenu;
                    menu.Initialize(graphController);
                    return menu;
                }
            }

            menu = CreateInstance<EdgeDropMenu>();
            menu.Initialize(graphController);
            return menu;
        }

        protected override void AddNodeEntries() {
            PortView currentPort = port;
            foreach (Type type in currentPort.connectableTypes) {
                if (nodeTypeToCreationLabel.ContainsKey(type)) {
                    void CreateNodeAndConnect() {
                        NodeView nodeView = graphController.CreateNewNode(type, false);
                        graphController.ConnectPorts(currentPort, nodeView.inputPort);
                    }
                    AddNodeEntry(nodeTypeToCreationLabel[type], (obj) => CreateNodeAndConnect());
                }
            }
        }

        public override bool DefaultNodeEnabledCheck() {
            return true;
        }

        public override bool DefaultEnableCheck() {
            return true;
        }

        protected override void AddCommands() {}
    }
}
