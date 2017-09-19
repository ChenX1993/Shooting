using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for PickupObject.
    /// </summary>
    public abstract class PickupObjectInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var itemPickup = target as PickupObject;
            if (itemPickup == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_PickupSound"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TriggerEnableDelay"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_PickupOnTriggerEnter"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(itemPickup, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(itemPickup);
            }
        }
    }
}