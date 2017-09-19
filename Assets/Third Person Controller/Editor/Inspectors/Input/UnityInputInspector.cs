using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Opsive.ThirdPersonController.Input;

namespace Opsive.ThirdPersonController.Editor.Input
{
    /// <summary>
    /// Shows a custom inspector for PlayerInput.
    /// </summary>
    [CustomEditor(typeof(UnityInput))]
    public class UnityInputInspector : InspectorBase
    {
        /// <summary>
        /// The elements axis type within the InputManager.
        /// </summary>
        private enum AxisType
        {
            KeyMouseButton, Mouse, Joystick
        }
        /// <summary>
        /// The element's axis number within the InputManager.
        /// </summary>
        private enum AxisNumber
        {
            X, Y, Three, Four, Five, Six, Seven, Eight, Nine, ten
        }

        private static Dictionary<string, int> m_FoundAxes;
        
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var playerInput = target as PlayerInput;
            if (playerInput == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1 || UNITY_BLACKBERRY)
            var mobileInput = PropertyFromName(serializedObject, "m_ForceMobileInput");
            mobileInput.boolValue = false;
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ForceStandaloneInput"));
#else
            var standaloneInput = PropertyFromName(serializedObject, "m_ForceStandaloneInput");
            standaloneInput.boolValue = false;
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ForceMobileInput"));
#endif
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DisableWithEscape"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DisableCursor"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DisableWhenButtonDown"));

            EditorGUILayout.HelpBox("Selecting Update will change Unity's Input Manager project file to work with third person controls.", MessageType.Info);
            if (GUILayout.Button("Update")) {
                UpdateInputManager();
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(playerInput, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(playerInput);
            }
        }

        /// <summary>
        /// Update the Input Manager to add all of the correct controls.
        /// </summary>
        public static void UpdateInputManager()
        {
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = serializedObject.FindProperty("m_Axes");

            m_FoundAxes = new Dictionary<string, int>();

            // Unity defined axis:
            AddInputAxis(axesProperty, "Horizontal", "left", "right", "a", "d", 1000, 0.001f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Vertical", "down", "up", "s", "w", 1000, 0.001f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Fire1", "", "left ctrl", "", "mouse 0", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Fire2", "", "", "", "mouse 1", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Jump", "", "space", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0, 0.1f, false, false, AxisType.Mouse, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0, 0.1f, false, false, AxisType.Mouse, AxisNumber.Y);
            AddInputAxis(axesProperty, "Horizontal", "", "", "", "", 1000, 0.19f, 1, false, false, AxisType.Joystick, AxisNumber.X);
            AddInputAxis(axesProperty, "Vertical", "", "", "", "", 1000, 0.19f, 1, false, true, AxisType.Joystick, AxisNumber.Y);
            AddInputAxis(axesProperty, "Fire1", "", "", "", "", 1000, 0.001f, 1000, false, false, AxisType.Joystick, AxisNumber.Six);
            AddInputAxis(axesProperty, "Fire2", "", "", "", "", 1000, 0.001f, 1000, false, false, AxisType.Joystick, AxisNumber.Three);
            AddInputAxis(axesProperty, "Jump", "", "joystick button 0", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0.19f, 1, false, false, AxisType.Joystick, AxisNumber.Four);
            AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0.19f, 1, false, true, AxisType.Joystick, AxisNumber.Five);

            // New axis:
            AddInputAxis(axesProperty, "Change Speeds", "", "left shift", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Crouch", "", "c", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Reload", "", "r", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Next Item", "", "e", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Previous Item", "", "q", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Item Toggle", "", "t", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Secondary", "", "g", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Action", "", "f", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Secondary", "", "", "", "", 1000, 0.001f, 1000, false, false, AxisType.Joystick, AxisNumber.Seven);
            AddInputAxis(axesProperty, "Action", "", "joystick button 1", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Reload", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Next Item", "", "joystick button 3", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Crouch", "", "joystick button 9", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip First Item", "", "1", "", "[1]", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Second Item", "", "2", "", "[2]", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Third Item", "", "3", "", "[3]", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Fourth Item", "", "4", "", "[4]", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Fifth Item", "", "5", "", "[5]", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Item Wheel", "", "tab", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Flashlight", "", "v", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Laser Sight", "", "b", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Roll", "", "x", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Drop Dual Wield Item", "", "v", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Extension Fire1", "", "v", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "FlyDive", "", "z", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds a new axis to the InputManager.
        /// </summary>
        /// <param name="axesProperty">The array of all of the axes.</param>
        /// <param name="name">The name of the new axis.</param>
        /// <param name="negativeButton">The name of the negative button of the new axis.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="altNegativeButton">The name of the alternative negative button of the new axis.</param>
        /// <param name="altPositiveButton">The name of the alternative positive button of the new axis.</param>
        /// <param name="sensitivity">The sensitivity of the new axis.</param>
        /// <param name="gravity">The gravity of the new axis.</param>
        /// <param name="dead">The dead value of the new axis.</param>
        /// <param name="snap">Does the new axis snap?</param>
        /// <param name="axisType">The type of axis to add.</param>
        private static void AddInputAxis(SerializedProperty axesProperty, string name, string negativeButton, string positiveButton,
                                string altNegativeButton, string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType axisType, AxisNumber axisNumber)
        {
            var property = FindAxisProperty(axesProperty, name);
            property.FindPropertyRelative("m_Name").stringValue = name;
            property.FindPropertyRelative("negativeButton").stringValue = negativeButton;
            property.FindPropertyRelative("positiveButton").stringValue = positiveButton;
            property.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
            property.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
            property.FindPropertyRelative("gravity").floatValue = gravity;
            property.FindPropertyRelative("dead").floatValue = dead;
            property.FindPropertyRelative("sensitivity").floatValue = sensitivity;
            property.FindPropertyRelative("snap").boolValue = snap;
            property.FindPropertyRelative("invert").boolValue = invert;
            property.FindPropertyRelative("type").intValue = (int)axisType;
            property.FindPropertyRelative("axis").intValue = (int)axisNumber;
        }

        /// <summary>
        /// Searches for a property with the given name and axis type within the axes property array. If no property is found then a new one will be created.
        /// </summary>
        /// <param name="axesProperty">The array to search through.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns></returns>
        private static SerializedProperty FindAxisProperty(SerializedProperty axesProperty, string name)
        {
            SerializedProperty foundProperty = null;
            // As new axes are being added make sure a previous axis is not overwritten because the name matches.
            var existingCount = 0;
            m_FoundAxes.TryGetValue(name, out existingCount);
            var localCount = 0;
            for (int i = 0; i < axesProperty.arraySize; ++i) {
                var property = axesProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name)) {
                    if (localCount == existingCount) {
                        foundProperty = property;
                        break;
                    }
                    localCount++;
                }
            }
            if (existingCount == 0) {
                m_FoundAxes.Add(name, 1);
            } else {
                m_FoundAxes[name] = existingCount + 1;
            }
            
            // If no property was found then create a new one.
            if (foundProperty == null) {
                axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
                foundProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            }

            return foundProperty;
        }
    }
}