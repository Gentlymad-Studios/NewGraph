using System;
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

        protected override string GetRootPropertyPath() {
            return nameof(GraphSettingsAsset.graphSettings);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            base.OnActivate(searchContext, rootElement);
            
            // create a button to reset all properties back to the bleuprint value
            Button resetAll = new Button(() => {
                CopyAllFromBlueprint(serializedObject);
            });
            resetAll.text = Settings.resetAllLabel;
            resetAll.tooltip = Settings.resetAllTooltip;
            resetAll.AddToClassList(nameof(resetAll));
            rootElement[0].Insert(1, resetAll);
            // add a custom stylesheet
            rootElement.styleSheets.Add(GraphSettings.settingsStylesheet);
        }

        /// <summary>
        /// Executed after every PropertyField.
        /// We'll attach a reset button here.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="container"></param>
        private void CreateAdditionalUI(SerializedProperty property, VisualElement container) {
            // create a resetButton per Row
            Button resetButton= new Button(() => {
                // if reset is executed, copy the property from the blueprint to the property
                CopyFromBlueprint(property);
            });
            resetButton.AddToClassList(nameof(resetButton));
            resetButton.Add(GraphSettings.ResetButtonIcon);
            resetButton.tooltip = Settings.resetButtonTooltip;
            container.Add(resetButton);
        }

        /// <summary>
        /// Called when any value changed.
        /// </summary>
        /// <param name="evt"></param>
        private void ValueChanged(SerializedPropertyChangeEvent evt) {
            // notify all listeneres (ReactiveSettings)
            Settings.NotifyValueChanged(evt);
            serializedObject.ApplyModifiedProperties();
            // call save on our singleton as it is a strange hybrid and not a full ScriptableObject
            Save();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            return Settings != null ? new GraphSettingsProvider() : null;
        }

        protected override string GetHeader() {
            return nameof(GraphSettings);
        }

        public override Type GetDataType() {
            return typeof(GraphSettingsAsset);
        }

        public override dynamic GetInstance() {
            return GraphSettingsSingleton.instance.settingsAsset;
        }

    }
}