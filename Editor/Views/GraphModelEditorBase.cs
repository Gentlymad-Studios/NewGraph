using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewGraph {

    using static GraphSettingsSingleton;

    public class GraphModelEditorBase : Editor {

		public override VisualElement CreateInspectorGUI() {
            VisualElement inspector = new VisualElement();
            inspector.AddToClassList("baseGraphEditor");

            Button openGraphButton = new Button(OpenGraphClicked) { text = Settings.openGraphButtonText };
            openGraphButton.Add(GraphSettings.LoadButtonIcon);
			openGraphButton.AddToClassList("openGraphButton");

			inspector.Add(openGraphButton);
            inspector.styleSheets.Add(GraphSettings.graphStylesheetVariables);
            inspector.styleSheets.Add(GraphSettings.graphStylesheet);

			CreateGUI(inspector);

            return inspector;
        }

        protected virtual void CreateGUI(VisualElement inspector) {
			IGraphModelData baseGraphModel = target as IGraphModelData;
			if (baseGraphModel.SerializedGraphData == null) {
				baseGraphModel.CreateSerializedObject();
			}

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
				if (i < baseGraphModel.Nodes.Count && i >= 0) {
					NodeModel node = baseGraphModel.Nodes[i];
					Label label = itemRow[0] as Label;
                    if (node != null) {
						label.text = $"Element {i + 1}: {node.GetName()}";
                    }
                }
            };

            ListView listView = new ListView() {
                showAddRemoveFooter = false,
                reorderable = false,
                showFoldoutHeader = false,
                showBorder = true,
				showBoundCollectionSize = false,
				showAlternatingRowBackgrounds = AlternatingRowBackground.All,
				itemsSource = baseGraphModel.Nodes,
				bindItem = BindItem,
                makeItem = MakeItem
            };
			listView.AddToClassList("plainNodeList");

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

