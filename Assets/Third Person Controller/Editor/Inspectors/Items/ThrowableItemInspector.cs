using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ThrowableItem.
    /// </summary>
    [CustomEditor(typeof(ThrowableItem))]
    public class ThrowableItemInspector : ItemInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var throwableItem = target as ThrowableItem;
            if (throwableItem == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanUseInAir"));
            var thrownObject = PropertyFromName(serializedObject, "m_ThrownObject");
            EditorGUILayout.PropertyField(thrownObject);
            if (thrownObject.objectReferenceValue == null || (thrownObject.objectReferenceValue as GameObject).GetComponent(typeof(IThrownObject)) == null) {
                EditorGUILayout.HelpBox("This field is required. The object must implement the IThrownObject interface.", MessageType.Error);
            }
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ThrowRate"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ThrowForce"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ThrowTorque"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Spread"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(throwableItem, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(throwableItem);
            }
        }
    }
}