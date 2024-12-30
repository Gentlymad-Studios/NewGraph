using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using GraphViewBase;

namespace NewGraph {

    using static GraphSettingsSingleton;

    public class NodeView : BaseNode {

        public VisualElement inspectorContent;
        private ReactiveSettings reactiveSettings;
        public List<EditableLabelElement> editableLabels = new List<EditableLabelElement>();
        public Color nodeColor;
        public Color labelColor;
        public bool shouldSetBackgroundColor = true;
        private bool hasInspectorProperty = false;

        public NodeController controller;
        public PortView inputPort = null;
        public List<PortView> outputPorts = new List<PortView>();
        public List<PortListView> portLists = new List<PortListView>();
        public List<Foldout> foldouts = new List<Foldout>();

        public NodeView(NodeController controller, Color nodeColor, Color labelColor) {
            this.controller = controller;
            this.nodeColor = nodeColor;
            this.labelColor = labelColor;
        }

        private void ColorizeBackground() {
            if (shouldSetBackgroundColor && nodeColor != default) {
                style.backgroundColor = nodeColor;
            } else {
                style.backgroundColor = Settings.defaultNodeColor;
            }
        }

        private void ColorizeLabels()
        {
            foreach (var field in controller.nodeView.ExtensionContainer.Children())
            {
                Label label = field.Q<Label>(className: "unity-base-field__label");
                if (label != null)
                    label.style.color = labelColor;
            }
        }

        public void InitializeView() {

            editableLabels.Clear();

            //ReactiveSettings.Create(ref reactiveSettings, SettingsChanged);
            SettingsChanged();

            Vector2 position = controller.GetStartPosition();
            SetPosition(position);

            if (!controller.nodeItem.isUtilityNode) {
                ColorizeBackground();

                inspectorContent = new VisualElement();
                controller.DoForNameProperty(CreateLabelUI);
                controller.DoForEachPortProperty(CreateOuputPortUI);
                controller.DoForInputPortProperty(CreateInputPortUI);
                controller.DoForEachPortListProperty(CreatePortListUI);
                controller.DoForEachPropertyOrGroup(new[] { ExtensionContainer, inspectorContent }, CreateGroupUI, CreatePropertyUI);

                // hide empty groups
                if (inspectorContent.childCount > 1 && !hasInspectorProperty) {
                    inspectorContent[1].style.display = DisplayStyle.None;
                }

            } else {
                IUtilityNode utilityNode = controller.nodeItem.nodeData as IUtilityNode;
                if (utilityNode.ShouldColorizeBackground()) {
                    ColorizeBackground();
                }

                if (utilityNode.CreateNameUI()) {
                    controller.DoForNameProperty(CreateLabelUI);
                }

                if (utilityNode.CreateInspectorUI()) {
                    inspectorContent = new VisualElement();
                    controller.DoForEachPropertyOrGroup(new[] { ExtensionContainer, inspectorContent }, CreateGroupUI, CreatePropertyUI);
                }

                (controller.nodeItem.nodeData as IUtilityNode).Initialize(controller);
            }

            controller.nodeItem.CleanupFoldoutStates();

            BindUI(controller.GetSerializedObject());

            ColorizeLabels();
        }

        public void RebuildPortListView(PortListView view) {
            SerializedProperty prop = view.listProperty;
            PortInfo info = view.portInfo;
            view.Unbind();
            view.RemoveFromHierarchy();

            int index = portLists.IndexOf(view);
            portLists[index] = null;
            view = new PortListView(prop, info, this, ExtensionContainer, index);
            portLists[index] = view;
            view.Bind(controller.GetSerializedObject());
        }

        private void CreatePortListUI(PortInfo info, SerializedProperty property) {
            portLists.Add(new PortListView(property, info, this, ExtensionContainer));
        }

        private void CreateInputPortUI(PortInfo info, SerializedProperty property) {
            inputPort = CreatePortUI(info, property);
        }

