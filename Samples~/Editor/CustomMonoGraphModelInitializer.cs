using UnityEditor;

namespace NewGraph.Samples {
    public class CustomMonoGraphModelInitializer {

        [InitializeOnLoadMethod]
        private static void AddWindowType() {
            GraphWindow.AddWindowType(typeof(CustomMonoGraphModel), typeof(MonoInspectorController), MonoGraphModel.GetGraphData);
        }

        [MenuItem(GraphSettings.menuItemBase + "CustomWindow")]
        private static void AddMenu() {
            GraphWindow.InitializeWindowBase(typeof(CustomMonoGraphModel));
        }
    }
}
