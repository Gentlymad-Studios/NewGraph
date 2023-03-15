using System;
using GraphViewBase;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace NewGraph {
    public class GraphView : GraphViewBase.GraphView {

        public Action<Actions, object> OnAction;
        public VisualElement graphViewRoot;

        public delegate void MouseDown(MouseDownEvent evt);
        public MouseDown OnMouseDown { get; set; }

        public GraphView(VisualElement parent, VisualElement root, Action<Actions, object> OnAction) {
            GraphWindow.OnGlobalKeyDown -= OnKeyDown;
            GraphWindow.OnGlobalKeyDown += OnKeyDown;
            root.RegisterCallback<MouseDownEvent>((evt) => { OnMouseDown(evt); });

            graphViewRoot = parent.Q<VisualElement>(nameof(graphViewRoot));
            graphViewRoot.Add(this);
            this.StretchToParentSize();

            this.OnAction = OnAction;
        }


        private void OnKeyDown(Event evt) {
            ExecuteShortcutHandler(evt.keyCode, evt.modifiers);
        }

        public Vector2 LocalToViewTransformPosition(Vector2 localMousePosition) {
            return new Vector2((localMousePosition.x - ContentContainer.transform.position.x) / ContentContainer.transform.scale.x, (localMousePosition.y - ContentContainer.transform.position.y) / ContentContainer.transform.scale.y);
        }

        public Vector2 GetCurrentScale() {
            return ContentContainer.transform.scale;
        }

        public Vector2 GetMouseViewPosition() {
            return LocalToViewTransformPosition(this.WorldToLocal(mousePosition));
        }

        /// <summary>
        /// Empty override to prevent defualt logic due to keydown event bugs.
        /// </summary>
        /// <param name="baseEvent"></param>
        protected override void ExecuteDefaultAction(EventBase baseEvent) {
            //base.ExecuteDefaultAction(baseEvent);
        }

        public void SetSelected(GraphElement graphElement, bool selected = true, bool clear = true) {
            if (clear) {
                ClearSelection();
            }
            graphElement.Selected = selected;
            OnActionExecuted(Actions.SelectionChanged, graphElement);
        }

        public void ClearView() {
            //this.Unbind();

            foreach (BaseNode node in ContentContainer.Nodes) {
                node.RemoveFromHierarchy();
            }

            foreach (BasePort port in ContentContainer.Ports) {
                port.RemoveFromHierarchy();
            }

            foreach (BaseEdge edge in ContentContainer.Edges) {
                edge.RemoveFromHierarchy();
            }
        }


        public BaseNode GetFirstSelectedNode() {
            return ContentContainer.NodesSelected.First();
        }

        public void ClearSelection() {
            ContentContainer.ClearSelection();
            OnActionExecuted(Actions.SelectionCleared);
        }

        public int GetSelectedNodeCount() {
            return ContentContainer.NodesSelected.Count();
        }

        public int GetSelectedEdgesCount() {
            return ContentContainer.EdgesSelected.Count();
        }

        public bool HasSelectedEdges() {

            return ContentContainer.EdgesSelected.Count() > 0 ? true : false;
        }

        public void ForEachPortDo(Action<BasePort> callback) {
            foreach (BasePort port in ContentContainer.Ports) {
                callback(port);
            }
        }

        public void FrameSelected() {
            Frame();
        }

        public List<BasePort> GetPorts() {
            return ContentContainer.Ports.ToList();
        }

        public void ForEachNodeDo(Action<BaseNode> callback) {
            foreach (BaseNode node in ContentContainer.Nodes) {
                callback(node);
            }
        }

        public void ForEachSelectedNodeDo(Action<BaseNode> callback) {
            foreach (BaseNode node in ContentContainer.NodesSelected) {
                callback(node);
            }
        }

        public void ForEachSelectedEdgeDo(Action<BaseEdge> callback) {
            foreach (BaseEdge edge in ContentContainer.EdgesSelected) {
                callback(edge);
            }
        }

        public override void OnActionExecuted(Actions actionType, object data = null) {
            OnAction(actionType, data);
        }

        public override BaseEdge CreateEdge() {
            return new EdgeView();
        }
    }
}
