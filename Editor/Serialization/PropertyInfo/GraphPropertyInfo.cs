using System.Collections.Generic;
using UnityEngine;

namespace NewGraph {
    public class GraphPropertyInfo : PropertyInfo {

        public GraphPropertyInfo(string relativePropertyPath, List<HeaderAttribute> headers = null, int spacesCount = 0, GraphDisplayAttribute graphDisplay = null) : base(relativePropertyPath) {
            if (headers != null) {
                this.headers = headers;
            }
            if (graphDisplay != null) {
                this.graphDisplay = graphDisplay;
            }
            this.spacesCount = spacesCount;
        }

        public GroupInfo groupInfo = null;
        public List<HeaderAttribute> headers = new List<HeaderAttribute>();
        public int spacesCount = 0;
        public GraphDisplayAttribute graphDisplay = new GraphDisplayAttribute();
    }
}
