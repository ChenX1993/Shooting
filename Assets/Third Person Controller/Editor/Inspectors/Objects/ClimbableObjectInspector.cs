using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ClimbableObject.
    /// </summary>
    [CustomEditor(typeof(ClimbableObject))]
    public class ClimbableObjectInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var climbableObject = target as ClimbableObject;
            if (climbableObject == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            var climbableType = PropertyFromName(serializedObject, "m_ClimbableType");
            EditorGUILayout.PropertyField(climbableType);
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanReverseMount"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_BottomMountOffset"));
            if (climbableType.enumValueIndex != (int)ClimbableObject.ClimbableType.Pipe) {
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TopMountOffset"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TopMountCompleteOffset"));
            }
            if (climbableType.enumValueIndex == (int)ClimbableObject.ClimbableType.Ladder) {
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RungSeparation"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UnuseableTopRungs"));
            } else if (climbableType.enumValueIndex == (int)ClimbableObject.ClimbableType.Vine) {
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HorizontalPadding"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TopDismountOffset"));
            } else { // Pipe.
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MountPositions"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HorizontalTransitionOffset"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_VerticalTransitionOffset"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ExtraForwardDistance"));
            }
            if (climbableType.enumValueIndex != (int)ClimbableObject.ClimbableType.Ladder) {
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_BottomDismountOffset"));
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(climbableObject, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(climbableObject);
            }
        }
    }
}