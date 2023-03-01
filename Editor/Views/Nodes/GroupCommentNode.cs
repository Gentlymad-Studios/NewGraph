using GraphViewBase;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {
    [Node]
    public class GroupCommentNode : INode, IUtilityNode {

        /// <summary>
        /// Our nodes usually don't need to serilaize their width but in this case, since it needs to be adjustable we need to serialize it.
        /// </summary>
        [SerializeField, HideInInspector]
        private float width = 300;
        
        /// <summary>
        /// Our nodes usually don't need to serilaize their height but in this case, since it needs to be adjustable we need to serialize it.
        /// </summary>
        [SerializeField, HideInInspector]
        private float height = 300;

        [SerializeField]
        private Color groupColor = new Color(30f / 255f, 30f / 255f, 30f / 255f, 0.5f);

        private NodeView nodeView;
        private NodeController nodeController;
        private VisualElement dragElement;
        private VisualElement container;
        private Vector2 expansionStartPosition;
        private Vector2 expansionMoveDelta;
        private float expansionStartWidth;
        private float expansionStartHeight;
        private Vector2 nodeStartPosition;
        private Vector2 nodeMoveDelta;
        private List<NodeView> containedNodes = new List<NodeView>();

        /// <summary>
        /// Create some nice vector based dragCaret graphics
        /// </summary>
        private static VectorImage dragCaretGraphics = null;
        private static VectorImage DragCaretGraphics {
            get {
                if (dragCaretGraphics == null) {
                    int imageSize = 10;
                    Painter2D painter = new Painter2D();
                    dragCaretGraphics = VectorImage.CreateInstance<VectorImage>();
                    painter.fillColor = Color.white;
                    painter.BeginPath();
                    painter.MoveTo(new(0, imageSize));
                    painter.LineTo(new(imageSize, 0));
                    painter.LineTo(new(imageSize, imageSize));
                    painter.LineTo(new(0, imageSize));
                    painter.Fill();
                    painter.SaveToVectorImage(dragCaretGraphics);
                }
                return dragCaretGraphics;
            }
        }

        public void Initialize(NodeController nodeController) {
            this.nodeController = nodeController;
            nodeView = nodeController.nodeView;
            // we want our node view to be "behind" all nodes so we give it its own layer
            nodeView.Layer = -20;
            nodeView.style.width = width;
            nodeView.style.height = height;
            nodeView.AddToClassList(nameof(CommentNode));
            nodeView.ExtensionContainer.RemoveFromHierarchy();
            nodeView.InputContainer.RemoveFromHierarchy();
            nodeView.OutputContainer.RemoveFromHierarchy();
            nodeView.RegisterCallback<MouseDownEvent>(OnNodeViewMouseDown);
            nodeView.RegisterCallback<MouseUpEvent>(OnMouseUp);

            // create a custom container
            container = new VisualElement();
            container.AddToClassList(nameof(GroupCommentNode)+"Container");
            container.style.backgroundColor = groupColor;
            nodeView.Add(container);

            // we want to react directly when the color of our group node was changed
            // so we need to retrieve our groupColor property field and listen on serializedproperty changes
            PropertyField colorfield = nodeView.inspectorContent.Q<PropertyField>(nameof(groupColor));
            colorfield.RegisterValueChangeCallback(OnColorChanged);
            container.pickingMode = PickingMode.Ignore;
            
            // the element that is initially clicked to "expand" the node
            dragElement = new Image() { vectorImage = DragCaretGraphics };
            dragElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            container.Add(dragElement);
        }

        private void OnColorChanged(SerializedPropertyChangeEvent evt) {
            container.style.backgroundColor = evt.changedProperty.colorValue;
        }

        /// <summary>
        /// If a left click was initiated we start listening on position changes and capture all nodes,
        /// that should be moved as being part of our group node.
        /// </summary>
        /// <param name="evt"></param>
        private void OnNodeViewMouseDown(MouseDownEvent evt) {
            // make sure we have a left click
            if (evt.button == 0) {
                // clear all previously captured nodes
                if (containedNodes == null) {
                    containedNodes = new List<NodeView>();
                }
                containedNodes.Clear();
                // go overy all nodes and check which nodes are contained
                // this is costly, so we do it only once when the movement process starts
                nodeController.ForEachNode((node) => {
                    // check via worldbound rect if node is contained in our group
                    if (node != nodeView && node != null && nodeView.worldBound.Overlaps(node.worldBound)) {
                        containedNodes.Add(node as NodeView);
                    }
                });
                // capture the initial start position
                nodeStartPosition = nodeView.GetPosition();
                // start listening for position changes
                nodeView.OnPositionChange -= OnPositionChange;
                nodeView.OnPositionChange += OnPositionChange;
            }
        }

        /// <summary>
        /// Called when the position of our node changed in the graph
        /// </summary>
        /// <param name="obj"></param>
        private void OnPositionChange(PositionData obj) {
            // calculate move data based on our last start position
            nodeMoveDelta = -1*(nodeStartPosition - obj.position);

            // set the position of all nodes that are contained
            foreach (NodeView nodeView in containedNodes) {
                Vector2 nodePos = nodeView.GetPosition();
                nodeView.SetPosition(new(nodePos.x+nodeMoveDelta.x, nodePos.y+nodeMoveDelta.y));
            }
            // reset move delta immediatly, since we don't want to keep track of the start position ov every node.
            nodeStartPosition = nodeView.GetPosition();
        }

        /// <summary>
        /// Called when any mouse button was down on our node
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseDown(MouseDownEvent evt) {
            if (evt.button == 0) {
                expansionStartPosition = GetViewScaleAdjustedPosition(evt.mousePosition);
                expansionMoveDelta = Vector2.zero;
                expansionStartWidth = nodeView.resolvedStyle.width;
                expansionStartHeight = nodeView.resolvedStyle.height;

                // prevent any event bubbeling or tickeling
                evt.StopImmediatePropagation();
                // on-demand adding of mouse move event
                nodeView.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                nodeView.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                // make sure we capture the mouse to exclusivly receive events
                nodeView.CaptureMouse();
            }
        }

        /// <summary>
        /// Called when any mouse button was up on our node.
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseUp(MouseUpEvent evt) {
            evt.StopImmediatePropagation();
            nodeView.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            nodeView.ReleaseMouse();
            nodeView.OnPositionChange -= OnPositionChange;
        }

        /// <summary>
        /// Called when any mouse button was bein pressed down.
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseMove(MouseMoveEvent evt) {
            // calculate the correct move delta
            expansionMoveDelta = -1 * (expansionStartPosition - GetViewScaleAdjustedPosition(evt.mousePosition));

            // make changes to the width and height appear in the undo stack
            Undo.RecordObject(nodeController.graphController.graphData, nameof(GroupCommentNode)+" Dimension change.");

            // set our serialized values for width & height
            width = expansionStartWidth + expansionMoveDelta.x;
            height = expansionStartHeight + expansionMoveDelta.y;
            
            // set styling accordingly
            nodeView.style.width = width;
            nodeView.style.height = height;

            // prevent immediate propagation
            evt.StopImmediatePropagation();
        }

        /// <summary>
        /// Create an adjusted position that takes the current view scale into account.
        /// </summary>
        /// <param name="position">position that needs adjustment</param>
        /// <returns>Adjusted position</returns>
        private Vector2 GetViewScaleAdjustedPosition(Vector2 position) {
            return new Vector2(position.x, position.y) / nodeController.GetViewScale();
        }

        /// <summary>
        /// Should inspector UI be created?
        /// </summary>
        /// <returns></returns>
        public bool CreateInspectorUI() => true;

        /// <summary>
        /// Should UI for the name be created?
        /// </summary>
        /// <returns></returns>
        public bool CreateNameUI() => true;

        /// <summary>
        /// Should the background be directly colorized by the node attribute?
        /// </summary>
        /// <returns></returns>
        public bool ShouldColorizeBackground() => false;
    }
}
