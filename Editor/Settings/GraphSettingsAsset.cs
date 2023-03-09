using UnityEngine;

namespace NewGraph {

    //[CreateAssetMenu(fileName = nameof(GraphSettingsAsset), menuName = nameof(GraphSettingsAsset), order = 1)]
    public class GraphSettingsAsset : ScriptableObject {
        public GraphSettings graphSettings = new GraphSettings();
    }
}
