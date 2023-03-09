using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {
    [System.Serializable]
    public class GraphSettings {
        public const string assetGUID = "015778251503b3c44a2b9dfc3a36ad10";
        public const string menuItemBase = "Tools/";
        public const string debugDefine = "TOOLS_DEBUG";
        public const string lastOpenedGraphEditorPrefsKey = nameof(NewGraph) + "." + nameof(GraphSettingsAsset) + "." + nameof(lastOpenedGraphEditorPrefsKey);
        public const string lastOpenedDirectoryPrefsKey = nameof(NewGraph) + "." + nameof(GraphSettingsAsset) + "." + nameof(lastOpenedDirectoryPrefsKey);
        public const string isInspectorVisiblePrefsKey = nameof(NewGraph) + "." + nameof(GraphSettingsAsset) + "." + nameof(isInspectorVisiblePrefsKey);

        public StyleSheet customStylesheet;

        [NonSerialized]
        private static string pathPartialToCategory = null;
        public static string PathPartialToCategory {
            get {
                if (pathPartialToCategory == null) {
                    pathPartialToCategory = menuItemBase + nameof(GraphSettings);
                }
                return pathPartialToCategory;
            }
        }

        public static GraphModel LastOpenedGraphModel {
            get {
                if (EditorPrefs.HasKey(lastOpenedGraphEditorPrefsKey)) {
                    string lastOpenedGraphGUID = EditorPrefs.GetString(lastOpenedGraphEditorPrefsKey, null);
                    if (!string.IsNullOrWhiteSpace(lastOpenedGraphGUID)) {
                        string assetPath = AssetDatabase.GUIDToAssetPath(lastOpenedGraphGUID);
                        if (!string.IsNullOrWhiteSpace(assetPath)) {
                            return AssetDatabase.LoadAssetAtPath<GraphModel>(assetPath);
                        }
                    }
                }
                return null;
            }
            set {
                string assetPath = AssetDatabase.GetAssetPath(value);
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    EditorPrefs.SetString(lastOpenedGraphEditorPrefsKey, AssetDatabase.AssetPathToGUID(assetPath));
                }
            }
        }

        public string searchWindowCommandHeader = "Commands";
        public string searchWindowRootHeader = "Create Nodes";
        public string openGraphButtonText = "Open Graph";

        [HideInInspector]
        public bool wasBlueprintTransferred = false;

        [SerializeField]
        private string hideInspectorIcon = "SceneLoadIn";
        public static Texture HideInspectorIcon => EditorGUIUtility.IconContent(Settings.hideInspectorIcon).image;

        [SerializeField]
        private string showInspectorIcon = "SceneLoadOut";
        public static Texture ShowInspectorIcon => EditorGUIUtility.IconContent(Settings.showInspectorIcon).image;

        [SerializeField]
        private string homeButtonIcon = "d_Profiler.UIDetails@2x";
        public static Image HomeButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Settings.homeButtonIcon).image };

        [SerializeField]
        private string createButtonIcon = "d_Toolbar Plus";
        public static Image CreateButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Settings.createButtonIcon).image };

        [SerializeField]
        private string loadButtonIcon = "d_FolderOpened Icon";
        public static Image LoadButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Settings.loadButtonIcon).image };

        [SerializeField]
        private string resetButtonIcon = "Refresh@2x";
        public static Image ResetButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Settings.resetButtonIcon).image };

        public string resetButtonTooltip = "Reset this value back to the default value.";
        public string resetAllLabel = "Reset All To Default";
        public string resetAllTooltip = "Reset All values to default values.";

        public string createUtilityNodeLabel = "Create Utility";
        public string createNodeLabel = "Create Node";
        public string noGraphLoadedLabel = "No graph active!";
        public string multipleSelectedMessagePartial = "selected. Select a single node to change any exposed values.";
        public string node = nameof(node);
        public string edge = nameof(edge);
        [Range(1, 10)]
        public int edgeWidthSelected = 2;
        [Range(1, 10)]
        public int edgeWidthUnselected = 2;
        public Color disabledPortColor = new(70 / 255f, 70 / 255f, 70 / 255f, 0.27f);
        public Color portColor = new(240 / 255f, 240 / 255f, 255 / 255f, 0.95f);
        public Color colorSelected = new(240 / 255f, 240 / 255f, 240 / 255f);
        public Color colorUnselected = new(146 / 255f, 146 / 255f, 146 / 255f);
        public Color defaultNodeColor;
        public string defaultLabelForPortListItems = "Empty";
        public string defaultInputName = "input";
        [Range(150, 400)]
        public int nodeWidth = 300;
        [Range(150, 400)]
        public int inspectorWidth = 300;
        [SerializeField]
        private string baseGraphPathPartial = string.Empty;
        [SerializeField]
        private Color loggerColor = Color.green;

        [NonSerialized]
        private string loggerColorHex = null;
        public static string LoggerColorHex {
            get {
                if (Settings.loggerColorHex == null) {
                    Settings.loggerColorHex = ColorUtility.ToHtmlStringRGB(Settings.loggerColor);
                }
                return Settings.loggerColorHex;
            }
        }

        [SerializeField]
        private string handleBarsPartialIdentifier = "4e7bd4c2a267fc147b96af42ce53487c";
        [NonSerialized]
        private static VisualTreeAsset handleBarsPartial = null;
        public static VisualTreeAsset HandleBarsPartial {
            get {
                if (handleBarsPartial == null) {
                    handleBarsPartial = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(Settings.handleBarsPartialIdentifier));
                }
                return handleBarsPartial;
            }
        }

        public static VisualElement CreateHandleBars() {
            return HandleBarsPartial.Instantiate()[0];
        }

        [SerializeField]
        private string graphDocumentIdentifier = "a1a3b33cb142d06479dfaa381cc82245";
        public static VisualTreeAsset graphDocument => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(Settings.graphDocumentIdentifier));

        [SerializeField]
        private string graphStylesheetVariablesIdentifier = "a22fe40a267e5824dbde2cea493f40e4";
        public static StyleSheet graphStylesheetVariables => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(Settings.graphStylesheetVariablesIdentifier));

        [SerializeField]
        private string graphStylesheetIdentifier = "e2825f280a2499d4587129e0a3716e17";
        public static StyleSheet graphStylesheet => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(Settings.graphStylesheetIdentifier));

        [SerializeField]
        private string settingsStylesheetIdentifier = "7d7e4dcd278879849b62a784de9bdc3c";
        public static StyleSheet settingsStylesheet => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(Settings.settingsStylesheetIdentifier));


        public delegate void ValueChangedEvent(SerializedPropertyChangeEvent evt);
        public event ValueChangedEvent ValueChanged;

        public void NotifyValueChanged(SerializedPropertyChangeEvent evt) {
            ValueChanged?.Invoke(evt);
        }
    }
}
