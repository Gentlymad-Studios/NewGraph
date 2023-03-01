using System;

namespace NewGraph {
    public class PortInfo : PropertyInfo {
        public PortBaseAttribute portDisplay;
        public Type fieldType;
        public string fieldName = null;

        public PortInfo(string relativePropertyPath, Type fieldType, PortBaseAttribute portDisplay, string portName = null) : base(relativePropertyPath) {
            this.portDisplay = portDisplay;
            this.fieldType = fieldType;
            this.fieldName = portName;
        }
    }
}
