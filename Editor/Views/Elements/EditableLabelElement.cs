using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace NewGraph {
    /// <summary>
    /// This is a converter for PropertyField elements, to convert a default Text/ string field into an editable label
    /// </summary>
    public class EditableLabelElement {
        private Button editButton;
        private Image editButtonImg;
        private VisualElement inputField;
        private TextField textField;
        private VisualElement textElement;
        private PropertyField propertyField;
        public bool isInEditMode = false;
        private bool isInitialized = false;
        private System.Action<string> onValueChange;
        private System.Action onEditModeLeft;
        private const string inputFieldUSSClass = "unity-base-text-field__input";
        private const string editButtonUSSClass = "editButton";
        private const string editIcon = "d_editicon.sml";
        private Texture editIconTexture;
#if UNITY_6000_0_OR_NEWER
		private const string closeIcon = "CrossIcon";
#else
		private const string closeIcon = "d_winbtn_win_close";
#endif
		private Texture closeIconTexture;
        private const string editableLabelUSSClass = nameof(EditableLabelElement);

        public EditableLabelElement(PropertyField propertyField, System.Action<string> onValueChange = null, System.Action onEditModeLeft = null) {
            // retrieve icon textures from unitys build in texture pool
            //https://github.com/halak/unity-editor-icons
            editIconTexture = EditorGUIUtility.IconContent(editIcon).image;
            closeIconTexture = EditorGUIUtility.IconContent(closeIcon).image;

            this.propertyField = propertyField;
            this.onValueChange = onValueChange;
            this.onEditModeLeft = onEditModeLeft;

			void ChangeEvent(ChangeEvent<string> evt) {
				this.onValueChange?.Invoke(evt.newValue);
			}

            // check if the propertyfield value changed
            propertyField.RegisterCallback<ChangeEvent<string>>(ChangeEvent);
            propertyField.AddToClassList(editableLabelUSSClass);

			void DetachFromPanelEvent(DetachFromPanelEvent evt) {
				propertyField.UnregisterCallback<ChangeEvent<string>>(ChangeEvent);
				propertyField.UnregisterCallback<DetachFromPanelEvent>(DetachFromPanelEvent);
			}
			propertyField.RegisterCallback<DetachFromPanelEvent>(DetachFromPanelEvent);

			// wait until the visualtree of the property has been built
			propertyField.RegisterCallback<GeometryChangedEvent>(InitializeAfterVisualTreeReady);
        }

        /// <summary>
        /// Called after the visual tree of the propertyfield was built. This way we can traverse through all children and apply changes.
        /// </summary>
        /// <param name="e"></param>
        private void InitializeAfterVisualTreeReady(GeometryChangedEvent e) {
            if (propertyField.childCount > 0 && propertyField[0].childCount > 1 && propertyField[0][1].childCount > 0) {
                textField = propertyField[0] as TextField;
                textField.RegisterCallback<KeyDownEvent>(OnKeyDown);

				void DetachFromPanelEventTxtFld(DetachFromPanelEvent evt) {
					textField.UnregisterCallback<KeyDownEvent>(OnKeyDown);
					textField.UnregisterCallback<DetachFromPanelEvent>(DetachFromPanelEventTxtFld);
				}
				textField.RegisterCallback<DetachFromPanelEvent>(DetachFromPanelEventTxtFld);


				inputField = textField[1];

                textElement = inputField[0];
                editButtonImg = new Image() { image = editIconTexture };
                editButton = new Button();
                editButton.AddToClassList(editButtonUSSClass);
                editButton.Add(editButtonImg);

                editButton.RegisterCallback<ClickEvent>(OnEditButtonClick);

				void DetachFromPanelEventEditBtn(DetachFromPanelEvent evt) {
					//removed this, because the editor button in inspector isnt called anymore after the second time opening it
                    //editButton.UnregisterCallback<ClickEvent>(OnEditButtonClick);
					editButton.UnregisterCallback<DetachFromPanelEvent>(DetachFromPanelEventEditBtn);
				}
				editButton.RegisterCallback<DetachFromPanelEvent>(DetachFromPanelEventEditBtn);

				propertyField.Add(editButton);
                isInitialized = true;
                EnableInput(false);
                propertyField.UnregisterCallback<GeometryChangedEvent>(InitializeAfterVisualTreeReady);
                //propertyField.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Executed when a key was doen in the inputfield
        /// </summary>
        /// <param name="evt"></param>
        private void OnKeyDown(KeyDownEvent evt) {
            // only do something when we are in edit mode and enter was pressed
            // enter/escape = exit edit mode
            if (isInitialized && isInEditMode && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Escape)) {
                EnableInput(false);
                evt.StopImmediatePropagation();
            }
        }

        /// <summary>
        /// Executed when the edit button is pressed
        /// </summary>
        /// <param name="e"></param>
        private void OnEditButtonClick(ClickEvent e) {
            EnableInput(!isInEditMode);
        }

        /// <summary>
        /// Main method to enable or disable the editablitly of the underlying text
        /// </summary>
        /// <param name="enable"></param>
        public void EnableInput(bool enable = true) {
            if (isInitialized) {
                if (enable != isInEditMode) {
                    isInEditMode = enable;
                    editButtonImg.image = isInEditMode ? closeIconTexture : editIconTexture;
                }

                // prevent selection if the field is currently disabled
                inputField.pickingMode = enable ? PickingMode.Position : PickingMode.Ignore;
                inputField[0].pickingMode = inputField.pickingMode;
                propertyField.pickingMode = inputField.pickingMode;
                inputField.focusable = enable;
                inputField.SetEnabled(enable); 
                inputField.Focus();

                if (!enable) {
                    onEditModeLeft?.Invoke();
                }
            }
        }
    }
}
