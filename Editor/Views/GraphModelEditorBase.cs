using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    using static GraphSettingsSingleton;

    public class GraphModelEditorBase : Editor {
        private SerializedProperty listProperty;
		protected IGraphModelData baseGraphModel;
		protected IGraphModelData BaseGraphModel {
			get {
				if (baseGraphModel == null) {
					baseGraphModel = target as IGraphModelData;
					if (baseGraphModel.SerializedGraphData == null) {
						baseGraphModel.CreateSerializedObject();
					}
				}
				return baseGraphModel;
			}
		}

		public override VisualElement CreateInspectorGUI() {
            VisualElement inspector = new VisualElement();
            inspector.AddToClassList("baseGraphEditor");

            Button openGraphButton = new Button(OpenGraphClicked) { text = Settings.openGraphButtonText };
            openGraphButton.Add(GraphSettings.LoadButtonIcon);
            inspector.Add(openGraphButton);
            inspector.styleSheets.Add(GraphSettings.graphStylesheetVariables);
            inspector.styleSheets.Add(GraphSettings.graphStylesheet);

            CreateGUI(inspector);

            return inspector;
        }

        protected virtual void CreateGUI(VisualElement inspector) {
            listProperty = BaseGraphModel.GetNodesProperty(false);

            VisualElement MakeItem() {
                VisualElement itemRow = new VisualElement();
                Label fieldLabel = new Label();
                fieldLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                itemRow.style.flexDirection = FlexDirection.Row;
                itemRow.Add(fieldLabel);
                itemRow.SetEnabled(false);
                return itemRow;
            }

            void BindItem(VisualElement itemRow, int i) {
                serializedObject.Update();
                if (i < listProperty.arraySize && i >= 0) {
                    SerializedProperty prop = listProperty.GetArrayElementAtIndex(i);
                    Label label = itemRow[0] as Label;
                    if (prop != null) {
                        SerializedProperty propRelative = prop.FindPropertyRelative(NodeModel.nameIdentifier);
                        if (propRelative != null) {
                            label.text = $"Element {i + 1}: {propRelative.stringValue}";
                        }
                    }
                }
            };

            ListView listView = new ListView() {
                showAddRemoveFooter = false,
                reorderable = false,
                showFoldoutHeader = false,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                bindingPath = listProperty.propertyPath,
                bindItem = BindItem,
                makeItem = MakeItem
            };

            inspector.Add(listView);
        }

        private void OpenGraphClicked() {
            IGraphModelData baseGraphModel = target as IGraphModelData;
            OpenGraph(baseGraphModel);
        }

        protected static void OpenGraph(IGraphModelData graphModel) {
            GraphWindow.LoadGraph(graphModel);
        }

    }
}

