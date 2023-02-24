using UnityEngine.UIElements;

namespace NewGraph {
    [Node("#00000001")]
    public class CommentNode : INode, IUtilityNode {
        private NodeView nodeView;

        public bool CreateInspectorUI() => false;

        public bool CreateNameUI() => true;

        public void Initialize(NodeController nodeController) {
            nodeView = nodeController.nodeView;
            // we need auto resizing, so the comment node expands to the full width of the provided text...
            nodeView.style.width = StyleKeyword.Auto;
            nodeView.AddToClassList(nameof(CommentNode));

            // comment node is a very special snowflake... :D
            // So: We completely remove all the default ui
            nodeView.ExtensionContainer.RemoveFromHierarchy();
            nodeView.InputContainer.RemoveFromHierarchy();
            nodeView.OutputContainer.RemoveFromHierarchy();
        }

        public bool ShouldColorizeBackground() {
            return true;
        }
    }
}
