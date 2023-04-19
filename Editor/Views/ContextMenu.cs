using GraphViewBase;
using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace NewGraph {

    using static GraphSettingsSingleton;

    /// <summary>
    /// Main ContextMenu that is being used when performing a right-click in the graph view.
    /// This can be inherited by a custom class that has the [CustomContextMenu] attribute.
    /// Beware: This class inherits from GraphSearchWindowProvider and therefore is a ScriptableObject by nature.
    /// </summary>
    public class ContextMenu : GraphSearchWindowProvider {
        /// <summary>
        /// static lookup table that gathers unique labels for node types.
        /// </summary>
        protected static Dictionary<Type, string> nodeTypeToCreationLabel = new Dictionary<Type, string>();
        /// <summary>
        /// the graph controller this context menu operates on.
        /// </summary>
        protected GraphController graphController;

        /// <summary>
        /// Initialize should be called after the object is created via CreateInstance
        /// </summary>
        /// <param name="graphController">The graph controller this context menu should operate on.</param>
        public void Initialize(GraphController graphController) {
            this.graphController = graphController;
            Initialize(this.graphController.graphView.shortcutHandler);
        }

        /// <summary>
        /// Instantiate a custom or default context menu
        /// </summary>
        /// <param name="graphController">The graph controller this context menu should operate on.</param>
        /// <returns>The instance of the newly created context menu.</returns>
        public static ContextMenu CreateContextMenu(GraphController graphController) {
            // get all types that have the [CustomContextMenu] attribute
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomContextMenuAttribute>();
            ContextMenu menu;

            foreach (Type type in types) {
                // is the detected type actually inheriting from ContextMenu?
                if (type.ImplementsOrInherits(typeof(ContextMenu))) {
                    // create an instance of the custom ContextMenu type!
                    menu = CreateInstance(type) as ContextMenu;
                    menu.Initialize(graphController);
                    return menu;
                }
            }

            // no custom editor found or ineligible
            menu = CreateInstance<ContextMenu>();
            menu.Initialize(graphController);
            return menu;
        }

        /// <summary>
        /// Default check wether a menu entry should be enabled.
        /// </summary>
        /// <returns>Should the menu entry be enabled?</returns>
        public virtual bool DefaultEnabledCheck() {
            return graphController.graphView.GetSelectedNodeCount() > 0;
        }

        /// <summary>
        /// Default check for node entries and wether they should be enabled.
        /// </summary>
        /// <returns>Should the node entry be enabled?</returns>
        public virtual bool DefaultNodeEnabledCheck() {
            return graphController.graphData != null;
        }

        /// <summary>
        /// Header label
        /// </summary>
        /// <returns></returns>
        protected virtual string GetHeader() {
            return Settings.searchWindowRootHeader;
        }

        /// <summary>
        /// Creates/builds the context menu
        /// </summary>
        public virtual void BuildContextMenu() {
            StartAddingMenuEntries(GetHeader());
            AddNodeEntries();
            ResolveNodeEntries(DefaultNodeEnabledCheck);
            AddCommands();
        }

        /// <summary>
        /// Add all node entries.
        /// </summary>
        protected virtual void AddNodeEntries() {
            AddNodeEntriesDefault();
        }

        /// <summary>
        /// Default implementation to create all node entries.
        /// </summary>
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
                    if (!nodeTypeToCreationLabel.ContainsKey(nodeType)) {
                        nodeTypeToCreationLabel.Add(nodeType, createNodeLabel.Substring(1));
                    }
                    createNodeLabel = (!isUtilityNode ? Settings.createNodeLabel : Settings.createUtilityNodeLabel) + createNodeLabel;
                    AddNodeEntry(createNodeLabel, (obj) => graphController.CreateNewNode(nodeType, isUtilityNode));
                }
            }
        }

        /// <summary>
        /// Add all command panel entries
        /// </summary>
        protected virtual void AddCommands() {
            AddDefaultCommands();
        }

        /// <summary>
        /// defualt implementation of all commands
        /// </summary>
        protected void AddDefaultCommands() {
            AddSeparator(Settings.searchWindowCommandHeader);
            AddShortcutEntry(Actions.Frame, SearchTreeEntry.AlwaysEnabled, graphController.FrameGraph);
            AddShortcutEntry(Actions.Rename, () => graphController.graphView.GetSelectedNodeCount() == 1, graphController.OnRename);
            AddShortcutEntry(Actions.Cut, DefaultEnabledCheck, graphController.OnCut);
            AddShortcutEntry(Actions.Copy, DefaultEnabledCheck, graphController.OnCopy);
            AddShortcutEntry(Actions.Paste, () => graphController.copyPasteHandler.HasNodes(), graphController.OnPaste);
            AddShortcutEntry(Actions.Duplicate, DefaultEnabledCheck, graphController.OnDuplicate);
            AddShortcutEntry(Actions.Delete, () => graphController.graphView.HasSelectedEdges() || DefaultEnabledCheck(), graphController.OnDelete);
        }
    }

}
