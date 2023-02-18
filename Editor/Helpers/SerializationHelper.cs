using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace NewGraph {
    public static class SerializationHelper {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        
        public static List<SerializedProperty> RetrieveAllSerializedProperties<T>(SerializedObject serializedObject) {
            List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
            RetrieveAllSerializedProperties<T>(ref serializedProperties, serializedObject);
            return serializedProperties;
        }

        public static void RetrieveAllSerializedProperties<T>(ref List<SerializedProperty> serializedProperties, SerializedObject serializedObject) {
            RetrieveAllSerializedProperties(ref serializedProperties, typeof(T), serializedObject);
        }

        public static void RetrieveAllSerializedProperties(ref List<SerializedProperty> serializedProperties, Type type, SerializedObject serializedObject) {
            serializedProperties.Clear();
            FieldInfo[] fields = type.GetFields(Flags);
            SerializedProperty prop;
            foreach (FieldInfo info in fields) {
                prop = serializedObject.FindProperty(info.Name);
                if (prop != null) {
                    serializedProperties.Add(prop);
                }
            }
        }

        /// <summary>
        /// Get a property by its name along a properties path.
        /// Be aware: This breaks down easily if encapsulated properties have the same name!
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static SerializedProperty GetPropertyAlongPath(SerializedProperty property, string propertyName, bool getParent = false) {
            char delimiter = '.';
            string[] propertyPathPartials = property.propertyPath.Split(delimiter);
            string propertyPathToBaseNodeItem = "";
            int i;
            // go through all partials
            for (i = 0; i < propertyPathPartials.Length; i++) {
                // check if the current partials is equal to the property
                // this would mean we have found the property
                if (propertyPathPartials[i] == propertyName) {
                    break;
                }
            }
            if (i == propertyPathPartials.Length) {
                return null;
            }

            // to get to the search property we need to add +1 to the index so it itself is included!
            if (!getParent) {
                i++;
            }

            for (int j = 0; j < i; j++) {
                propertyPathToBaseNodeItem += propertyPathPartials[j] + delimiter;
            }
            
            // remove last '.'
            propertyPathToBaseNodeItem = propertyPathToBaseNodeItem.Remove(propertyPathToBaseNodeItem.Length - 1);

            // get the actual property based on the attached main object
            return property.serializedObject.FindProperty(propertyPathToBaseNodeItem);
        }
    }

}