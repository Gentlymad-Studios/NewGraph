namespace NewGraph {
    public interface IUtilityNode {
        bool ShouldColorizeBackground();
        bool CreateInspectorUI();
        bool CreateNameUI();
        void Initialize(NodeController nodeController);
    }
}
