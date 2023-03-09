using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    /// <summary>
    /// Setting sproviders are neeeded to show up settings in the ProjectSettings window.
    /// This is an abstract base class to help with some of the heavy lifting to get this to work.
    /// </summary>
    public abstract class SettingsProviderBase : SettingsProvider {

        /// <summary>
        /// The serialized object to operate on.
        /// </summary>
        protected SerializedObject serializedObject;

        public SettingsProviderBase(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) {}

        /// <summary>
        /// Get the instance to the specific opject that should be managed
        /// </summary>
        /// <returns></returns>
        public abstract dynamic GetInstance();

        /// <summary>
        /// Get the data type of the Settings we should operate on
        /// </summary>
        /// <returns></returns>
        public abstract System.Type GetDataType();

        /// <summary>
        /// Provide a Root Property Path, this allows you to skip several property levels if they are not needed.
        /// Default is null which means that all properties will be drawn.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetRootPropertyPath() {
            return null;
        }

        /// <summary>
        /// Provide a callback that is fired when any property changed.
        /// </summary>
        /// <returns></returns>
        protected virtual EventCallback<SerializedPropertyChangeEvent> GetValueChangedCallback() {
            return null;
        }

        /// <summary>
        /// Provide an additional UI creation action on a per row/propertyfield basis.
        /// </summary>
        /// <returns></returns>
        protected virtual System.Action<SerializedProperty,VisualElement> GetCreateAdditionalUIAction() {
            return null;
        }

        /// <summary>
        /// Provide a specific header/label for the Settings.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetHeader() {
            if (serializedObject != null) {
                return serializedObject.targetObject.name;
            }
            return "";
        }

        /// <summary>
        /// Executed when the user clicks on the settings entry in the project settings.
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public override void OnActivate(string searchContext, VisualElement rootElement) {
            serializedObject = new SerializedObject(GetInstance());
            VisualElement settingsRoot = new VisualElement();

            var title = new Label() { text = GetHeader() };
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 10;
            title.AddToClassList("title");

            settingsRoot.style.paddingTop = settingsRoot.style.paddingBottom = settingsRoot.style.paddingRight = 2;
            settingsRoot.style.paddingLeft = 5;

            settingsRoot.Add(title);

            UIElementsHelper.CreateGenericUI(serializedObject, settingsRoot, GetValueChangedCallback(), GetCreateAdditionalUIAction(), GetRootPropertyPath());

            rootElement.Add(settingsRoot);
            settingsRoot.Bind(serializedObject);
        }

    }
}
