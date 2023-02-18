namespace NewGraph {
    /// <summary>
    /// Allows us to display simple headers within the same level and without encapsulated entries
    /// </summary>
    public class InlineHeaderEntry : SearchTreeEntry {
        public InlineHeaderEntry(string content) : base(content, AlwaysEnabled) {}
    }
}