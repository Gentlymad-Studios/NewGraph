using System;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// Based on: https://github.com/Unity-Technologies/UnityCsReference/blob/2022.2/Modules/GraphViewEditor/NodeSearch/SearchWindow.cs
    /// Converted to our own class so this works even if unity decides to change the API.
    /// This is part of the infamous UnityEditor.Experimental.GraphView namespace that is considered dprecated.
    /// However there is no substitution on the horizon for a neat search window like this one.
    /// </summary>
    [Serializable]
    public class SearchTreeGroupEntry : SearchTreeEntry {
        internal int selectedIndex;
        internal Vector2 scroll;

        public SearchTreeGroupEntry(string content, Func<bool> enabledCheck, int level = 0) : base(content, enabledCheck) {
            this.level = level;
        }
    }
}