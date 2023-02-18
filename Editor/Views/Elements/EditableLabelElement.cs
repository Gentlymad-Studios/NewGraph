using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace NewGraph {
    /// <summary>
    /// This is a converter for PropertyField elements, to convert a default Text/ string field into an editable label
    /// </summary>
    public class EditableLabelElement {
        private Image editButton;
        private VisualElement inputField;
        private VisualElement textElement;
        private PropertyField propertyField;
        public bool isInEditMode = false;
        private bool isInitialized = false;
        private System.Action<string> onValueChange;
        private System.Action onEditModeLeft;
        private const string inputFieldUSSClass = "unity-base-text-field__input";
        private const string editIcon = "d_editicon.sml";
        private Texture editIconTexture;
        private const string closeIcon = "d_winbtn_win_close";
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

            // check if the propertyfield value changed
            propertyField.RegisterCallback<ChangeEvent<string>>((value) => {
                this.onValueChange?.Invoke(value.newValue);
            });
            propertyField.AddToClassList(editableLabelUSSClass);

            // wait until the visualtree of the property has been built
            propertyField.RegisterCallback<GeometryChangedEvent>(InitializeAfterVisualTreeReady);
        }

        /// <summary>
        /// Called after the visual tree of the propertyfield was built. This way we can traverse through all children and apply changes.
        /// </summary>
        /// <param name="e"></param>
        private void InitializeAfterVisualTreeReady(GeometryChangedEvent e) {
            if (propertyField.childCount > 0 && propertyField[0].childCount > 1 && propertyField[0][1].childCount > 0) {
                inputField = propertyField[0][1];
                textElement = inputField[0];
                editButton = new Image() { image = editIconTexture };

                editButton.RegisterCallback<ClickEvent>(OnEditButtonClick);
                propertyField.Add(editButton);
                isInitialized = true;
                EnableInput(false);
                propertyField.UnregisterCallback<GeometryChangedEvent>(InitializeAfterVisualTreeReady);
                //propertyField.style.display = DisplayStyle.None;
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
                    editButton.image = isInEditMode ? closeIconTexture : editIconTexture;
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
