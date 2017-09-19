using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for StaticItem.
    /// </summary>
    [CustomEditor(typeof(StaticItem))]
    public class StaticItemInspector : ItemInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var staticItem = target as StaticItem;
            if (staticItem == null || staticItem == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(staticItem, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(staticItem);
            }
        }
    }
}