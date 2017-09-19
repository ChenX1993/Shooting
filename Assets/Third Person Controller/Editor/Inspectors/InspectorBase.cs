using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Base class for all Third Person Controller inspectors.
    /// </summary>
    public abstract class InspectorBase : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> m_PropertyStringMap = new Dictionary<string, SerializedProperty>();
        
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null)
                return; // How'd this happen?

            // Show the script field.
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Script"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Inspector");
                serializedObject.ApplyModifiedProperties();
                if (!target.Equals(null)) {
                    InspectorUtility.SetObjectDirty(target);
                }
            }
        }

        /// <summary>
        /// Uses a dictionary to lookup a property from a string key.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The found SerializedProperty.</returns>
        public SerializedProperty PropertyFromName(SerializedObject serializedObject, string name)
        {
            SerializedProperty property = null;
            if (m_PropertyStringMap.TryGetValue(name, out property)) {
                return property;
            }

            property = serializedObject.FindProperty(name);
            if (property == null) {
                Debug.LogError("Unable to find property " + name);
                return null;
            }
            m_PropertyStringMap.Add(name, property);
            return property;
        }
    }
}