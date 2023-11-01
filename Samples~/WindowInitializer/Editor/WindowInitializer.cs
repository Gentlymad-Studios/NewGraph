using NewGraph;
using UnityEditor;

namespace WindowInitializer {
	public class WindowInitializer {
		/// <summary>
		/// Here you can customize where the graph window menu should appear
		/// </summary>
		[MenuItem("Tools/" + nameof(GraphWindow))]
		static void OpenWindow() {
			// Here you can define what type of graph model you would like to use.
			GraphWindow.InitializeWindowBase(typeof(ScriptableGraphModel));
		}
	}
}