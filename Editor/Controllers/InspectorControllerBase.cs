using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {
    /// <summary>
    /// Inspector window where the main command menu is implemented.
    /// Also handles renaming of a graph, basic asset loading and displaying additional content.
    /// </summary>
    public abstract class InspectorControllerBase {
        private ReactiveSettings reactiveSettings;

        private VisualElement commandPanel;
        private VisualElement inspectorRoot;
        private Label multipleNodesSelected;
        private VisualElement inspectorContainer;
        private VisualElement selectedNodeInspector;
        protected VisualElement inspectorHeader;
        private Image inspectorButtonImage;

        protected int currentPickerWindow;
        protected bool pickerActive = false;
        private bool isInspectorVisible = true;

        public Action<IGraphModelData> OnShouldLoadGraph;
        public Action<IGraphModelData> OnAfterGraphCreated;
        public Action OnHomeClicked;

        public InspectorControllerBase(VisualElement parent) {

            commandPanel = parent.Q<VisualElement>(nameof(commandPanel));
            inspectorRoot = parent.Q<VisualElement>(nameof(inspectorRoot));
            ReactiveSettings.Create(ref reactiveSettings, SettingsChanged);

            inspectorContainer = parent.Q<VisualElement>(nameof(inspectorContainer));
            
            Button createButton = commandPanel.Q<Button>(nameof(createButton));
            SetupCreateButton(createButton);

            Button loadButton = commandPanel.Q<Button>(nameof(loadButton));
            SetupLoadButton(loadButton);
            
            Button homeButton;
            homeButton = commandPanel.Q<Button>(nameof(homeButton));
            homeButton.clicked+= HomeButtonClicked;
            homeButton.Add(GraphSettings.HomeButtonIcon);

            Button inspectorButton;
            inspectorButton = commandPanel.Q<Button>(nameof(inspectorButton));
            inspectorButton.clicked += InspectorButtonClicked;
            inspectorButtonImage = new Image();
            inspectorButton.Add(inspectorButtonImage);
            inspectorHeader = inspectorRoot.Q<VisualElement>(nameof(inspectorHeader));

            Label startLabel = new Label(Settings.noGraphLoadedLabel);
            startLabel.AddToClassList(nameof(startLabel));

            inspectorHeader.Add(startLabel);
            multipleNodesSelected = inspectorRoot.Q<Label>(nameof(multipleNodesSelected));
            multipleNodesSelected.text = "";

            SetSelectedNodeInfoActive();
            SetInspectorVisibility(EditorPrefs.GetBool(GraphSettings.isInspectorVisiblePrefsKey, isInspectorVisible));
        }

        public abstract void SetupCreateButton(Button createButton);

        public abstract void SetupLoadButton(Button loadButton);
        
        /// <summary>
        /// Called when the home button was clicked.
        /// </summary>
        private void HomeButtonClicked() {
            OnHomeClicked?.Invoke();
        }

        private void SetInspectorVisibility(bool visible=true) {
            if (visible != isInspectorVisible) {
                EditorPrefs.SetBool(GraphSettings.isInspectorVisiblePrefsKey, visible);
            }

            isInspectorVisible = visible;
            if (!isInspectorVisible) {
                inspectorContainer.style.visibility = Visibility.Hidden;
                inspectorHeader.style.visibility = Visibility.Hidden;
                inspectorRoot.style.minWidth = inspectorRoot.style.maxWidth = 0;
                inspectorButtonImage.image = GraphSettings.ShowInspectorIcon;
            } else {
                inspectorContainer.style.visibility = Visibility.Visible;
                inspectorHeader.style.visibility = Visibility.Visible;
                inspectorRoot.style.minWidth = inspectorRoot.style.maxWidth = Settings.inspectorWidth;
                inspectorButtonImage.image = GraphSettings.HideInspectorIcon;
            }
        }

        private void InspectorButtonClicked() {
            SetInspectorVisibility(!isInspectorVisible);
        }

        /// <summary>
        /// Show/Hide a label with info about the count of the currently selected nodes & edges.
        /// </summary>
        /// <param name="nodeCount">count of selected nodes</param>
        /// <param name="edgeCount">count of selected edges</param>
        /// <param name="active">wether to hide or show this</param>
        public void SetSelectedNodeInfoActive(int nodeCount=0, int edgeCount=0, bool active=true) {
            string SelectText(ref int selectedCount, string baseName) {
                return $"<b>{selectedCount} {baseName}{(selectedCount > 1 ? "s" : "")}</b>";
            }

            string DoNodeText(ref int selectedCount) {
                return SelectText(ref selectedCount, Settings.node);
            }

            string DoEdgeText(ref int selectedCount) {
                return SelectText(ref selectedCount, Settings.edge);
            }

            if (active && nodeCount == 0 && edgeCount == 0) {
                active = false;
            } else {
                if (nodeCount > 0 && edgeCount > 0) {
                    multipleNodesSelected.text = $"{DoNodeText(ref nodeCount)} and {DoEdgeText(ref edgeCount)} {Settings.multipleSelectedMessagePartial}";
                } else {
                    multipleNodesSelected.text = $"{(nodeCount > 0 ? DoNodeText(ref nodeCount) : DoEdgeText(ref edgeCount))} {Settings.multipleSelectedMessagePartial}";
                }
            }

            multipleNodesSelected.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Sets the a actual inspector content.
        /// </summary>
        /// <param name="content">The content to display</param>
        /// <param name="serializedObject">The Serialized object that should be used for binding.</param>
        public void SetInspectorContent(VisualElement content, SerializedObject serializedObject=null) {
            // release the previous inspector content into the ether of unknown Unity land...
            if (selectedNodeInspector != null) {
                selectedNodeInspector.Unbind();
                selectedNodeInspector.RemoveFromHierarchy();
            }
            // set the content that should be displayed as a reference...
            selectedNodeInspector = content;

            if (content != null) {
                // actually add the content to the container of this inspector...
                inspectorContainer.Add(selectedNodeInspector);

                // in case we were supplied with a serialized object do the bind operation!
                if (serializedObject != null) {
                    selectedNodeInspector.Bind(serializedObject);
                }
            }

        }

        /// <summary>
        /// Adjust visual styling when setting shave changed.
        /// </summary>
        private void SettingsChanged() {
            if (isInspectorVisible) {
                inspectorRoot.style.minWidth = inspectorRoot.style.maxWidth = Settings.inspectorWidth;
            }
        }

        /// <summary>
        /// Resets the current insepctor content.
        /// </summary>
        public void Clear() {
            SetInspectorContent(null);
        }

        /// <summary>
        /// Implement renaming of the current graph.
        /// This create an EditableLabelElement so renaming UX is consistent.
        /// </summary>
        /// <param name="graph">The graph that renaming should operate on</param>
        public abstract void CreateRenameGraphUI(IGraphModelData graph);

        /// <summary>
        /// IMGUI draw method to display the object picker popup
        /// </summary>
        public void Draw() {
            if (pickerActive && Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow) {
                UnityEngine.Object graph = EditorGUIUtility.GetObjectPickerObject();
                if (graph != null) {
                    IGraphModelData graphModelData = graph as IGraphModelData;
                    graphModelData.CreateSerializedObject();
                    CreateRenameGraphUI(graphModelData);
                    Clear();
                    OnShouldLoadGraph?.Invoke(graphModelData);
                }
                pickerActive = false;
            }
        }

    }
}
