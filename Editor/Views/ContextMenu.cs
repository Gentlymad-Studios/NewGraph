using GraphViewBase;
using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {
    public class ContextMenu : GraphSearchWindowProvider {
        protected static Dictionary<Type, string> nodeTypeToCreationLabel = new Dictionary<Type, string>();
        protected GraphController graphController;

        public void Initialize(GraphController graphController) {
            this.graphController = graphController;
            Initialize(this.graphController.graphView.shortcutHandler);
        }

        public static ContextMenu CreateContextMenu(GraphController graphController) {
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomContextMenuAttribute>();
            ContextMenu menu;

            foreach (Type type in types) {
                if (type.ImplementsOrInherits(typeof(ContextMenu))) {
                    menu = CreateInstance(type) as ContextMenu;
                    menu.Initialize(graphController);
                    return menu;
                }
            }

            menu = CreateInstance<ContextMenu>();
            menu.Initialize(graphController);
            return menu;
        }

        public virtual bool DefaultEnableCheck() {
            return graphController.graphView.GetSelectedNodeCount() > 0;
        }

        public virtual bool DefaultNodeEnabledCheck() {
            return graphController.graphData != null;
        }

        protected virtual string GetHeader() {
            return Settings.searchWindowRootHeader;
        }

        public virtual void BuildContextMenu() {
            StartAddingMenuEntries(GetHeader());
            AddNodeEntries();
            ResolveNodeEntries(DefaultNodeEnabledCheck);
            AddCommands();
        }

        protected virtual void AddNodeEntries() {
            AddNodeEntriesDefault();
        }

        protected void AddNodeEntriesDefault() {
            // get all types across all assemblies that implement our INode interface
            TypeCache.TypeCollection nodeTypes = TypeCache.GetTypesWithAttribute<NodeAttribute>();
            foreach (Type nodeType in nodeTypes) {
                // make sure the class tagged with the attribute actually is of type INode
                if (nodeType.ImplementsOrInherits(typeof(INode))) {
                    // check if we have a utility node...
                    bool isUtilityNode = nodeType.ImplementsOrInherits(typeof(IUtilityNode));
                    NodeAttribute nodeAttribute = NodeModel.GetNodeAttribute(nodeType);

                    // retrieve subcategories
                    string categoryPath = nodeAttribute.categories;
                    string endSlash = "/";
                    categoryPath.Replace(@"\", "/");
                    if (string.IsNullOrWhiteSpace(categoryPath)) {
                        categoryPath = endSlash;
                    } else if (!categoryPath.EndsWith(endSlash)) {
                        categoryPath += endSlash;
                    }
                    if (!categoryPath.StartsWith(endSlash)) {
                        categoryPath = endSlash + categoryPath;
                    }

                    // add to the list of createable nodes
                    string createNodeLabel = $"{categoryPath}{nodeAttribute.GetName(nodeType)}";
                    nodeTypeToCreationLabel.Add(nodeType, createNodeLabel.Substring(1));
                    createNodeLabel = (!isUtilityNode ? Settings.createNodeLabel : Settings.createUtilityNodeLabel) + createNodeLabel;
                    AddNodeEntry(createNodeLabel, (obj) => graphController.CreateNewNode(nodeType, isUtilityNode));
                }
            }
        }

        protected virtual void AddCommands() {
            AddDefaultCommands();
        }

        protected void AddDefaultCommands() {
            AddSeparator(Settings.searchWindowCommandHeader);
            AddShortcutEntry(Actions.Frame, SearchTreeEntry.AlwaysEnabled, graphController.FrameGraph);
            AddShortcutEntry(Actions.Rename, () => graphController.graphView.GetSelectedNodeCount() == 1, graphController.OnRename);
            AddShortcutEntry(Actions.Cut, DefaultEnableCheck, graphController.OnCut);
            AddShortcutEntry(Actions.Copy, DefaultEnableCheck, graphController.OnCopy);
            AddShortcutEntry(Actions.Paste, () => graphController.copyPasteHandler.HasNodes(), graphController.OnPaste);
            AddShortcutEntry(Actions.Duplicate, DefaultEnableCheck, graphController.OnDuplicate);
            AddShortcutEntry(Actions.Delete, () => graphController.graphView.HasSelectedEdges() || DefaultEnableCheck(), graphController.OnDelete);
        }
    }

}
