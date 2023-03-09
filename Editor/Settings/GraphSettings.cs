using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    [CreateAssetMenu(fileName = nameof(GraphSettings), menuName = nameof(GraphSettings), order = 1)]
    public class GraphSettings : ScriptableObject {
        public const string assetGUID = "015778251503b3c44a2b9dfc3a36ad10";
        public const string menuItemBase = "Tools/";
        public const string debugDefine = "TOOLS_DEBUG";
        public const string lastOpenedGraphEditorPrefsKey = nameof(NewGraph) + "." + nameof(GraphSettings) + "." + nameof(lastOpenedGraphEditorPrefsKey);
        public const string lastOpenedDirectoryPrefsKey = nameof(NewGraph) + "." + nameof(GraphSettings) + "." + nameof(lastOpenedDirectoryPrefsKey);
        public const string isInspectorVisiblePrefsKey = nameof(NewGraph) + "." + nameof(GraphSettings) + "." + nameof(isInspectorVisiblePrefsKey);

        public StyleSheet customStylesheet;

        [NonSerialized]
        private static string pathPartialToCategory = null;
        public static string PathPartialToCategory {
            get {
                if (pathPartialToCategory == null) {
                    pathPartialToCategory = Path.Combine(Instance.basePathToSettingsFile, Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(Instance)));
                    pathPartialToCategory = pathPartialToCategory.Replace("\\", "/");
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

        public string basePathToSettingsFile = "Project/Tools";
        public string searchWindowCommandHeader = "Commands";
        public string searchWindowRootHeader = "Create Nodes";
        public string openGraphButtonText = "Open Graph";

        [HideInInspector]
        public bool wasBlueprintTransferred = false;

        [SerializeField]
        private string hideInspectorIcon = "SceneLoadIn";
        public static Texture HideInspectorIcon => EditorGUIUtility.IconContent(Instance.hideInspectorIcon).image;

        [SerializeField]
        private string showInspectorIcon = "SceneLoadOut";
        public static Texture ShowInspectorIcon => EditorGUIUtility.IconContent(Instance.showInspectorIcon).image;

        [SerializeField]
        private string homeButtonIcon = "d_Profiler.UIDetails@2x";
        public static Image HomeButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Instance.homeButtonIcon).image };

        [SerializeField]
        private string createButtonIcon = "d_Toolbar Plus";
        public static Image CreateButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Instance.createButtonIcon).image };

        [SerializeField]
        private string loadButtonIcon = "d_FolderOpened Icon";
        public static Image LoadButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Instance.loadButtonIcon).image };

        /*
        [SerializeField]
        private string saveButtonIcon = "d_SaveAs@2x";
        public static Image SaveButtonIcon => new Image() { image = EditorGUIUtility.IconContent(Instance.saveButtonIcon).image };
        */

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
                if (Instance.loggerColorHex == null) {
                    Instance.loggerColorHex = ColorUtility.ToHtmlStringRGB(Instance.loggerColor);
                }
                return Instance.loggerColorHex;
            }
        }

        [SerializeField]
        private string handleBarsPartialIdentifier = "4e7bd4c2a267fc147b96af42ce53487c";
        [NonSerialized]
        private static VisualTreeAsset handleBarsPartial = null;
        public static VisualTreeAsset HandleBarsPartial {
            get {
                if (handleBarsPartial == null) {
                    handleBarsPartial = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(Instance.handleBarsPartialIdentifier));
                }
                return handleBarsPartial;
            }
        }

        public static VisualElement CreateHandleBars() {
            return HandleBarsPartial.Instantiate()[0];
        }

        [SerializeField]
        private string graphDocumentIdentifier = "a1a3b33cb142d06479dfaa381cc82245";
        public static VisualTreeAsset graphDocument => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(Instance.graphDocumentIdentifier));

        [SerializeField]
        private string graphStylesheetVariablesIdentifier = "a22fe40a267e5824dbde2cea493f40e4";
        public static StyleSheet graphStylesheetVariables => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(Instance.graphStylesheetVariablesIdentifier));

        [SerializeField]
        private string graphStylesheetIdentifier = "e2825f280a2499d4587129e0a3716e17";
        public static StyleSheet graphStylesheet => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(Instance.graphStylesheetIdentifier));

        public delegate void ValueChangedEvent(SerializedPropertyChangeEvent evt);
        public event ValueChangedEvent ValueChanged;

        public void NotifyValueChanged(SerializedPropertyChangeEvent evt) {
            ValueChanged?.Invoke(evt);
        }

        [NonSerialized]
        private static GraphSettings _instance = null;
        public static GraphSettings Instance {
            get {
                if (_instance == null) {
                    AssetDatabase.Refresh();
                    string blueprintSettingsPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    GraphSettings blueprintSettings = AssetDatabase.LoadAssetAtPath<GraphSettings>(blueprintSettingsPath);
                    if (blueprintSettings == null) {
                        Debug.LogError($"settings blueprint could not be resolved! Path: {blueprintSettingsPath} GUID: {assetGUID}");
                        blueprintSettings = ScriptableObject.CreateInstance<GraphSettings>();
                    }
#if !TOOLS_KEEP_SETTINGS
                    string targetPath = Path.Combine("Assets", blueprintSettings.basePathToSettingsFile, Path.GetFileName(blueprintSettingsPath));
                    string fullTargetPath = Path.GetFullPath(targetPath);

                    if (!File.Exists(fullTargetPath)) {
                        string directories = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(directories)) {
                            Directory.CreateDirectory(directories);
                        }
                        bool copied = AssetDatabase.CopyAsset(blueprintSettingsPath, targetPath);
                        if (!copied) {
                            Logger.LogAlways($"Unable to copy settings to directory: {fullTargetPath}. Falling back to original file! Please make sure this file can be copied.");
                            _instance = blueprintSettings;
                        }
                        AssetDatabase.Refresh();
                    }
                    _instance = AssetDatabase.LoadAssetAtPath<GraphSettings>(targetPath);
                    if (_instance == null) {
                        Logger.LogAlways($"There was an error loading the settings file at path: {fullTargetPath}. Falling back to original file! Please fix any errors.");
                        _instance = blueprintSettings;
                    }
#else
                    _instance = blueprintSettings;
#endif
                }
                return _instance;
            }
        }
    }
}
