using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    public abstract class SettingsProviderBase : SettingsProvider {

        protected SerializedObject serializedObject;

        public SettingsProviderBase(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) {}

        public abstract dynamic GetInstance();
        public abstract System.Type GetDataType();
        protected abstract void OnChange();
        protected virtual bool OnCustomChangeCheck() { return false; }
        protected virtual EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
            return null;
        }

        // This function is called when the user clicks on the MyCustom element in the Settings window.
        public override void OnActivate(string searchContext, VisualElement rootElement) {
            serializedObject = new SerializedObject(GetInstance());
            VisualElement settingsRoot = new VisualElement();

            var title = new Label() { text = serializedObject.targetObject.name };
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.AddToClassList("title");

            settingsRoot.style.paddingTop = settingsRoot.style.paddingBottom = settingsRoot.style.paddingRight = 2;
            settingsRoot.style.paddingLeft = 5;

            settingsRoot.Add(title);


            UIElementsHelper.CreateGenericUI(serializedObject, GetDataType(), settingsRoot, GetValueChangedCallback());

            rootElement.Add(settingsRoot);
            settingsRoot.Bind(serializedObject);
        }

    }
}
