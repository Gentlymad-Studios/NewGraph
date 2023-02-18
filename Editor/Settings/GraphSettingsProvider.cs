using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {

    public class GraphSettingsProvider : SettingsProviderBase {

        private const string path = GraphSettings.basePathToSettingsFile + nameof(GraphSettings);
        private static readonly string[] tags = new string[] { nameof(GraphSettings), nameof(NewGraph) };

        public GraphSettingsProvider(SettingsScope scope = SettingsScope.Project)
            : base(path, scope) {
            keywords = tags;
        }

        protected override EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
            return ValueChanged;
        }

        private void ValueChanged(SerializedPropertyChangeEvent evt) {
            GraphSettings.Instance.NotifyValueChanged(evt);
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {
            return GraphSettings.Instance ? new GraphSettingsProvider() : null;
        }

        public override Type GetDataType() {
            return typeof(GraphSettings);
        }

        public override dynamic GetInstance() {
            return GraphSettings.Instance;
        }

        protected override void OnChange() {

        }
    }
}