        private void CreateOuputPortUI(PortInfo info, SerializedProperty property) {
            outputPorts.Add(CreatePortUI(info, property));
        }

        public PortView CreatePortUI(PortInfo info, SerializedProperty property) {
            PortView port = new PortView(info, property.Copy());

            if (info.portDisplay.name != null) {
                port.PortName = info.portDisplay.name;
            } else {
                if (info.fieldName == null) {
                    if (property != null) {
                        port.PortName = property.displayName;
                    } else {
                        port.PortName = info.fieldType.Name;
                    }
                } else {
                    port.PortName = info.fieldName;
                }
            }

            AddPort(port);
            return port;
        }

        private void CreatePropertyUI(VisualElement[] groupParents, GraphPropertyInfo propertyInfo, SerializedProperty property) {
            PropertyField Create(VisualElement groupParent, Editability edtability) {
                PropertyField propertyField = CreatePropertyField(property);
                SetupPropertyField(propertyField, propertyInfo, edtability);

                groupParent.Add(propertyField);
                return propertyField;
            }

            PropertyField nodeViewPropField = null;
            PropertyField inspectorPropField = null;
            
            if (groupParents[0] != null && !controller.nodeItem.isUtilityNode && propertyInfo.graphDisplay.displayType.HasFlag(DisplayType.NodeView)) {
                nodeViewPropField = Create(groupParents[0], Editability.NodeView);
            }

            if (groupParents[1] != null && propertyInfo.graphDisplay.displayType.HasFlag(DisplayType.Inspector)) {
                hasInspectorProperty= true;
                inspectorPropField = Create(groupParents[1], Editability.Inspector);
            }

            // workaround for value change disconnection bug, this can only happen if we have an inspector & and a nodeview together
            if (inspectorPropField != null && nodeViewPropField != null) {
                bool inspectorChanged = false;
                bool nodeViewChanged = false;

                nodeViewPropField.RegisterValueChangeCallback((evt) => {
                    if (!inspectorChanged) {
                        inspectorPropField.Unbind();
                        inspectorPropField.BindProperty(property);
                        nodeViewChanged = true;
                    }
                    inspectorChanged = false;
                });

                inspectorPropField.RegisterValueChangeCallback((evt) => {
                    if (!nodeViewChanged) {
                        nodeViewPropField.Unbind();
                        nodeViewPropField.BindProperty(property);
                        inspectorChanged = true;
                    }
                    nodeViewChanged = false;
                });
            }
        }


