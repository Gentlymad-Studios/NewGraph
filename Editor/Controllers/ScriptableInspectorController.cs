using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

	public class ScriptableInspectorController : ScriptableInspectorControllerGeneric<ScriptableGraphModel> {
		public ScriptableInspectorController(VisualElement parent) : base(parent) {}
	}
}
