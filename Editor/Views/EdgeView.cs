using GraphViewBase;
using UnityEngine;

namespace NewGraph {
    public class EdgeView : Edge {
        public EdgeView() : base() {
            EdgeWidth = EdgeWidthUnselected;
        }
        public override int EdgeWidthSelected => GraphSettings.Instance.edgeWidthSelected;
        public override int EdgeWidthUnselected => GraphSettings.Instance.edgeWidthUnselected;
        public override Color ColorSelected => GraphSettings.Instance.colorSelected;
        public override Color ColorUnselected => currentUnselectedColor;

        public Color currentUnselectedColor = GraphSettings.Instance.colorUnselected;
    }
}
