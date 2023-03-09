using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {
    public static class UIElementsHelper {

        /// <summary>
        /// Create a PropertyField with a callback.
        /// </summary>
        /// <param name="property">SerializedProperty that should be used to create the PropertyField.</param>
        /// <param name="changeCallback">Callback that gets triggered if a value changed.</param>
        /// <returns></returns>
        public static PropertyField CreatePropertyFieldWithCallback(SerializedProperty property, EventCallback<SerializedPropertyChangeEvent> changeCallback = null) {
            PropertyField propertyField = new PropertyField(property);
            if (changeCallback != null) {
                propertyField.RegisterValueChangeCallback(changeCallback);
            }
            return propertyField;
        }

        /// <summary>
        /// This is an alternative to InspectorElement.FillDefaultInspector that works without having to provide an editor object.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="root">The UI root object.</param>
        /// <param name="changeCallback">Optional: Callback fired when a property changed.</param>
        /// <param name="CreateAdditionalUI">Optional: A method executed to create additional UI for every row.</param>
        /// <param name="rootPropertyPath">Optional: A relative property path that can be provided to start with a different property inside the serializedObject.</param>
        public static void CreateGenericUI(SerializedObject serializedObject, VisualElement root, EventCallback<SerializedPropertyChangeEvent> changeCallback = null, System.Action<SerializedProperty, VisualElement> CreateAdditionalUI = null, string rootPropertyPath = null) {

            SerializedProperty rootProperty = null;
            if (rootPropertyPath != null) {
                rootProperty = serializedObject.FindProperty(rootPropertyPath);
            }

            Action<SerializedProperty, ScrollView> creationLogic;
            if (CreateAdditionalUI != null) {
                creationLogic = (prop, scrollView) => {
                    VisualElement container = new VisualElement();
                    container.AddToClassList(nameof(container)+nameof(PropertyField));
                    container.Add(CreatePropertyFieldWithCallback(prop, changeCallback));
                    CreateAdditionalUI(prop, container);
                    scrollView.Add(container);
                };
            } else {
                creationLogic = (prop, scrollView) => {
                    scrollView.Add(CreatePropertyFieldWithCallback(prop, changeCallback));
                };
            }

            void ForEachProperty(ref ScrollView scrollView) {
                SerializedProperty prop = rootProperty == null ? serializedObject.GetIterator(): rootProperty;
                if (prop.NextVisible(true)) {
                    do {
                        if (prop.name != "m_Script") {
                            creationLogic(prop.Copy(), scrollView);
                        }
                    }
                    while (prop.NextVisible(false));
                }
            }

            ScrollView scrollView = new ScrollView();
            scrollView.AddToClassList("propertyList"+nameof(ScrollView));
            root.Add(scrollView);
            ForEachProperty(ref scrollView);

        }
    }
}
