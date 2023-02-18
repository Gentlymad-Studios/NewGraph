using System.Collections.Generic;

namespace NewGraph {
    public class GroupInfo : PropertyInfo {
        public string groupName;
        public List<PropertyInfo> graphProperties = new List<PropertyInfo>();

        public GroupInfo(string groupName, string relativePropertyPath) : base(relativePropertyPath) {
            this.groupName = groupName;
        }
    }
}
