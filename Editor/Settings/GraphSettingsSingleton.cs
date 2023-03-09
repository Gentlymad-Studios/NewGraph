using System;
using UnityEditor;
using UnityEngine;

namespace NewGraph {
    [FilePath("ProjectSettings/"+nameof(GraphSettings)+".asset", FilePathAttribute.Location.ProjectFolder)]
    public class GraphSettingsSingleton : ScriptableSingleton<GraphSettingsSingleton> {
        public GraphSettings settings = null;
        
        private static GraphSettings blueprintSettings = null;
        public static GraphSettings BlueprintSettings {
            get {
                if (blueprintSettings == null) {
                    AssetDatabase.Refresh();
                    string blueprintSettingsPath = AssetDatabase.GUIDToAssetPath(GraphSettings.assetGUID);
                    blueprintSettings = AssetDatabase.LoadAssetAtPath<GraphSettings>(blueprintSettingsPath);
                    if (blueprintSettings == null) {
                        Debug.LogError($"settings blueprint could not be resolved! Path: {blueprintSettingsPath} GUID: {GraphSettings.assetGUID}");
                        blueprintSettings = CreateInstance<GraphSettings>();
                    }
                }
                return blueprintSettings;
            }
        }

#if TOOLS_KEEP_SETTINGS
        [NonSerialized]
        private static bool isInitialized = false;
#endif

        public static GraphSettings Settings {
            get {
                if (instance.settings == null) {
                    instance.settings = CreateInstance<GraphSettings>();
                }

#if !TOOLS_KEEP_SETTINGS
                if (!instance.settings.wasBlueprintTransferred) {
#else
                if (!isInitialized) {
#endif
#if !TOOLS_KEEP_SETTINGS
                    EditorUtility.CopySerialized(BlueprintSettings, instance.settings);
                    Save();
                    instance.settings.wasBlueprintTransferred = true;
#else
                    instance.settings = BlueprintSettings;
                    Save();
                    isInitialized = true;
#endif
                }

                return instance.settings;
            }
        }

        protected GraphSettingsSingleton() {

        }

        /*
        private void OnValidate() {
            Save();
        }
        */

        public static void Save() {
            instance.Save(false);
        }

        public static string GetPathToFile() {
            return GetFilePath();
        }
    }
}