        private VisualElement[] CreateGroupUI(GroupInfo groupInfo, VisualElement[] parents, SerializedProperty property) {
            VisualElement[] newGroups = new VisualElement[parents.Length];

            // handle special cases where we have an embedded managed reference that is within a group
            // normally we would have double header labels, so to be able to hide them via styling we need to add some classes.
            void HandleEmbeddedReferenceCase(VisualElement foldoutContent) {
                PropertyField propertyField = foldoutContent.Q<PropertyField>();
                if (propertyField != null) {
                    // get the containing foldout
                    Foldout foldout = propertyField.Q<Foldout>();
                    if (foldout != null) {
                        // make sure to force the foldoutvalue to stay open
                        foldout.value = true;
                        foldout.RegisterValueChangedCallback((evt) => {
                            if (evt.newValue != true) {
                                foldout.value = true;
                            }
                        });
                        // get the toggle
                        Toggle toggle = foldout.Q<Toggle>();
                        if (toggle != null) {
                            // we found everything we need so tag classes
                            foldout.AddToClassList("managedReference");
                            toggle.AddToClassList(nameof(groupInfo.hasEmbeddedManagedReference));
                        }
                    }
                }
            }

            // handle our custom foldout state for a group
            void HandleFoldoutState(Foldout newGroup, int index) {
                int propertyPathHash = newGroup.name.GetHashCode();
                NodeModel.FoldoutState foldOutState = controller.nodeItem.GetOrCreateFoldout(propertyPathHash);
                foldOutState.used = true;
                newGroup.value = foldOutState.isExpanded;
                newGroup.RegisterValueChangedCallback((evt) => {
                    foldOutState.isExpanded = evt.newValue;
                    controller.GetSerializedObject().ApplyModifiedPropertiesWithoutUndo();
                });
                newGroups[index] = newGroup;
                foldouts.Add(newGroup);
            }

            void AddAtIndex(int index, bool empty, string prefix) {
                // add label/ foldout etc.
                if (!empty) {
                    Foldout newGroup = new Foldout();
                    newGroup.pickingMode = PickingMode.Ignore;
                    newGroup.AddToClassList(nameof(GroupInfo));
                    newGroup.text = groupInfo.groupName;
                    newGroup.name = prefix + groupInfo.relativePropertyPath;

                    // handle our custom foldout state
                    HandleFoldoutState(newGroup, index);

                    // wait until the visual tree was built
                    void GeomChanged(GeometryChangedEvent _) {
                        VisualElement unityContent = newGroup.Q<VisualElement>("unity-content");

                        // check if we need to apply special behavior for managed references
                        if (groupInfo.hasEmbeddedManagedReference) {
                            HandleEmbeddedReferenceCase(unityContent);
                        }

                        // make sure to disable, that the ui captures input an prevents panning
                        unityContent.pickingMode = PickingMode.Ignore;
                        
                        // toss the callback away, since we are done
                        newGroup.UnregisterCallback<GeometryChangedEvent>(GeomChanged);
                    }
                    newGroup.RegisterCallback<GeometryChangedEvent>(GeomChanged);

                } else {
                    newGroups[index] = null;
                }
                if (parents[index] != null) {
                    parents[index].Add(newGroups[index]);
                }
            }

            bool noNodeView = true;
            if (groupInfo.graphDisplay.displayType.HasFlag(DisplayType.NodeView)) {
                noNodeView = false;
            }
            AddAtIndex(0, noNodeView, nameof(DisplayType.NodeView));

            bool noInspector = true;
            if (groupInfo.graphDisplay.displayType.HasFlag(DisplayType.Inspector)) {
                noInspector = false;
            }
            AddAtIndex(1, noInspector, nameof(DisplayType.Inspector));

            return newGroups;
        }

        private void CreateLabelUI(SerializedProperty property) {

            // Add label to title Container
            PropertyField propertyField = CreatePropertyField(property);
            editableLabels.Add(new EditableLabelElement(propertyField));
            TitleContainer.Add(propertyField);

            // Add label to inspector
            if (inspectorContent != null) {
                PropertyField propertyFieldInspector = CreatePropertyField(property);
                editableLabels.Add(new EditableLabelElement(propertyFieldInspector));
                inspectorContent.Add(propertyFieldInspector);
            }
        }

        private void BindUI(SerializedObject serializedObject) {
            this.Bind(serializedObject);
        }


        private void SetupPropertyField(VisualElement propertyField, GraphPropertyInfo propertyInfo, Editability editability) {
            if (!propertyInfo.graphDisplay.editability.HasFlag(editability)) {
                propertyField.SetEnabled(false);
            }
        } 

        private PropertyField CreatePropertyField(SerializedProperty property) {
            PropertyField propertyField = new PropertyField(property.Copy()) {
                name = property.name,
                bindingPath = property.propertyPath,
            };

            return propertyField;
        }

        public override void SetPosition(Vector2 newPosition) {
            base.SetPosition(newPosition);
            controller.SetPosition(newPosition.x, newPosition.y);
            
        }

        public void SetInspectorActive(bool active=true) {
            if (!active) {
                foreach (EditableLabelElement editableLabel in editableLabels) {
                    editableLabel.EnableInput(false);
                }
            }
            inspectorContent.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public VisualElement GetInspectorContent() {
            return inspectorContent;
        }

        private void SettingsChanged() {
            style.width = Settings.nodeWidth;
        }
    }
}
