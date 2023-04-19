using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace NewGraph {

    using static GraphSettingsSingleton;

    public class PortListView : ListView {
        public List<PortView> ports = new List<PortView>();
        public SerializedProperty listProperty;
        private NodeView nodeView;
        public PortInfo portInfo;
        private VisualElement container;

        public PortListView(SerializedProperty listProperty, PortInfo portListInfo, NodeView nodeView, VisualElement container, int index=-1) {

            this.nodeView = nodeView;
            this.listProperty = listProperty.Copy();
            this.container = container;
            this.portInfo= portListInfo;

            Label staticHeader = new Label(portListInfo.fieldName);
            staticHeader.AddToClassList(nameof(staticHeader));
            hierarchy.Add(staticHeader);
            staticHeader.SendToBack();

            // Set Flags
            reorderable = true;
            showAddRemoveFooter = true;
            showBorder = true;
            showBoundCollectionSize = false;
            showFoldoutHeader = false;
            showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            headerTitle = listProperty.displayName;
            bindingPath = listProperty.propertyPath;

            // FixedHeight is faster but doesn't play well with foldouts!
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            // animated is a mess in conjunction with dragging ports
            reorderMode = ListViewReorderMode.Simple;

            // style changes
            // Your ListView needs to take all the remaining space
            style.flexGrow = 1;

            // hook up internal callback methods
            makeItem = MakeItem;
            bindItem = BindItem;

            itemIndexChanged -= ItemIndexChanged;
            itemIndexChanged += ItemIndexChanged;

            // overwrite default button behaviors due to bugs in ListView code
            Button removeButton = this.Q<Button>("unity-list-view__remove-button");
            removeButton.clickable = null;
            removeButton.clicked += OnRemoveClicked;
            Button addButton = this.Q<Button>("unity-list-view__add-button");
            addButton.clickable = null;
            addButton.clicked += OnAddClicked;

            // add ourselves to the visual tree
            if (index >= 0) {
                container.Insert(index, this);
            } else {
                container.Add(this);
            }

        }

        private void OnAddClicked() {
            listProperty.arraySize++;
            listProperty.serializedObject.ApplyModifiedProperties();

            nodeView.RebuildPortListView(this);
        }

        private void OnItemsRemoved(IEnumerable<int> indices) {
            /*
            foreach (var index in indices) {
                PortView port = ports[index];

                GraphViewBase.BaseEdge[] edges = port.Connections.ToArray();
                for (int i = edges.Length - 1; i >= 0; i--) {
                    edges[i].Disconnect();
                }

                Debug.Log("disconnect");
                ports.Remove(port);
            }*/
            
            for (int i= ports.Count-1; i>=0; i--) {
                PortView port = ports[i];
                GraphViewBase.BaseEdge[] edges = port.Connections.ToArray();
                for (int j = edges.Length - 1; j >= 0; j--) {
                    edges[j].Disconnect();
                }

                ports.Remove(port);
            }

            nodeView.RebuildPortListView(this);
        }

        /// <summary>
        /// We need to re-implement this basic behavior because unity's default behavior for this has a bug...
        /// </summary>
        private void OnRemoveClicked() {
            List<int> indices = new List<int>(selectedIndices);
            indices.Sort();

            listProperty.serializedObject.Update();
            if (indices.Any()) {
                for (var i = indices.Count - 1; i >= 0; i--) {
                    int index = indices[i];
                    if (index >= 0 && index < listProperty.arraySize) {
                        listProperty.DeleteArrayElementAtIndex(index);

                        if (index < listProperty.arraySize - 1) {
                            var currentProperty = listProperty.GetArrayElementAtIndex(index);
                            for (var j = index + 1; j < listProperty.arraySize; j++) {
                                var nextProperty = listProperty.GetArrayElementAtIndex(j);
                                if (nextProperty != null) {
                                    currentProperty.isExpanded = nextProperty.isExpanded;
                                    currentProperty = nextProperty;
                                }
                            }
                        }
                    }
                }
                ClearSelection();
            } else if (listProperty.arraySize > 0) {
                var index = listProperty.arraySize - 1;
                viewController.RemoveItem(index);
                indices.Add(index);
            }

            if (indices.Any()) {
                listProperty.serializedObject.ApplyModifiedProperties();
            }
            OnItemsRemoved(indices);
        }

        private VisualElement MakeItem() {
            VisualElement itemRow = new VisualElement();
            itemRow.style.flexDirection = FlexDirection.Row;

            if (reorderable) {
                // add handlebars with hand symbol to imply dragging support
                itemRow.Add(GraphSettings.CreateHandleBars());
            }

            // create a label that gives some indication about the 
            Label fieldLabel = new Label();
            fieldLabel.AddToClassList(nameof(fieldLabel));
            itemRow.Add(fieldLabel);

            PortView port = new PortView(portInfo, null);
            ports.Add(port);
            port.SetParent(nodeView);
            itemRow.Add(port);
            port.BringToFront();
            return itemRow;
        }

        private int AdjustIfReorderable(int requestedIndex) {
            if (reorderable) {
                requestedIndex++;
            }
            return requestedIndex;
        }

        private void BindItem(VisualElement rowItem, int i) {
            PortView port;

            if (i>=ports.Count) {
                if (AdjustIfReorderable(1) < rowItem.childCount) {
                    port = rowItem[AdjustIfReorderable(1)] as PortView;
                    ports.Add(port);
                } else {
                    port = new PortView(portInfo, null);
                    ports.Add(port);
                    port.SetParent(nodeView);
                    rowItem.Add(port);
                    port.BringToFront();
                }
            } else {
                port = ports[i];
            }

            SerializedProperty prop = listProperty.GetArrayElementAtIndex(i);
            Label label = rowItem[AdjustIfReorderable(0)] as Label;

            // remove port from hierachy
            if (rowItem.hierarchy.childCount == AdjustIfReorderable(2)) {
                rowItem.hierarchy.RemoveAt(AdjustIfReorderable(1));
            }

            // re-add the correct port (important to visually adjust after reordering)
            rowItem.hierarchy.Add(port);

            // send the property over to the port so it can operate on it
            port.boundProperty = prop.Copy();

            // we want to update the label in case the connection changed
            // so we hookup to the connection changed action of the port
            void OnConnectionChanged() {
                if (prop.managedReferenceValue == null) {
                    label.text = Settings.defaultLabelForPortListItems;
                } else {
                    // we need to get the foreign nodeview
                    NodeView attachedNodeView = nodeView.controller.graphController.GetNodeView(prop.managedReferenceValue);
                    if (attachedNodeView != null) {
                        //...and retrieve its name, hihi
                        label.text = attachedNodeView.controller.nodeItem.GetName();
                        label.TrackPropertyValue(attachedNodeView.controller.nodeItem.GetNameSerializedProperty().Copy(), (p) => label.text = p.stringValue);
                    }
                }
            }
            port.SetConnectionChangedCallback(OnConnectionChanged);

            // create visible connection
            RedrawConnection(port, prop);

            // force a change event so edges are redrawn
            container.style.display = DisplayStyle.None;
            void PortGeomChanged(GeometryChangedEvent e) {
                container.style.display = DisplayStyle.Flex;
                container.UnregisterCallback<GeometryChangedEvent>(PortGeomChanged);
            }
            container.RegisterCallback<GeometryChangedEvent>(PortGeomChanged);
        }

        private void RedrawConnection(PortView port, SerializedProperty prop) {
            if (prop.managedReferenceValue != null) {
                NodeView otherNode= nodeView.controller.graphController.GetNodeView(prop.managedReferenceValue);
                if (otherNode != null && !port.IsConnectedTo(otherNode.inputPort)) {
                    nodeView.controller.graphController.ConnectPorts(port, otherNode.inputPort);
                }
            }
        }

        private void ItemIndexChanged(int draggedIndex, int dropIndex) {
            // get "real" indices
            draggedIndex = viewController.GetIndexForId(draggedIndex);
            dropIndex = viewController.GetIndexForId(dropIndex);

            // reorder ports list
            PortView item = ports[draggedIndex];
            ports.RemoveAt(draggedIndex);
            ports.Insert(dropIndex, item);
        }
    }
}
