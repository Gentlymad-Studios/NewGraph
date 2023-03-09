using GraphViewBase;
using UnityEngine;
using static NewGraph.GraphSettingsSingleton;

namespace NewGraph {
    public class EdgeView : Edge {
        public EdgeView() : base() {
            EdgeWidth = EdgeWidthUnselected;
        }
        public override int EdgeWidthSelected => Settings.edgeWidthSelected;
        public override int EdgeWidthUnselected => Settings.edgeWidthUnselected;
        public override Color ColorSelected => Settings.colorSelected;
        public override Color ColorUnselected => currentUnselectedColor;

        public Color currentUnselectedColor = Settings.colorUnselected;
    }
}
