using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NewGraph {
    public static class UIElementsHelper {

        public static void CreateGenericUI<T>(SerializedObject serializedObject, VisualElement root, EventCallback<SerializedPropertyChangeEvent> changeCallback = null, bool createGroup = false) {
            CreateGenericUI(serializedObject, typeof(T), root, changeCallback, createGroup);
        }

        public static void CreateGenericUI(SerializedObject serializedObject, Type type, VisualElement root, EventCallback<SerializedPropertyChangeEvent> changeCallback = null, bool createGroup = false) {
            List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
            SerializationHelper.RetrieveAllSerializedProperties(ref serializedProperties, type, serializedObject);

            void ForEachProperty(ref VisualElement parent) {
                foreach (SerializedProperty property in serializedProperties) {
                    PropertyField propertyField = new PropertyField(property);
                    if (changeCallback != null) {
                        propertyField.RegisterValueChangeCallback(changeCallback);
                    }
                    parent.Add(propertyField);
                }
            }

            if (createGroup) {
                VisualElement group = new VisualElement();
                ForEachProperty(ref group);
                root.Add(group);
                group.Bind(serializedObject);
            } else {
                ForEachProperty(ref root);
                root.Bind(serializedObject);
            }

        }
    }
}
