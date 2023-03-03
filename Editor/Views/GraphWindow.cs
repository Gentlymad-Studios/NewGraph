using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {
    public class GraphWindow : EditorWindow {

        private GraphController graphController;
        private PlayModeStateChange lastState;
        private static GraphWindow window;

        [MenuItem(GraphSettings.menuItemBase+nameof(GraphWindow))]
        public static void Initialize() {
            window = GetWindow<GraphWindow>(nameof(GraphWindow));
            window.wantsMouseMove= true;
            window.Show();
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        public void LogPlayModeState(PlayModeStateChange state) {
            if (lastState == PlayModeStateChange.ExitingPlayMode && state == PlayModeStateChange.EnteredEditMode) {
                graphController?.Reload();
            }
            lastState= state;
        }

        private void OnGUI() {
            graphController?.Draw();
        }

        private void OnDisable() {
            graphController?.Disable();
        } 

        public static Vector2 screenPosition {
            get {
                if (window == null) {
                    Initialize();
                }
                return window.position.position;
            }
        }

        public static void Redraw() {
            if (window == null) {
                Initialize();
            }
            window.Repaint();
        }

        private void CreateGUI() {
            VisualElement uxmlRoot = GraphSettings.graphDocument.CloneTree();
            rootVisualElement.Add(uxmlRoot);
            uxmlRoot.StretchToParentSize();

            graphController = new GraphController(uxmlRoot, rootVisualElement);
            rootVisualElement.styleSheets.Add(GraphSettings.graphStylesheetVariables);
            rootVisualElement.styleSheets.Add(GraphSettings.graphStylesheet);

            // add potential custom stylesheet
            if (GraphSettings.Instance.customStylesheet != null) {
                rootVisualElement.styleSheets.Add(GraphSettings.Instance.customStylesheet);
            }

            // re-open the last opened graph
            GraphModel lastLoadedGraph = GraphSettings.LastOpenedGraphModel;
            if (lastLoadedGraph != null) {
                graphController.OpenGraphExternal(lastLoadedGraph);
            }
        }

    }
}
