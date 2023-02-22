namespace NewGraph {
    public class GraphPropertyInfo : PropertyInfo {

        public GraphPropertyInfo(string relativePropertyPath, GraphDisplayAttribute graphDisplay = null) : base(relativePropertyPath) {
            if (graphDisplay != null) {
                this.graphDisplay = graphDisplay;
            }
        }

        public GroupInfo groupInfo = null;
        public GraphDisplayAttribute graphDisplay = new GraphDisplayAttribute();
    }
}
