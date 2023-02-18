using System;

namespace NewGraph {
    [Flags]
    public enum Editability { 
        Unspecified = 0,
        None = 1,
        Inspector = 2, 
        NodeView = 4, 
        BothViews = Inspector | NodeView
    };
}
