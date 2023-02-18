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
    public class SearchTreeEntry : IComparable<SearchTreeEntry> {
        public int level;
        public GUIContent content;

        public Action<object> actionToExecute;
        public Func<bool> enabledCheck;
        public bool IsEnabled;
        public object userData;

        public SearchTreeEntry(string content, Func<bool> enabledCheck, Action<object> actionToExecute = null) {
            this.content = new GUIContent(content);
            this.enabledCheck = enabledCheck;
            this.actionToExecute = actionToExecute;
        }

        public static bool AlwaysEnabled() {
            return true;
        }

        public string name {
            get { return content.text; }
        }

        public int CompareTo(SearchTreeEntry o) {
            return name.CompareTo(o.name);
        }
    }
}