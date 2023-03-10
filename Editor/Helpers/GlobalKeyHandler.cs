using System;
using UnityEditor;
using UnityEngine;

namespace NewGraph {
    /// <summary>
    /// Thanks to https://forum.unity.com/threads/4-6-editorapplication-modifierkeyschanged-how-to-find-out-which-key-was-pressed.357367/#post-2705846
    /// </summary>
    public static class GlobalKeyEventHandler {
        public static event Action<Event> OnKeyEvent;
        public static bool RegistrationSucceeded = false;
        private const string globalEventHandler = nameof(globalEventHandler);

        static GlobalKeyEventHandler() {
            RegistrationSucceeded = false;
            string msg = "";
            try {
                System.Reflection.FieldInfo info = typeof(EditorApplication).GetField(globalEventHandler, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (info != null) {
                    EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

                    value -= OnKeyPressed;
                    value += OnKeyPressed;

                    info.SetValue(null, value);

                    RegistrationSucceeded = true;
                } else {
                    msg = $"{globalEventHandler} not found.";
                }
            } catch (Exception e) {
                msg = e.Message;
            } finally {
                if (!RegistrationSucceeded) {
                    Debug.LogWarning($"{nameof(GlobalKeyEventHandler)}: error while registering for {globalEventHandler}: " + msg);
                }
            }
        }

        private static void OnKeyPressed() {
            OnKeyEvent?.Invoke(Event.current);
        }
    }
}