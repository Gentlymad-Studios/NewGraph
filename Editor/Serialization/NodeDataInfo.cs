using System.Collections.Generic;

namespace NewGraph {
    /// <summary>
    /// this helper class holds the original node data as well as a list of "external" references
    /// external in this case means outside of the captured selection.
    /// </summary>
    public class NodeDataInfo {
        public List<ExternalReference> externalReferences = new List<ExternalReference>();
        public NodeModel baseNodeItem;
    }
}
