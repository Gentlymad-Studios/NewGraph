using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {
    /// <summary>
    /// Inspector window where the main command menu is implemented.
    /// Also handles renaming of a graph, basic asset loading and displaying additional content.
    /// </summary>
    /// <typeparam name="T">The type of the graph data we want to operate on (needs to be based on BaseGraphData)</typeparam>
    public class InspectorController<T> where T:GraphModel {
        private ReactiveSettings reactiveSettings;

        private VisualElement commandPanel;
        private VisualElement inspectorRoot;
        private Button createButton, /*saveButton,*/ loadButton, homeButton, inspectorButton;
        private Label multipleNodesSelected;
        private VisualElement inspectorContainer;
        private VisualElement selectedNodeInspector;
        private VisualElement inspectorHeader;
        private Image inspectorButtonImage;

        private int currentPickerWindow;
        private bool pickerActive = false;
        private bool isInspectorVisible = true;

        public Action OnCreateButtonClicked, /*OnSaveClicked,*/ OnLoadClicked, OnHomeClicked;
        public Action<T> OnShouldLoadGraph;
        public Action<T> OnAfterGraphCreated;

        public InspectorController(VisualElement parent) {
            commandPanel = parent.Q<VisualElement>(nameof(commandPanel));
            inspectorRoot = parent.Q<VisualElement>(nameof(inspectorRoot));
            ReactiveSettings.Create(ref reactiveSettings, SettingsChanged);

            inspectorContainer = parent.Q<VisualElement>(nameof(inspectorContainer));

            createButton = commandPanel.Q<Button>(nameof(createButton));
            createButton.clicked += CreateButtonClicked;
            createButton.Add(GraphSettings.CreateButtonIcon);

            /*
            saveButton = commandPanel.Q<Button>(nameof(saveButton));
            saveButton.clicked += SaveButtonClicked;
            saveButton.Add(GraphSettings.SaveButtonIcon);
            */

            loadButton = commandPanel.Q<Button>(nameof(loadButton));
            loadButton.clicked += LoadButtonClicked;
            loadButton.Add(GraphSettings.LoadButtonIcon);

            homeButton = commandPanel.Q<Button>(nameof(homeButton));
            homeButton.clicked += HomeButtonClicked;
            homeButton.Add(GraphSettings.HomeButtonIcon);

            inspectorButton = commandPanel.Q<Button>(nameof(inspectorButton));
            inspectorButton.clicked += InspectorButtonClicked;
            inspectorButtonImage = new Image();
            inspectorButton.Add(inspectorButtonImage);
            inspectorHeader = inspectorRoot.Q<VisualElement>(nameof(inspectorHeader));

            Label startLabel = new Label(GraphSettings.Instance.noGraphLoadedLabel);
            startLabel.AddToClassList(nameof(startLabel));

            inspectorHeader.Add(startLabel);
            multipleNodesSelected = inspectorRoot.Q<Label>(nameof(multipleNodesSelected));
            multipleNodesSelected.text = "";

            SetSelectedNodeInfoActive();
            SetInspectorVisibility(EditorPrefs.GetBool(GraphSettings.isInspectorVisiblePrefsKey, isInspectorVisible));
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
                inspectorRoot.style.minWidth = inspectorRoot.style.maxWidth = GraphSettings.Instance.inspectorWidth;
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
                return SelectText(ref selectedCount, GraphSettings.Instance.node);
            }

            string DoEdgeText(ref int selectedCount) {
                return SelectText(ref selectedCount, GraphSettings.Instance.edge);
            }

            if (active && nodeCount == 0 && edgeCount == 0) {
                active = false;
            } else {
                if (nodeCount > 0 && edgeCount > 0) {
                    multipleNodesSelected.text = $"{DoNodeText(ref nodeCount)} and {DoEdgeText(ref edgeCount)} {GraphSettings.Instance.multipleSelectedMessagePartial}";
                } else {
                    multipleNodesSelected.text = $"{(nodeCount > 0 ? DoNodeText(ref nodeCount) : DoEdgeText(ref edgeCount))} {GraphSettings.Instance.multipleSelectedMessagePartial}";
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
                inspectorRoot.style.minWidth = inspectorRoot.style.maxWidth = GraphSettings.Instance.inspectorWidth;
            }
        }
         
        /// <summary>
        /// called when the create button was clicked.
        /// </summary>
        private void CreateButtonClicked() {
            OnCreateButtonClicked?.Invoke();

            string fileEnding = "asset";

            // retrieve the last opened folder
            string lastOpenedDirectoryGUID = EditorPrefs.GetString(GraphSettings.lastOpenedDirectoryPrefsKey, null);
            string lastFolder = "";
            if (lastOpenedDirectoryGUID != null) {
                lastFolder = AssetDatabase.GUIDToAssetPath(lastOpenedDirectoryGUID);
                if (lastFolder == null) {
                    lastFolder = "";
                }
            }

            // generate a unique filename that avoids conflicts
            string uniqueFilename = Path.Combine(lastFolder, typeof(T).Name + "."+ fileEnding);
            if (File.Exists(uniqueFilename)) {
                uniqueFilename = AssetDatabase.GenerateUniqueAssetPath(uniqueFilename);
            }
            uniqueFilename = Path.GetFileName(uniqueFilename);

            // show the dialog
            string path = EditorUtility.SaveFilePanel("Create New Graph", lastFolder, uniqueFilename, fileEnding);
            if (path.Length != 0) {
                T graphData = ScriptableObject.CreateInstance<T>();
                path= path.Substring(Application.dataPath.Length-6);

                // save the last selcted folder
                string folder = AssetDatabase.AssetPathToGUID(Path.GetDirectoryName(path));
                EditorPrefs.SetString(GraphSettings.lastOpenedDirectoryPrefsKey, folder);

                AssetDatabase.CreateAsset(graphData, path);
                AssetDatabase.SaveAssets();
                CreateRenameGraphUI(graphData);
                Clear();

                OnAfterGraphCreated?.Invoke(graphData);
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
        public void CreateRenameGraphUI(T graph) {

            // avoid some redudancy when setting string property...
            void SetStringValueAndApply(SerializedProperty prop, string otherValue) {
                prop.stringValue = otherValue;
                prop.serializedObject.ApplyModifiedProperties();
            }

            // make sure we do have a serializedobject setup
            if (graph.serializedGraphData == null) {
                graph.CreateSerializedObject();
            }

            // clear all former ui elements
            inspectorHeader.Unbind();
            inspectorHeader.Clear();

            // retrieve the "real" name of the scriptable object;
            graph.serializedGraphData.Update();
            SerializedProperty assetNameProperty = graph.serializedGraphData.FindProperty("m_Name").Copy();

            // get our "fake" property that is just there to avoid overwriting the "real" name which Unity really doesn't like...
            SerializedProperty tmpNameProperty = graph.serializedGraphData.FindProperty(nameof(tmpNameProperty)).Copy();
            SetStringValueAndApply(tmpNameProperty, assetNameProperty.stringValue);
            
            // create the property field our EditableLabelElement will operate on
            PropertyField labelField = new PropertyField(tmpNameProperty);
            inspectorHeader.Add(labelField);
            inspectorHeader.Bind(graph.serializedGraphData);

            // create the label and start the rename procedure when we exited the label edit mode
            EditableLabelElement editableLabel = new EditableLabelElement(labelField, onEditModeLeft: () => {
                string graphAssetPath = AssetDatabase.GetAssetPath(graph);
                string originalName = Path.GetFileNameWithoutExtension(graphAssetPath);
                string name = tmpNameProperty.stringValue;

                // if the graph is an actual asset and the name has changed..
                if (graphAssetPath != null && originalName != name) {
                    // check if we were dealt a shit hand... (empty string or invalid characters)
                    if (!string.IsNullOrWhiteSpace(name) && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) {
                        // get the directory of the path
                        string pathToDirectory = Path.GetDirectoryName(graphAssetPath);
                        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(pathToDirectory, name));
                        string uniqueFilename = Path.GetFileNameWithoutExtension(uniquePath);

                        // GenerateUniqueAssetPath will handle duplicates so lets inform the user that adjustments were made!
                        if (name != uniqueFilename) {
                            Logger.LogAlways($"The provided name was already present or not valid! Changing {name} to {uniqueFilename}!");
                        }

                        // update our fake property, rename the real asset, refresh the database
                        SetStringValueAndApply(tmpNameProperty, uniqueFilename);
                        AssetDatabase.RenameAsset(graphAssetPath, uniqueFilename);
                        AssetDatabase.Refresh();
                        return;
                    }

                    // we were dealt a shit hand, so lets revert the name back to the original and inform the user!
                    Logger.LogAlways($"The provided name ({name}) was empty or not a valid filename!");
                    SetStringValueAndApply(tmpNameProperty, originalName);
                }
            });
        }

        /// <summary>
        /// Called when the home button was clicked.
        /// </summary>
        private void HomeButtonClicked() {
            OnHomeClicked?.Invoke();
        }

        /*
        /// <summary>
        /// Called when the save button was clicked.
        /// </summary>
        private void SaveButtonClicked() {
            OnSaveClicked?.Invoke();
        }
        */

        /// <summary>
        /// Called when the load button was clicked.
        /// </summary>
        private void LoadButtonClicked() {
            currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<T>(null, false, "", currentPickerWindow);
            pickerActive = true;
            OnLoadClicked?.Invoke();
        }

        /// <summary>
        /// IMGUI draw method to display the object picker popup
        /// </summary>
        public void Draw() {
            if (pickerActive && Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow) {
                UnityEngine.Object pickedObj = EditorGUIUtility.GetObjectPickerObject();
                if (pickedObj != null) {
                    T graph = EditorGUIUtility.GetObjectPickerObject() as T;
                    CreateRenameGraphUI(graph);
                    Clear();
                    OnShouldLoadGraph?.Invoke(graph);
                }
                pickerActive = false;
            }
        }

    }
}
