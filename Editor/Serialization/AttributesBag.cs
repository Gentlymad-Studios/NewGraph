using OdinSerializer.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NewGraph {
    public class AttributesBag {
        private const BindingFlags AllBindingFlags = (BindingFlags)(-1);
        public object[] attributes = new object[] { };
        public Type type = null;

        /// <summary>
        /// Retrieve all attributes by a given propertyPath string (from a SerializedProperty)
        /// </summary>
        /// <param name="targetObjectType"></param>
        /// <param name="relativePropPath"></param>
        /// <param name="inherit"></param>
        public void GetAttributes(Type targetObjectType, string relativePropPath, bool inherit) {
            attributes = new object[] { };
            type = null;

            FieldInfo fieldInfo = null;
            System.Reflection.PropertyInfo propertyInfo = null;
            IEnumerable<Type> allTypes;

            foreach (var name in relativePropPath.Split('.')) {

                allTypes = targetObjectType.GetBaseTypes(true);
                foreach (Type type in allTypes) {
                    fieldInfo = type.GetField(name, AllBindingFlags);
                    if (fieldInfo != null) {
                        break;
                    }
                }

                if (fieldInfo == null) {
                    propertyInfo = targetObjectType.GetProperty(name, AllBindingFlags);
                    if (propertyInfo == null) {
                        return;
                    }
                    targetObjectType = propertyInfo.PropertyType;
                } else {
                    targetObjectType = fieldInfo.FieldType;
                }
            }

            if (fieldInfo != null) {
                type = fieldInfo.FieldType;

                attributes = fieldInfo.GetCustomAttributes(inherit);

            } else if (propertyInfo != null) {
                type = propertyInfo.PropertyType;
                attributes = propertyInfo.GetCustomAttributes(inherit);
            }
        }
    }
}
