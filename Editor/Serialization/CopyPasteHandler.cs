using OdinSerializer;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// This class handles the capturing & resolvement of graph content for Copy & Paste purposes.
    /// </summary>
    public class CopyPasteHandler {
        private List<NodeModel> clones = new List<NodeModel>();
        private List<NodeDataInfo> originals = new List<NodeDataInfo>();
        private Dictionary<INode, INode> originalsToClones = new Dictionary<INode, INode>();
        private GraphModel baseGraphData;

        /// <summary>
        /// Do we have nodes for a copy/paste operation present?
        /// </summary>
        /// <returns>Returns true if we have nodes for a copy/paste operation present</returns>
        public bool HasNodes() {
            return originals.Count > 0;
        }

        /// <summary>
        /// Capture a list of nodes that should be copied
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="rootData"></param>
        public void CaptureSelection(List<NodeView> nodes, GraphModel rootData) {
            // Adds a port to the given list of external references
            void AddPortAsExternalReference(NodeDataInfo nodeResolveData, PortView port, NodeView node) {
                string relativePath = port.boundProperty.propertyPath.Replace(node.controller.nodeItem.GetSerializedProperty().propertyPath, "");
                relativePath = relativePath.Substring(1, relativePath.Length - 1);

                if (port.boundProperty.managedReferenceValue != null) {
                    nodeResolveData.externalReferences.Add(new NodeReference() {
                        nodeData = port.boundProperty.managedReferenceValue,
                        relativePropertyPath = relativePath
                    });
                }
            }

            Clear();
            baseGraphData = rootData;
            foreach (NodeView node in nodes) {
                NodeDataInfo nodeResolveData = new NodeDataInfo();

                // go over every output port and add it as an external reference
                foreach (PortView outputPort in node.outputPorts) {
                    AddPortAsExternalReference(nodeResolveData, outputPort, node);
                }

                // go over every port found in a port list and add it as an external reference
                foreach (PortListView portListView in node.portLists) {
                    foreach (PortView port in portListView.ports) {
                        AddPortAsExternalReference(nodeResolveData, port, node);
                    }
                }
                nodeResolveData.baseNodeItem = node.controller.nodeItem;
                originals.Add(nodeResolveData);
            }

            // go over all captured nodes and clean up the external references list
            foreach (NodeDataInfo nodeData in originals) {
                for(int i=nodeData.externalReferences.Count-1; i>=0; i--) {
                    for (int j = originals.Count - 1; j >= 0; j--) {
                        // if we find a referenced node in the list of captured nodes
                        // it is not an external reference, therefore we can remove it from this list
                        if (nodeData.externalReferences[i].nodeData == originals[j].baseNodeItem.nodeData) {
                            nodeData.internalReferences.Add(nodeData.externalReferences[i]);
                            nodeData.externalReferences.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolve the currently captured selection into clones and reconstruct all connections
        /// </summary>
        public void Resolve(GraphModel rootData, System.Action<List<NodeModel>> onBeforeAdding, System.Action onAfterAdding) {
            List<NodeDataInfo> originalsCopy = new List<NodeDataInfo>(originals);
            Clear();

            bool isSameData = true;
            if (baseGraphData != null && baseGraphData != rootData) {
                isSameData = false;
            }

            foreach (NodeDataInfo original in originalsCopy) {
                // re-check for null as something could have happened to the captured INode object
                if (original != null && original.baseNodeItem != null) {
                    originals.Add(original);
                    NodeModel deepClone = DeepClone(original.baseNodeItem);
                    clones.Add(deepClone);
                    originalsToClones.Add(original.baseNodeItem.nodeData, deepClone.nodeData);
                }
            }

            if (clones.Count > 0) {
                NodeModel clone;
                NodeDataInfo originalResolveData;

                // execute callback with all nodes for pre-processing
                onBeforeAdding(clones);

                for (int i=0; i<clones.Count; i++) {
                    // get the clone
                    clone = clones[i];
                    // get the original ands its external references
                    originalResolveData = originals[i];

                    // add node to list of nodes and update serialized object!
                    rootData.AddNode(clone);
                    rootData.serializedGraphData.ApplyModifiedProperties();

                    // resolve the list of external references of this node
                    ResolveExternalReferences(ref originalResolveData.internalReferences, ref originalResolveData.externalReferences, ref isSameData, ref rootData);
                }
                onAfterAdding();
            }
        }

        /// <summary>
        /// When copy & pasting it might happen that we have references to nodes that are not part of the selection.
        /// This will happen quite frequently. Problem is, when deep copying, all of these references will become "new" nodes which is not what we want.
        /// Instead we want to reference the original node. With this method we can resolve list of external references so they point to the original node in the graph.
        /// A little bit more complexity is added if the paste command was executed while we
        /// </summary>
        /// <param name="externalReferences">The list of external references we want to resolve</param>
        /// <param name="isSameData">Are we operating on the same graphData where our selection was captures? Or are we on a foreign graph?</param>
        /// <param name="graphData">The actual graph data</param>
        private void ResolveExternalReferences(ref List<NodeReference> internalReferences, ref List<NodeReference> externalReferences, ref bool isSameData, ref GraphModel graphData) {
            SerializedProperty portReference;
            SerializedProperty node;

            // check if we have external references
            // external references = references to nodes that are outside of this capture
            if (externalReferences.Count > 0 || internalReferences.Count > 0) {

                // make sure we update the serialized object so we can get the serialized property of the lastly added node
                graphData.serializedGraphData.Update();
                node = graphData.GetLastAddedNodeProperty(false);

                foreach (NodeReference internalReference in internalReferences) {
                    // find the field (port) that has the external reference
                    portReference = node.FindPropertyRelative(internalReference.relativePropertyPath);
                    if (portReference != null) {
                        // set the managedReferenceValue of the serialized property to the original value
                        portReference.managedReferenceValue = originalsToClones[(INode)internalReference.nodeData];
                        graphData.serializedGraphData.ApplyModifiedProperties();
                    }
                }

                // go over the list of external references
                foreach (NodeReference externalReference in externalReferences) {
                    // find the field (port) that has the external reference
                    portReference = node.FindPropertyRelative(externalReference.relativePropertyPath);

                    if (portReference != null) {
                        // check if changed serialization context (another graph was opened)
                        if (isSameData) {
                            // set the managedReferenceValue of the serialized property to the original value
                            portReference.managedReferenceValue = externalReference.nodeData;
                        } else {
                            // if we are on another graph than the capture was made, we set the value to null as everything outside the capture should not exist here.
                            portReference.managedReferenceValue = null;
                        }
                        graphData.serializedGraphData.ApplyModifiedProperties();
                    }
                }
            }
        }

        /// <summary>
        /// Deeply clone a given object
        /// </summary>
        /// <param name="original">The original object</param>
        /// <returns></returns>
        private NodeModel DeepClone(NodeModel original) {
            List<Object> unityObjects;
            // this is the most brute force approach, but it has the benefit that we can really deepclone everything right out of the box.
            byte[] serializedNode = OdinSerializer.SerializationUtility.SerializeValueWeak(original, DataFormat.Binary, out unityObjects);
            return OdinSerializer.SerializationUtility.DeserializeValueWeak(serializedNode, DataFormat.Binary, unityObjects) as NodeModel;
        }

        /// <summary>
        /// Clear all captured data & clones.
        /// </summary>
        private void Clear() {
            clones.Clear();
            originals.Clear();
            originalsToClones.Clear();
        }
    }
}
