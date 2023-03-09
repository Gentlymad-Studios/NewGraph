using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {

    public class GraphSettingsProvider : SettingsProviderBase {

        private static readonly string[] tags = new string[] { nameof(GraphSettings), nameof(NewGraph) };

        public GraphSettingsProvider(SettingsScope scope = SettingsScope.Project) : base(GraphSettings.PathPartialToCategory, scope) {
            keywords = tags;
        }
         
        protected override EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
            return ValueChanged;
        }

        protected override Action<SerializedProperty, VisualElement> GetCreateAdditionalUIAction() {
            return CreateAdditionalUI;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            base.OnActivate(searchContext, rootElement);
            rootElement.styleSheets.Add(GraphSettings.settingsStylesheet);
            rootElement.Add(new Button(() => {
                EditorUtility.CopySerialized(BlueprintSettings, Settings);
                new SerializedObject(Settings).ApplyModifiedProperties();
            }) { text = "Reset All To Default"});
        }

        private void CreateAdditionalUI(SerializedProperty property, VisualElement container) {
            Button resetButton= new Button(() => {
                SerializedProperty blueprintProperty = new SerializedObject(BlueprintSettings).FindProperty(property.propertyPath);
                property.serializedObject.CopyFromSerializedProperty(blueprintProperty);
                property.serializedObject.ApplyModifiedProperties();
            });
            resetButton.AddToClassList(nameof(resetButton));
            resetButton.Add(GraphSettings.ResetButtonIcon);
            resetButton.tooltip = "Reset this value back to the default value.";
            container.Add(resetButton);
        }

        private void ValueChanged(SerializedPropertyChangeEvent evt) {
            Settings.NotifyValueChanged(evt);
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            return Settings ? new GraphSettingsProvider() : null;
        }

        public override Type GetDataType() {
            return typeof(GraphSettings);
        }

        public override dynamic GetInstance() {
            return Settings;
        }

        protected override void OnChange() {
            Save();
        }
    }
}