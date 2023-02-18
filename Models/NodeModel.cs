using System;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// Runtime valid part of our NodeModel.
    /// This is narrowed down to the minimum needed to work in runtime situations.
    /// See NodeModelEditorSpecifics.cs for all editor specific parts of this class.
    /// </summary>
    [Serializable]
    public partial class NodeModel {
        /// <summary>
        /// The actual node data.
        /// </summary>
        [SerializeReference]
        public INode nodeData;
    }
}
