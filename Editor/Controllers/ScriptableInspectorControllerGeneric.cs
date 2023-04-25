using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {
	public class ScriptableInspectorControllerGeneric<T> : InspectorControllerBase where T : ScriptableGraphModel {

		public ScriptableInspectorControllerGeneric(VisualElement parent) : base(parent) { }

		/// <summary>
		/// Setup the create button for this inspector
		/// </summary>
		/// <param name="createButton"></param>
		public override void SetupCreateButton(Button createButton) {
			createButton.clicked += CreateButtonClicked;
			createButton.Add(GraphSettings.CreateButtonIcon);
		}

		/// <summary>
		/// called when the create button was clicked.
		/// </summary>
		private void CreateButtonClicked() {
			string fileEnding = "asset";

			// retrieve the last opened folder
			string lastOpenedDirectoryGUID = EditorPrefs.GetString(GraphSettings.lastOpenedDirectoryPrefsKey, null);
			string lastFolder = "";
			if (lastOpenedDirectoryGUID != null) {
				lastFolder = AssetDatabase.GUIDToAssetPath(lastOpenedDirectoryGUID);
				if (lastFolder == null) {
					lastFolder = "";
				}
			}

			// generate a unique filename that avoids conflicts
			string uniqueFilename = Path.Combine(lastFolder, typeof(T).Name + "." + fileEnding);
			if (File.Exists(uniqueFilename)) {
				uniqueFilename = AssetDatabase.GenerateUniqueAssetPath(uniqueFilename);
			}
			uniqueFilename = Path.GetFileName(uniqueFilename);

			// show the dialog
			string path = EditorUtility.SaveFilePanel("Create New Graph", lastFolder, uniqueFilename, fileEnding);
			if (path.Length != 0) {
				UnityEngine.Object graphData = ScriptableObject.CreateInstance<T>();
				path = path.Substring(Application.dataPath.Length - 6);

				// save the last selcted folder
				string folder = AssetDatabase.AssetPathToGUID(Path.GetDirectoryName(path));
				EditorPrefs.SetString(GraphSettings.lastOpenedDirectoryPrefsKey, folder);

				AssetDatabase.CreateAsset(graphData, path);
				AssetDatabase.SaveAssets();

				IGraphModelData graphModel = graphData as IGraphModelData;
				graphModel.CreateSerializedObject();

				CreateRenameGraphUI(graphModel);
				Clear();

				OnAfterGraphCreated?.Invoke(graphModel);
			}
		}

		public override void SetupLoadButton(Button loadButton) {
			loadButton.clicked += LoadButtonClicked;
			loadButton.Add(GraphSettings.LoadButtonIcon);
		}

		/// <summary>
		/// Called when the load button was clicked.
		/// </summary>
		private void LoadButtonClicked() {
			currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
			EditorGUIUtility.ShowObjectPicker<T>(null, false, "", currentPickerWindow);
			pickerActive = true;
		}

		/// <summary>
		/// Implement renaming of the current graph.
		/// This create an EditableLabelElement so renaming UX is consistent.
		/// </summary>
		/// <param name="graph">The graph that renaming should operate on</param>
		public override void CreateRenameGraphUI(IGraphModelData graph) {
			T graphModel = graph as T;

			// avoid some redudancy when setting string property...
			void SetStringValueAndApply(SerializedProperty prop, string otherValue) {
				prop.stringValue = otherValue;
				prop.serializedObject.ApplyModifiedProperties();
			}

			// clear all former ui elements
			inspectorHeader.Unbind();
			inspectorHeader.Clear();

			// retrieve the "real" name of the scriptable object;
			graphModel.SerializedGraphData.Update();
			SerializedProperty assetNameProperty = graphModel.GetOriginalNameProperty().Copy();

			// get our "fake" property that is just there to avoid overwriting the "real" name which Unity really doesn't like...
			SerializedProperty tmpNameProperty = graphModel.GetTmpNameProperty().Copy();
			SetStringValueAndApply(tmpNameProperty, assetNameProperty.stringValue);

			// create the property field our EditableLabelElement will operate on
			PropertyField labelField = new PropertyField(tmpNameProperty);
			inspectorHeader.Add(labelField);
			inspectorHeader.Bind(graphModel.SerializedGraphData);

			// create the label and start the rename procedure when we exited the label edit mode
			EditableLabelElement editableLabel = new EditableLabelElement(labelField, onEditModeLeft: () => {
				string graphAssetPath = AssetDatabase.GetAssetPath(graph.BaseObject);
				string originalName = Path.GetFileNameWithoutExtension(graphAssetPath);
				string name = tmpNameProperty.stringValue;

				// if the graph is an actual asset and the name has changed..
				if (graphAssetPath != null && originalName != name) {
					// check if we were dealt a shit hand... (empty string or invalid characters)
					if (!string.IsNullOrWhiteSpace(name) && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) {
						// get the directory of the path
						string pathToDirectory = Path.GetDirectoryName(graphAssetPath);
						string uniquePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(pathToDirectory, name));
						string uniqueFilename = Path.GetFileNameWithoutExtension(uniquePath);

						// GenerateUniqueAssetPath will handle duplicates so lets inform the user that adjustments were made!
						if (name != uniqueFilename) {
							Logger.LogAlways($"The provided name was already present or not valid! Changing {name} to {uniqueFilename}!");
						}

						// update our fake property, rename the real asset, refresh the database
						SetStringValueAndApply(tmpNameProperty, uniqueFilename);
						AssetDatabase.RenameAsset(graphAssetPath, uniqueFilename);
						AssetDatabase.Refresh();
						return;
					}

					// we were dealt a shit hand, so lets revert the name back to the original and inform the user!
					Logger.LogAlways($"The provided name ({name}) was empty or not a valid filename!");
					SetStringValueAndApply(tmpNameProperty, originalName);
				}
			});
		}
	}
}
