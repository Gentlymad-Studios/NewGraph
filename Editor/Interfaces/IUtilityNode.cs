namespace NewGraph {
    public interface IUtilityNode {
        bool CreateInspectorUI();
        bool CreateNameUI();
        void Initialize(NodeController nodeController);
    }
}
