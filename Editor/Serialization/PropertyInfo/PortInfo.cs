using System;

namespace NewGraph {
    public class PortInfo : PropertyInfo {
        public PortAttribute portDisplay;
        public Type fieldType;
        public string portName = null;

        public PortInfo(string relativePropertyPath, Type fieldType, PortAttribute portDisplay, string portName = null) : base(relativePropertyPath) {
            this.portDisplay = portDisplay;
            this.fieldType = fieldType;
            this.portName = portName;
        }
    }
}
