using GraphViewBase;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    using static GraphSettings;

    public class GraphController {

		public event Action<IGraphModelData> OnGraphLoaded;
		public event Action<IGraphModelData> OnBeforeGraphLoaded;

		public IGraphModelData graphData = null;
        public CopyPasteHandler copyPasteHandler = new CopyPasteHandler();
        public GraphView graphView;

        private bool isLoading = false;
        private InspectorControllerBase inspector;
        private ContextMenu contextMenu;
        private EdgeDropMenu edgeDropMenu;
        private Dictionary<Actions, Action<object>> internalActions;
        private Dictionary<object, NodeView> dataToViewLookup = new Dictionary<object, NodeView>();

        public Vector2 GetViewScale() {
            return graphView.GetCurrentScale();
        } 

        public void ForEachNode(Action<BaseNode> callback) {
            graphView.ForEachNodeDo(callback);
        }

        public GraphController(VisualElement uxmlRoot, VisualElement root, Type inspectorType) {
            graphView = new GraphView(uxmlRoot, root, OnGraphAction);
            graphView.OnViewTransformChanged -= OnViewportChanged;
            graphView.OnViewTransformChanged += OnViewportChanged;
            graphView.OnMouseDown -= OpenContextMenu;
            graphView.OnMouseDown += OpenContextMenu;

            inspector = Activator.CreateInstance(inspectorType, uxmlRoot) as InspectorControllerBase;  
            inspector.OnShouldLoadGraph = OnShouldLoadGraph;
            inspector.OnAfterGraphCreated = OnAfterGraphCreated;
            inspector.OnHomeClicked = OnHomeClicked;

            Undo.undoRedoPerformed -= Reload;
            Undo.undoRedoPerformed += Reload;

            internalActions = new Dictionary<Actions, Action<object>>() {
                { Actions.Paste, OnPaste },
                { Actions.Delete, OnDelete },
                { Actions.Cut, OnCut },
                { Actions.Copy, OnCopy },
                { Actions.Duplicate, OnDuplicate },
                { Actions.EdgeCreate, OnEdgeCreate },
                { Actions.EdgeDelete, OnEdgeDelete },
                { Actions.SelectionChanged, OnSelected },
                { Actions.SelectionCleared, OnDeselected },
                { Actions.EdgeDrop, OnEdgeDrop },
                { Actions.Rename, OnRename }
            };


            if (contextMenu == null) {
                contextMenu = ContextMenu.CreateContextMenu(this);
                contextMenu.BuildContextMenu();
            }

            if (edgeDropMenu == null) {
                edgeDropMenu = EdgeDropMenu.CreateEdgeDropMenu(this);
            }

        }

        public void OnRename(object obj) {
            if (graphView.GetSelectedNodeCount() == 1){
                NodeView node = graphView.GetFirstSelectedNode() as NodeView;
                foreach (EditableLabelElement label in node.editableLabels){
                    label.EnableInput(!label.isInEditMode);
                    break;
                }
            }
        }

        private void OnEdgeDrop(object edge) {
            BaseEdge baseEdge = (BaseEdge)edge;
            if(baseEdge == null) return;

            PortView port = baseEdge.Output != null ? baseEdge.Output as PortView : null;
            if (port != null) {
                edgeDropMenu.port = port;
                edgeDropMenu.BuildContextMenu();
                SearchWindow.Open(edgeDropMenu, graphView);
            }
        }

		private bool IsFocusedElementNullOrNotBindable => graphView.focusController == null || graphView.focusController.focusedElement == null || !(graphView.focusController.focusedElement is IBindable);

		private void OpenContextMenu(MouseDownEvent evt) {
            if (evt.button == 1) {
				graphView.schedule.Execute(() => {
					if (IsFocusedElementNullOrNotBindable) {
						SearchWindow.Open(contextMenu, graphView);
					}
				});
            } 
        }

        /// <summary>
        /// Re-focus the graph to the center based on all present nodes.
        /// </summary>
        private void OnHomeClicked() {
            graphView.ClearSelection();
            FrameGraph();
        }

        /// <summary>
        /// Frame the graph based on the active selection.
        /// </summary>
        public void FrameGraph(object _=null) {
            graphView.FrameSelected();
        }

        /// <summary>
        /// find a node in the lookuptable of nodes.
        /// </summary>
        /// <param name="serializedValue"></param>
        /// <returns></returns>
        public NodeView GetNodeView(object serializedValue) {
            if (dataToViewLookup.ContainsKey(serializedValue)) {
                return dataToViewLookup[serializedValue];
            }
            return null;
        }

        public void Disable() {
            EnsureSerialization();
        }

        /// <summary>
        /// Ensure that every change is written to disk.
        /// </summary>
        public void EnsureSerialization() {
            if (graphData != null && graphData.SerializedGraphData != null) {
                Logger.Log("save");
                graphData.ForceSerializationUpdate();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Called when "something" was selected in this graph
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        private void OnSelected(object data = null) {
            inspector.SetInspectorContent(null);
            inspector.SetSelectedNodeInfoActive(active: false);

            int selectedNodesCount = graphView.GetSelectedNodeCount();
            if (selectedNodesCount > 1) {
                inspector.SetSelectedNodeInfoActive(selectedNodesCount, graphView.GetSelectedEdgesCount(), true);
            } else if(selectedNodesCount > 0) {
                NodeView selectedNode = graphView.GetFirstSelectedNode() as NodeView;
                inspector.SetInspectorContent(selectedNode.GetInspectorContent(), graphData.SerializedGraphData);
            }
        }

        /// <summary>
        /// Called when "something" was de-selected in this graph
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        private void OnDeselected(object data = null) {
            inspector.SetInspectorContent(null);
            inspector.SetSelectedNodeInfoActive(active: false);
        }

        /// <summary>
        /// Called if a copy operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        public void OnCopy(object data = null) {
			if (IsFocusedElementNullOrNotBindable) {
				List<NodeView> nodesToCapture = new List<NodeView>();

				graphView.ForEachSelectedNodeDo((node) => {
					NodeView scopedNodeView = node as NodeView;
					if (scopedNodeView != null) {
						nodesToCapture.Add(scopedNodeView);
					}
				});

				copyPasteHandler.CaptureSelection(nodesToCapture, graphData);
			}
        }

        /// <summary>
        /// Called if a paste operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        public void OnPaste(object data = null) {
			if (IsFocusedElementNullOrNotBindable) {
				copyPasteHandler.Resolve(graphData, (nodes) => {
					Undo.RecordObject(graphData.BaseObject, "Paste Action");
					// position node clones relative to the current mouse position & add them to the current graph
					Vector2 viewPosition = graphView.GetMouseViewPosition();
					PositionNodesRelative(viewPosition, nodes);
				}, Reload);
			}
        }

        /// <summary>
        /// Positions a set of nodes relative to the given view Position
        /// </summary>
        /// <param name="viewPosition"></param>
        /// <param name="nodes"></param>
        private void PositionNodesRelative(Vector2 viewPosition, List<NodeModel> nodes) {
            if (nodes == null || nodes.Count == 0) {
                return;
            }

            Bounds bounds = new Bounds(nodes[0].GetPosition(), Vector3.zero);
            foreach (NodeModel node in nodes) {
                bounds.Encapsulate(node.GetPosition());
            }

            Vector2 refPos = bounds.center;
            Vector2 newBasePos = new Vector2(refPos.x + (viewPosition.x - refPos.x), refPos.y + (viewPosition.y - refPos.y));

            foreach (NodeModel node in nodes) {
                Vector2 oldPos = node.GetPosition();
                Vector2 offset = (refPos - oldPos).normalized * Vector2.Distance(oldPos, refPos);
                Vector2 newPos = newBasePos - offset;
                node.SetPosition(newPos.x, newPos.y);
            }
        }

        /// <summary>
        /// Called if a delete operation should be started...
        /// </summary>
        /// <param name="data"></param>
        public void OnDelete(object data = null) {
            bool isDirty = false;

            // go over every selected edge...
            graphView.ForEachSelectedEdgeDo((edge) => {
                isDirty = true;
                // get the ouput port, this is where the referenced node sits
                PortView outputPort = edge.GetOutputPort() as PortView;
                outputPort.Reset();
            });

            // go over every selected node and build a list of nodes that should be deleted....
            List<NodeModel> nodesToRemove = new List<NodeModel>();
            graphView.ForEachSelectedNodeDo((node) => {
                NodeView scopedNodeView = node as NodeView;
                if (scopedNodeView != null) {
                    nodesToRemove.Add(scopedNodeView.controller.nodeItem);
                    isDirty = true;
                }
            });

            // if we have nodes marked for deletion...
            if (nodesToRemove.Count > 0) {
                // tidy up all ports before actual deletion...
                graphView.ForEachPortDo((basePort) => {
                    // we can ignore input ports, so check that we only operate on output ports...
                    if (basePort.Direction == Direction.Output) {
                        PortView port = basePort as PortView;
                        // check that the port actually is not empty...
                        if (port.boundProperty != null && port.boundProperty.managedReferenceValue != null) {
                            // loop over the list of nodes that should be removed...
                            foreach (NodeModel nodeToRemove in nodesToRemove) {
                                // if the ports actual object value is equal to a node that should be removed...
                                if (nodeToRemove.nodeData == port.boundProperty.managedReferenceValue) {
                                    // reset / nullify the port value to we don't have invisible nodes in our graph...
                                    port.Reset();
                                    break;
                                }
                            }
                        }
                    }
                });
            }

            // if we are dirty and objects were changed....
            if (isDirty) {
                // unbind and reload this graph to avoid serialization issues...
                graphView.Unbind();
                //graphView.schedule.Execute(() => {
                graphData.RemoveNodes(nodesToRemove);
                Reload();
                //});
            }

        }

        /// <summary>
        /// Called if a cut operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        public void OnCut(object data = null) {
            OnCopy();
            OnDelete();
        }

        /// <summary>
        /// Called if a duplication operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        public void OnDuplicate(object data = null) {
            OnCopy();
            OnPaste();
        }

        /// <summary>
        /// Called when an edge should be created...
        /// </summary>
        /// <param name="data">the edge</param>
        private void OnEdgeCreate(object data = null) {
            BaseEdge edge = (BaseEdge)data;
            graphView.AddElement(edge);
        }

        /// <summary>
        /// Called when an edge should be removed...
        /// </summary>
        /// <param name="data">the edge</param>
        private void OnEdgeDelete(object data = null) {
            BaseEdge edge = (BaseEdge)data;
            graphView.RemoveElement(edge);
        }

        /// <summary>
        /// Called when something in the viewport changed...
        /// </summary>
        /// <param name="data">unused</param>
        private void OnViewportChanged(GraphElementContainer contentContainer) {
            if (graphData != null && graphData.BaseObject != null) {
                graphData.SetViewport(contentContainer.transform.position, contentContainer.transform.scale);
            }
        }

        /// <summary>
        /// Called to reload the complete graph...
        /// </summary>
        public void Reload() {
            Logger.Log("reload");
            if (graphData != null /*&& graphData.BaseObject != null*/) {
                Load(graphData);
            }
        }

        /// <summary>
        /// Method to receive / tap into the action stream of the base graphview.
        /// This is useful to extend/ add more keyboard shortcuts to those already present...
        /// </summary>
        /// <param name="actionType">the actionType that was recognized</param>
        /// <param name="data">The actionData object that was given</param>
        private void OnGraphAction(Actions actionType, object data = null) {
            if (internalActions.ContainsKey(actionType)) {
                internalActions[actionType](data);
            }
        }

        /// <summary>
        /// Create a new node for the graph simply based on a valid type.
        /// </summary>
        /// <param name="nodeType">The node type.</param>
        public NodeView CreateNewNode(Type nodeType, bool isUtilityNode = false) {
            // get the current view position of the mouse, so we can display the new node at the tip of the mouse...
            Vector2 viewPosition = graphView.GetMouseViewPosition();

            // create a new instance of the give node type
            INode node = Activator.CreateInstance(nodeType) as INode;
            NodeModel nodeItem = graphData.AddNode(node, isUtilityNode);

            nodeItem.SetData(graphData.GetLastAddedNodeProperty(isUtilityNode));

            // create the actual node controller & add its view to this graph
            NodeController nodeController = new NodeController(nodeItem, this, viewPosition);
            graphView.AddElement(nodeController.nodeView);
            nodeController.Initialize();

            // Select newly created node
            graphView.SetSelected(nodeController.nodeView);

            dataToViewLookup.Add(nodeItem.nodeData, nodeController.nodeView);
            return nodeController.nodeView;
        }

        /// <summary>
        /// Called when a new graph asset was created and should now be loaded...
        /// </summary>
        /// <param name="graphData"></param>
        private void OnAfterGraphCreated(IGraphModelData graphData) {
            graphView.ClearView();
            this.graphData = graphData;
        }

        /// <summary>
        /// Called when we should load a graph...
        /// </summary>
        /// <param name="graphData"></param>
        private void OnShouldLoadGraph(IGraphModelData graphData) {
            inspector.CreateRenameGraphUI(graphData);
            inspector.Clear();
            Load(graphData);
        }

        /// <summary>
        /// Opens a new graph from an external resource.
        /// </summary>
        /// <param name="baseGraphModel">The graph data that should get loaded.</param>
        public void OpenGraphExternal(IGraphModelData baseGraphModel) {
            OnShouldLoadGraph(baseGraphModel);
        }

        /// <summary>
        /// Called when we actually should load a graph...
        /// </summary>
        /// <param name="graphData"></param>
        private void Load(IGraphModelData graphData) {
			OnBeforeGraphLoaded?.Invoke(graphData);

			// return early if we are already in the process of loading a graph...
			if (isLoading) {
                return;
            }

            // make sure to save the old graphs state...
            EnsureSerialization();

            // signal we are in the process of loading...
            isLoading = true;

            // clear everything up since we have changed contexts....
            inspector.Clear();
            graphView.ClearView();
            dataToViewLookup.Clear();
            inspector.SetSelectedNodeInfoActive(active: false);

            // offload the actual loading to the next frame...
            graphView.schedule.Execute(() => {
                this.graphData = graphData;
                if (this.graphData.SerializedGraphData == null) {
                    this.graphData.CreateSerializedObject();
                }

                // remember that this is our currently opened graph...
                SetLastOpenedGraphData(this.graphData);

                // update the viewport to the last saved location
                if (this.graphData.ViewportInitiallySet) {
                    graphView.UpdateViewTransform(this.graphData.ViewPosition, this.graphData.ViewScale);
                }

                void ForEachNodeProperty(List<NodeModel> nodes, SerializedProperty nodesProperty) {
                    // go over every node...
                    for (int i = 0; i < nodes.Count; i++) {
                        NodeModel node = nodes[i];

                        // initialize the node...
                        node.Initialize();

                        // find the acompanying nodeData serializedProperty and set it...
                        SerializedProperty nodeSerializedData = nodesProperty.GetArrayElementAtIndex(i);
                        node.SetData(nodeSerializedData);

                        // create a node controller for this node...
                        NodeController nodeController = new NodeController(node, this);
                        // add the actual node view to this graph view....
                        graphView.AddElement(nodeController.nodeView);
                        // initialize the controller after everything is ready to go...
                        nodeController.Initialize();
                        dataToViewLookup.Add(node.nodeData, nodeController.nodeView);
                    }
                }

                // find the nodes property of the loaded graphData
                SerializedProperty nodesProperty = this.graphData.GetNodesProperty(false);
                ForEachNodeProperty(this.graphData.Nodes, nodesProperty);

                // find the nodes property of the loaded graphData
                SerializedProperty utilitiyNodesProperty = this.graphData.GetNodesProperty(true);
                ForEachNodeProperty(this.graphData.UtilityNodes, utilitiyNodesProperty);

                // draw all port connections...
                // this does not happen automatically as we need to call ConnectPorts...
                graphView.ForEachNodeDo((node) => {
                    NodeView nodeView = node as NodeView;

                    // go through every port
                    foreach (PortView port in nodeView.outputPorts) {
                        // get the real object value of the port
                        object value = port.boundProperty.managedReferenceValue;
                        // if the value is not null...
                        if (value != null) {
                            // check if we can find the corresponding node in our lookup
                            if (dataToViewLookup.ContainsKey(value)) {
                                // if we found it, we know that the port must be the input port, so we can draw the connection
                                NodeView otherView = dataToViewLookup[value];
                                if (otherView.controller.nodeItem.nodeData == value) {
                                    ConnectPorts(port, otherView.inputPort);
                                }
                            }
                        }
                    }
                });

                isLoading = false;
				OnGraphLoaded?.Invoke(this.graphData);

				Logger.Log("data loaded");
            });
        }

        /// <summary>
        /// Method to connect to ports with one another....
        /// </summary>
        /// <param name="input">The input port....</param>
        /// <param name="output">The output port....</param>
        public void ConnectPorts(PortView input, PortView output) {
            graphView.ConnectPorts(input, output);
        }

        /// <summary>
        /// Draw method called by the window for legacy IMGUI code.
        /// Yes, we still need this to implement the graph load selection...
        /// </summary>
        public void Draw() {
            SearchWindow.targetPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            inspector.Draw();
        }
    }
}
