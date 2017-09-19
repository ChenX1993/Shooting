using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for SecondaryItemType.
    /// </summary>
    [CustomEditor(typeof(SecondaryItemType))]
    public class SecondaryItemTypeInspector : ItemTypeInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var secondaryItemType = target as SecondaryItemType;
            if (secondaryItemType == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            var prevInfinity = (secondaryItemType.GetCapacity() == int.MaxValue);
            GUI.enabled = !prevInfinity;
            secondaryItemType.Capacity = EditorGUILayout.IntField("Capacity", secondaryItemType.GetCapacity()) ;
            GUI.enabled = true;
            var infinity = EditorGUILayout.ToggleLeft("Infinity", prevInfinity, GUILayout.Width(70));
            if (prevInfinity != infinity) {
                if (infinity) {
                    secondaryItemType.Capacity = int.MaxValue;
                } else {
                    secondaryItemType.Capacity = 1;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(secondaryItemType, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(secondaryItemType);
            }
        }
    }
}