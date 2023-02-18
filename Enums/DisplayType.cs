using System;

namespace NewGraph {
    [Flags]
    public enum DisplayType { 
        Unspecified = 0,
        Hide = 1,
        Inspector = 2,
        NodeView = 4,
        BothViews = Inspector | NodeView
    };
}