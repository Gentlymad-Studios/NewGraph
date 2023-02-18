using System.Collections.Generic;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// Runtime valid part of our GraphModel.
    /// This is narrowed down to the minimum needed to work with a scriptable object of this type even in runtime situations.
    /// See GraphModelEditorSpecifics.cs for all editor specific parts of this class.
    /// </summary>
    public partial class GraphModel : ScriptableObject {
        /// <summary>
        /// List of all the nodes we want to work on.
        /// </summary>
        [SerializeReference]
        public List<NodeModel> nodes = new List<NodeModel>();
    }

}
