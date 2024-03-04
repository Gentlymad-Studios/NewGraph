using GraphViewBase;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace NewGraph {

    using static GraphSettingsSingleton;

    public class PortView : BasePort {

        public Type type;
        public SerializedProperty boundProperty;
        public List<Type> connectableTypes;

        private Action connectionChangedCallback;
        private Func<Type,Type, bool> isValidConnectionCheck;
        private UnityEngine.Color targetColor;

        public PortView(PortInfo info, SerializedProperty boundProperty, Action connectionChangedCallback=null) : base(Orientation.Horizontal, (Direction)(int)info.portDisplay.direction, (PortCapacity)(int)info.portDisplay.capacity) {
            this.type = info.fieldType;
            this.isValidConnectionCheck = info.portDisplay.isValidConnectionCheck;
            this.connectableTypes = info.connectableTypes;
            this.boundProperty = boundProperty;
            this.connectionChangedCallback = connectionChangedCallback;

            PortColor = DefaultPortColor;
        }

        public override UnityEngine.Color DefaultPortColor => Settings.portColor;
        public override UnityEngine.Color DisabledPortColor => Settings.disabledPortColor;

        public void SetConnectionChangedCallback(Action callback) {
            connectionChangedCallback = callback;
            connectionChangedCallback();
        }

        public override void Connect(BaseEdge edge) {
            base.Connect(edge);
            if (edge.Input != null && edge.Output != null) {
                //Logger.Log("Connect");
                PortView inputPort = edge.Input as PortView;
                PortView outputPort = edge.Output as PortView;

                if (outputPort.boundProperty.managedReferenceValue != inputPort.boundProperty.managedReferenceValue) {
                    //Logger.Log("Connect: change values");
                    //Undo.RegisterCompleteObjectUndo(outputPort.boundProperty.serializedObject.targetObject, "Add Connection");
                    //outputPort.boundProperty.managedReferenceValue = inputPort.boundProperty.managedReferenceValue;
                    outputPort.boundProperty.managedReferenceId = inputPort.boundProperty.managedReferenceId;
                    //EditorUtility.SetDirty(outputPort.boundProperty.serializedObject.targetObject);
                    outputPort.boundProperty.serializedObject.ApplyModifiedProperties();
                    outputPort.connectionChangedCallback?.Invoke();
                    connectionChangedCallback?.Invoke();
                }
                ColorizeEdgeAndPort(edge as EdgeView);
            }
        }

        public void Reset() {
			// set its value to null = remove reference
			if (boundProperty != null) {
				boundProperty.managedReferenceValue = null;
				boundProperty.serializedObject.ApplyModifiedProperties();
				connectionChangedCallback?.Invoke();
			}
        }

        public override bool CanConnectTo(BasePort other, bool ignoreCandidateEdges = true) {
            PortView outputPort;
            PortView inputPort;
            
            if (Direction == Direction.Output) {
                outputPort = this;
                inputPort = (PortView)other;
            } else {
                outputPort = (PortView)other;
                inputPort = this;
            }

            if(!isValidConnectionCheck(inputPort.type, outputPort.type)) {
                return false;
            }
            return base.CanConnectTo(other, ignoreCandidateEdges);
        }

        private void ColorizeEdgeAndPort(EdgeView edge) {
            Type nodeType = (edge.Input as PortView).type;
            targetColor = NodeModel.GetNodeAttribute(nodeType).color;
            edge.currentUnselectedColor = (targetColor == default) ? Settings.colorUnselected : targetColor;
            edge.InputColor = edge.OutputColor = edge.currentUnselectedColor;

            if (edge.Input != null) {
                targetColor = (targetColor == default) ? edge.Input.DefaultPortColor : targetColor;
                edge.Input.PortColor = targetColor;
                if (edge.Output != null) {
                    edge.Output.PortColor = targetColor;
                }
            }
        }

        public override BaseEdge ConnectTo(BasePort other) {
            EdgeView edge = base.ConnectTo(other) as EdgeView;
            ColorizeEdgeAndPort(edge);
            return edge;
        }
    }
}
