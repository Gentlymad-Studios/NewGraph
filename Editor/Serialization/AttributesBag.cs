using System;
using System.Reflection;
using UnityEditor;

namespace NewGraph {
    public class AttributesBag {

        private delegate FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty aProperty, out Type aType);
        private static GetFieldInfoAndStaticTypeFromProperty m_GetFieldInfoAndStaticTypeFromProperty;

        public object[] attributes = new object[] { };
        public Type type = null;

        /// <summary>
        /// get attributes directly by the property
        /// </summary>
        /// <param name="property"></param>
        /// <param name="inherit"></param>
        public void GetAttributes(SerializedProperty property, bool inherit) {
            FieldInfo fieldInfo = GetFieldInfoAndStaticType(property, out _);
            if (fieldInfo != null) {
                type = fieldInfo.FieldType;
                attributes = fieldInfo.GetCustomAttributes(inherit);
            }
        }

        /// <summary>
        /// Get the field info and type directly by the property.
        /// Uses an internal unity method.
        /// Based on: https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-6432620
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static FieldInfo GetFieldInfoAndStaticType(SerializedProperty prop, out Type type) {
            if (m_GetFieldInfoAndStaticTypeFromProperty == null) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    foreach (var t in assembly.GetTypes()) {
                        if (t.Name == "ScriptAttributeUtility") {
                            MethodInfo mi = t.GetMethod(nameof(GetFieldInfoAndStaticTypeFromProperty), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            m_GetFieldInfoAndStaticTypeFromProperty = (GetFieldInfoAndStaticTypeFromProperty)Delegate.CreateDelegate(typeof(GetFieldInfoAndStaticTypeFromProperty), mi);
                            break;
                        }
                    }
                    if (m_GetFieldInfoAndStaticTypeFromProperty != null) break;
                }
                if (m_GetFieldInfoAndStaticTypeFromProperty == null) {
                    UnityEngine.Debug.LogError("GetFieldInfoAndStaticType::Reflection failed!");
                    type = null;
                    return null;
                }
            }
            return m_GetFieldInfoAndStaticTypeFromProperty(prop, out type);
        }
    }
}
