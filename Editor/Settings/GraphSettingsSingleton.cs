using System;
using UnityEditor;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// GraphSettings as a singleton asset stored outside the assets folder.
    /// </summary>
    [FilePath("ProjectSettings/"+nameof(GraphSettings)+".asset", FilePathAttribute.Location.ProjectFolder)]
    public class GraphSettingsSingleton : ScriptableSingleton<GraphSettingsSingleton> {
        /// <summary>
        /// Very quirky, if we don't go over this ScriptableObject that is only used temporarly we'll get uneditable properties.
        /// By operating on this data everything will be editable. Maybe this is a Unity bug.
        /// </summary>
        [NonSerialized]
        public GraphSettingsAsset settingsAsset = null;

        /// <summary>
        /// The actual GraphSettings that are serialized. 
        /// </summary>
        public GraphSettings settings = new GraphSettings();

        /// <summary>
        /// Blueprints is a GraphSettings file that is shipped with the NewGrpah framework.
        /// </summary>
        private static GraphSettingsAsset blueprintSettings = null;
        public static GraphSettingsAsset BlueprintSettings {
            get {
                if (blueprintSettings == null) {
                    AssetDatabase.Refresh();
                    string blueprintSettingsPath = AssetDatabase.GUIDToAssetPath(GraphSettings.assetGUID);
                    blueprintSettings = AssetDatabase.LoadAssetAtPath<GraphSettingsAsset>(blueprintSettingsPath);
                    if (blueprintSettings == null) {
                        Debug.LogError($"settings blueprint could not be resolved! Path: {blueprintSettingsPath} GUID: {GraphSettings.assetGUID}");
                        blueprintSettings = CreateInstance<GraphSettingsAsset>();
                    }
                }
                return blueprintSettings;
            }
        }

        public static GraphSettings Settings {
            get {
                // first call after re compilation, the settings asset will be empty
                if (instance.settingsAsset == null) {
                    // create a new asset
                    instance.settingsAsset = CreateInstance<GraphSettingsAsset>();
                    // set the settings based on what we already have in store
                    instance.settingsAsset.graphSettings = instance.settings;
                }

                // check wether settings were transferred from the bleuprint at least once.
                // if not copy the settings from our blueprint to our current settings.
                if (!instance.settings.wasBlueprintTransferred) {
                    EditorUtility.CopySerialized(BlueprintSettings, instance.settingsAsset);
                    instance.settings = instance.settingsAsset.graphSettings;
                    instance.settings.wasBlueprintTransferred = true;
                    Save();
                }

                return instance.settings;
            }
        }

        /// <summary>
        /// Publicly expose the save method so we can call this externally
        /// </summary>
        public static void Save() {
            instance.Save(false);
        }

        /// <summary>
        /// Copy a specific property from the blueprint to a property (requires matching objects)
        /// </summary>
        /// <param name="dest"></param>
        public static void CopyFromBlueprint(SerializedProperty dest) {
            SerializedProperty blueprintProperty = new SerializedObject(BlueprintSettings).FindProperty(dest.propertyPath);
            dest.serializedObject.CopyFromSerializedProperty(blueprintProperty);
            dest.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Copy all values from the blueprints to our settings.
        /// </summary>
        /// <param name="serializedObject"></param>
        public static void CopyAllFromBlueprint(SerializedObject serializedObject) {
            EditorUtility.CopySerialized(BlueprintSettings, instance.settingsAsset);
            serializedObject.ApplyModifiedProperties();
        }
    }
}