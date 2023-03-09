using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {
    public static class UIElementsHelper {

        public static PropertyField CreatePropertyFieldWithCallback(SerializedProperty property, EventCallback<SerializedPropertyChangeEvent> changeCallback = null) {
            PropertyField propertyField = new PropertyField(property);
            if (changeCallback != null) {
                propertyField.RegisterValueChangeCallback(changeCallback);
            }
            return propertyField;
        }

        public static void CreateGenericUI(SerializedObject serializedObject, VisualElement root, EventCallback<SerializedPropertyChangeEvent> changeCallback = null, System.Action<SerializedProperty, VisualElement> CreateAdditionalUI = null) {

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
                SerializedProperty prop = serializedObject.GetIterator();
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
            root.Bind(serializedObject);

        }
    }
}
