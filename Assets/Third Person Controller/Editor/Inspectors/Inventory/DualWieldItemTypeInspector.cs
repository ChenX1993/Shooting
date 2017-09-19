using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for PrimaryItemType.
    /// </summary>
    [CustomEditor(typeof(DualWieldItemType))]
    public class DualWieldItemTypeInspector : ItemTypeInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var dualWieldItemType = target as DualWieldItemType;
            if (dualWieldItemType == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_PrimaryItem"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(dualWieldItemType, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(dualWieldItemType);
            }
        }
    }
}