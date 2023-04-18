using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    using static GraphSettingsSingleton;
    using static GraphSettings;

    /// <summary>
    /// The actual editor window for our graph.
    /// Contains a key down workaround to prevent an issue where key down events are passing through elements instead of bein received.
    /// https://forum.unity.com/threads/capturing-keydownevents-in-editorwindow-and-focus.762155/
    /// </summary>
    public class GraphWindow : EditorWindow {

        private static readonly Dictionary<Type, Type> windowControllerLookup = new Dictionary<Type, Type>() {
            { typeof(ScriptableGraphModel), typeof(ScriptableInspectorController) },
            { typeof(MonoGraphModel), typeof(MonoInspectorController) },
        };

        private static readonly Dictionary<Type, Func<string, IGraphModelData>> lastGraphCreationLookup = new Dictionary<Type, Func<string, IGraphModelData>>() {
            { typeof(ScriptableGraphModel), ScriptableGraphModel.GetGraphData },
            { typeof(MonoGraphModel), MonoGraphModel.GetGraphData },
        };

        private KeyCode lastKeyCode;
        private EventModifiers lastModifiers;
        private EventType eventType;
        private GraphController graphController;


        private static Type currentWindowType = null;
        private static string currentWindowTypeKey = nameof(NewGraph) + "." + nameof(currentWindowType);
        private static Type CurrentWindowType {
            get {
                if (currentWindowType == null) {
                    string savedType = EditorPrefs.GetString(currentWindowTypeKey, null);
                    if (savedType != null) {
                        currentWindowType = Type.GetType(savedType);
                    } 
                }
                return currentWindowType;
            }
            set {
                currentWindowType = value;
                EditorPrefs.SetString(currentWindowTypeKey, currentWindowType.AssemblyQualifiedName);
            }
        }

        public static event Action<Event> OnGlobalKeyDown;

        [NonSerialized]
        private static GraphWindow window = null;

        [NonSerialized]
        private static bool loadRequested = false;

        [MenuItem(menuItemBase + nameof(GraphWindow))]
        private static void InitializeScriptableWindow() {
            InitializeWindowBase(typeof(ScriptableGraphModel));
        }
 
        private static void InitializeWindowBase(Type windowType) {
            if (window != null && CurrentWindowType != windowType) {
                window.Close();
            }
            if (window == null) {
                CurrentWindowType = windowType;
                window = GetWindow<GraphWindow>(Settings.windowName);
                window.wantsMouseMove = true;
                window.Show();
            }
        }

        private void OnEnable() {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            EditorApplication.playModeStateChanged += LogPlayModeState;
            GlobalKeyEventHandler.OnKeyEvent -= HandleGlobalKeyPressEvents;
            GlobalKeyEventHandler.OnKeyEvent += HandleGlobalKeyPressEvents;
        }

        private void EditorUpdate() {
            LoadGraph();
            EditorApplication.update -= EditorUpdate;
        }

        private void HandleGlobalKeyPressEvents(Event evt) {
            if (evt.isKey && mouseOverWindow == this && hasFocus) {
                if (lastKeyCode != evt.keyCode || lastModifiers != evt.modifiers) {
                    lastModifiers = evt.modifiers;
                    lastKeyCode = evt.keyCode;
                    eventType = evt.type;
                    OnGlobalKeyDown?.Invoke(evt);
                }
                if (evt.type == EventType.KeyUp) {
                    lastKeyCode = KeyCode.None;
                    lastModifiers = EventModifiers.None;
                }
            }
        }

        public void LogPlayModeState(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                graphController?.EnsureSerialization();

            } else if (state == PlayModeStateChange.EnteredEditMode) {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                if (window != null) {
                    window.Close();
                }

            }else if (state == PlayModeStateChange.EnteredPlayMode) {
                graphController?.Reload();
            }
        }

        private void OnGUI() {
            graphController?.Draw();
        }

        private void OnDisable() {
            //graphController?.Disable();
            GlobalKeyEventHandler.OnKeyEvent -= HandleGlobalKeyPressEvents;
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            loadRequested = false;
        }

        public static void LoadGraph(IGraphModelData graph =null) {
            if (graph != null) {
                SetLastOpenedGraphData(graph);
            } else {
                LastGraphInfo lastGraphInfo = LastOpenedGraphInfo;
                if (lastGraphInfo != null) {
                    graph = lastGraphCreationLookup[lastGraphInfo.graphType](lastGraphInfo.GUID);
                }
            }

            if (graph != null) {

                if(graph.SerializedGraphData == null) {
                    graph.CreateSerializedObject();
                }

                Type windowType = graph.BaseObject.GetType();
                InitializeWindowBase(windowType);
                window.graphController.OpenGraphExternal(graph);

            } else {
                LastOpenedGraphInfo = null;
            }

            loadRequested = true;
        }

        private void CreateGUI() {
            VisualElement uxmlRoot = graphDocument.CloneTree();
            rootVisualElement.Add(uxmlRoot);
            uxmlRoot.StretchToParentSize();

            graphController = new GraphController(uxmlRoot, rootVisualElement, windowControllerLookup[CurrentWindowType]);
            rootVisualElement.styleSheets.Add(graphStylesheetVariables);
            rootVisualElement.styleSheets.Add(graphStylesheet); 

            // add potential custom stylesheet
            if (Settings.customStylesheet != null) {
                rootVisualElement.styleSheets.Add(Settings.customStylesheet);
            }

            // delay loading the last graph to the next frame
            // otherwise this method will be called before loadRequested could be set
            rootVisualElement.schedule.Execute(() =>{
                if (!loadRequested) {
                    LoadGraph(); 
                }
                loadRequested = false;
            });

        }
    }
}
