using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for CharacterFootsteps.
    /// </summary>
    [CustomEditor(typeof(CharacterFootsteps))]
    public class CharacterFootstepsInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var characterFootsteps = target as CharacterFootsteps;
            if (characterFootsteps == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();
            
            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Feet"), true);
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_PerFootSounds"), true);
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Footsteps"), true);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(characterFootsteps, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(characterFootsteps);
            }
        }
    }
}