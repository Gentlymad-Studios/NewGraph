using GraphViewBase;
using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {
    public class GraphController {

        public GraphModel graphData;
        private GraphView graphView;
        private InspectorController<GraphModel> inspector;
        private GraphSearchWindow searchWindow;

        private Dictionary<Actions, Action<object>> internalActions;
        private CopyPasteHandler copyPasteHandler = new CopyPasteHandler();

        private bool isLoading = false;
        private Dictionary<object, NodeView> dataToViewLookup = new Dictionary<object, NodeView>();
        private Dictionary<Type, string> nodeTypeToCreationLabel = new Dictionary<Type, string>();
        
        private GenericMenu dropdownMenu;
        private Type dropdownMenuCurrentType;

        public Vector2 GetViewScale() {
            return graphView.GetCurrentScale();
        }

        public void ForEachNode(Action<BaseNode> callback) {
            graphView.ForEachNodeDo(callback);
        }

        public GraphController(VisualElement uxmlRoot, VisualElement root) {
            graphView = new GraphView(uxmlRoot, root, OnGraphAction);
            graphView.OnViewTransformChanged -= OnViewportChanged;
            graphView.OnViewTransformChanged += OnViewportChanged;
            graphView.OnMouseDown -= OpenContextMenu;
            graphView.OnMouseDown += OpenContextMenu;

            inspector = new InspectorController<GraphModel>(uxmlRoot);  
            //inspector.OnSaveClicked = OnSaveClicked;
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
            };

            if (searchWindow == null) {
                searchWindow = ScriptableObject.CreateInstance<GraphSearchWindow>();
                searchWindow.Initialize(graphView.shortcutHandler);
                BuildSearchableMenu();
            }

        }

        private void OnEdgeDrop(object edge) {
            BaseEdge baseEdge = (BaseEdge)edge;
            if(baseEdge == null) return;

            PortView port = baseEdge.Output != null ? baseEdge.Output as PortView : null;
            if (port != null) {
                if (port.type != dropdownMenuCurrentType) {
                    dropdownMenu = null;
                    dropdownMenu = new GenericMenu();
                    foreach (Type type in port.connectableTypes) {
                        if (nodeTypeToCreationLabel.ContainsKey(type)) {
                            void CreateNodeAndConnect() {
                                NodeView nodeView = CreateNewNode(type, false);
                                ConnectPorts(port, nodeView.inputPort);
                                //Reload();
                            }
                            dropdownMenu.AddItem(new GUIContent(nodeTypeToCreationLabel[type]), false, CreateNodeAndConnect);
                        }
                    }
                    dropdownMenu.ShowAsContext();
                }
            }
        }

        private void OpenContextMenu(MouseDownEvent evt) {
            if (evt.button == 1) {
                SearchWindow.Open(searchWindow, graphView);
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
        private void FrameGraph(object _=null) {
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
            if (graphData != null && graphData.serializedGraphData != null) {
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
                inspector.SetInspectorContent(selectedNode.GetInspectorContent(), graphData.serializedGraphData);
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
        private void OnCopy(object data = null) {
            List<NodeView> nodesToCapture = new List<NodeView>();

            graphView.ForEachSelectedNodeDo((node) => {
                NodeView scopedNodeView = node as NodeView;
                if (scopedNodeView != null) {
                    nodesToCapture.Add(scopedNodeView);
                }
            });

            copyPasteHandler.CaptureSelection(nodesToCapture, graphData);
        }

        /// <summary>
        /// Called if a paste operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        private void OnPaste(object data = null) {
            copyPasteHandler.Resolve(graphData, (nodes) => {
                Undo.RecordObject(graphData, "Paste Action");
                // position node clones relative to the current mouse position & add them to the current graph
                Vector2 viewPosition = graphView.GetMouseViewPosition();
                PositionNodesRelative(viewPosition, nodes);
            }, Reload);
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
        private void OnDelete(object data = null) {
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
                graphView.schedule.Execute(() => {
                    graphData.RemoveNodes(nodesToRemove);
                    Reload();
                });
            }

        }

        /// <summary>
        /// Called if a cut operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        private void OnCut(object data = null) {
            OnCopy();
            OnDelete();
        }

        /// <summary>
        /// Called if a duplication operation should be started...
        /// </summary>
        /// <param name="data">currently unused, check selected lists to get the actual selected objects...</param>
        private void OnDuplicate(object data = null) {
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
            if (graphData != null) {
                graphData.SetViewport(contentContainer.transform.position, contentContainer.transform.scale);
            }
        }

        /// <summary>
        /// Called to reload the complete graph...
        /// </summary>
        public void Reload() {
            Logger.Log("reload");
            if (graphData != null) {
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

        private void BuildSearchableMenu() {
            Func<bool> defaultEnabledCheck = () => graphView.GetSelectedNodeCount() > 0;
            Func<bool> nodeEnabledCheck = () => graphData != null;

            searchWindow.StartAddingMenuEntries(Settings.searchWindowRootHeader);
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
                    createNodeLabel = (!isUtilityNode ? Settings.createNodeLabel : Settings.createUtilityNodeLabel) + createNodeLabel;
                    nodeTypeToCreationLabel.Add(nodeType, createNodeLabel);

                    searchWindow.AddNodeEntry(createNodeLabel, (obj) => CreateNewNode(nodeType, isUtilityNode));
                }
            }
            searchWindow.ResolveNodeEntries(nodeEnabledCheck);
            searchWindow.AddSeparator(Settings.searchWindowCommandHeader);
            searchWindow.AddShortcutEntry(Actions.Frame, SearchTreeEntry.AlwaysEnabled, FrameGraph);
            searchWindow.AddShortcutEntry(Actions.Cut, defaultEnabledCheck, OnCut);
            searchWindow.AddShortcutEntry(Actions.Copy, defaultEnabledCheck, OnCopy);
            searchWindow.AddShortcutEntry(Actions.Paste, () => copyPasteHandler.HasNodes(), OnPaste);
            searchWindow.AddShortcutEntry(Actions.Duplicate, defaultEnabledCheck, OnDuplicate);
            searchWindow.AddShortcutEntry(Actions.Delete, () => graphView.HasSelectedEdges() || defaultEnabledCheck(), OnDelete);
        }

        /// <summary>
        /// Create a new node for the graph simply based on a valid type.
        /// </summary>
        /// <param name="nodeType">The node type.</param>
        private NodeView CreateNewNode(Type nodeType, bool isUtilityNode = false) {
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
        private void OnAfterGraphCreated(GraphModel graphData) {
            graphView.ClearView();
            this.graphData = graphData;
            this.graphData.CreateSerializedObject();
        }

        /// <summary>
        /// Called when we should load a graph...
        /// </summary>
        /// <param name="graphData"></param>
        private void OnShouldLoadGraph(GraphModel graphData) {
            Load(graphData);
        }

        /// <summary>
        /// Opens a new graph from an external resource.
        /// </summary>
        /// <param name="baseGraphModel">The graph data that should get loaded.</param>
        public void OpenGraphExternal(GraphModel baseGraphModel) {
            inspector.CreateRenameGraphUI(baseGraphModel);
            inspector.Clear();
            OnShouldLoadGraph(baseGraphModel);
        }

        /// <summary>
        /// Called when we actually should load a graph...
        /// </summary>
        /// <param name="graphData"></param>
        private void Load(GraphModel graphData) {

            // return early if we are already in the process of loading a graph...
            if (isLoading) {
                return;
            }

            // make sure to save the old graphs state...
            EnsureSerialization();

            // signal we are in the process of loading...
            isLoading = true;

            // clear everything up since we have changed contexts....
            graphView.ForEachNodeDo((node) => {
                (node as NodeView).controller.Dispose();
            });
            inspector.Clear();
            graphView.ClearView();
            dataToViewLookup.Clear();
            inspector.SetSelectedNodeInfoActive(active: false);

            // offload the actual loading to the next frame...
            graphView.schedule.Execute(() => {
                this.graphData = graphData;
                this.graphData.CreateSerializedObject();

                // remember that this is our currently opened graph...
                GraphSettings.LastOpenedGraphModel = graphData;

                // update the viewport to the last saved location
                if (graphData.ViewportInitiallySet) {
                    graphView.UpdateViewTransform(graphData.ViewPosition, graphData.ViewScale);
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
                SerializedProperty nodesProperty = graphData.GetNodesProperty(false);
                ForEachNodeProperty(graphData.nodes, nodesProperty);

                // find the nodes property of the loaded graphData
                SerializedProperty utilitiyNodesProperty = graphData.GetNodesProperty(true);
                ForEachNodeProperty(graphData.utilityNodes, utilitiyNodesProperty);

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
                                if(otherView.controller.nodeItem.nodeData == value) {
                                    ConnectPorts(port, otherView.inputPort);
                                }
                            }
                        }
                    }
                });

                isLoading = false;
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
