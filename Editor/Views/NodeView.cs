using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using GraphViewBase;

namespace NewGraph {

    public class NodeView : BaseNode {

        public VisualElement inspectorContent;
        private ReactiveSettings reactiveSettings;
        private List<EditableLabelElement> editableLabels = new List<EditableLabelElement>();
        public Color nodeColor;
        private bool hasNodeViewProperty = false;
        private bool hasInspectorProperty = false;
        
        public NodeController controller;
        public PortView inputPort = null;
        public List<PortView> outputPorts = new List<PortView>();
        public List<PortListView> portLists = new List<PortListView>();

        public NodeView(NodeController controller, Color nodeColor) {
            this.controller = controller;
            this.nodeColor = nodeColor;
        }

        private void ColorizeBackground() {
            if (nodeColor != default) {
                style.backgroundColor = nodeColor;
            } else {
                style.backgroundColor = GraphSettings.Instance.defaultNodeColor;
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
                if (ExtensionContainer.childCount > 0 && !hasNodeViewProperty) {
                    ExtensionContainer[0].style.display = DisplayStyle.None;
                }
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

            BindUI(controller.GetSerializedObject());
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
            PortView port = new PortView(info, property);

            port.PortName = info.portName == null ? (property != null ? property.displayName : info.fieldType.Name) : info.portName;
            AddPort(port);
            return port;
        }

        private void CreatePropertyUI(VisualElement[] groupParents, GraphPropertyInfo propertyInfo, SerializedProperty property) {
            void Create(VisualElement groupParent, Editability edtability) {
                /*
                foreach (HeaderAttribute headerInfo in propertyInfo.headers) {
                    Label header = new Label { text = headerInfo.header };
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    groupParent.Add(header);
                }*/

                /*
                for (int i = 0; i < propertyInfo.spacesCount; i++) {
                    groupParent.Add(new Label { text = " " });
                }*/

                PropertyField propertyField = CreatePropertyField(property);
                SetupPropertyField(propertyField, propertyInfo, edtability);
                groupParent.Add(propertyField);
            }

            if (!controller.nodeItem.isUtilityNode && propertyInfo.graphDisplay.displayType.HasFlag(DisplayType.NodeView)) {
                hasNodeViewProperty = true;
                Create(groupParents[0], Editability.NodeView);
            }

            if (propertyInfo.graphDisplay.displayType.HasFlag(DisplayType.Inspector)) {
                hasInspectorProperty= true;
                Create(groupParents[groupParents.Length-1], Editability.Inspector);
            }
        }

        private VisualElement[] CreateGroupUI(GroupInfo groupInfo, VisualElement[] parents, SerializedProperty property) {
            SerializedProperty prop = property.Copy();
            VisualElement[] newGroups = new VisualElement[parents.Length];
            for (int i=0; i<newGroups.Length; i++) {
                Foldout newGroup = new Foldout();
                newGroup.AddToClassList(nameof(GroupInfo));
                newGroup.text = groupInfo.groupName;
                newGroup.name = groupInfo.relativePropertyPath;
                newGroup.bindingPath = prop.propertyPath;
                newGroup.name = "unity-foldout-" + prop.propertyPath;

                // add label/ foldout etc.
                newGroups[i] = newGroup;
                parents[i].Add(newGroups[i]);
            }
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
            //inspectorContent.Bind(serializedObject);
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
            style.width = GraphSettings.Instance.nodeWidth;
        }
    }
}
