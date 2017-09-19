using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for PrimaryItemType.
    /// </summary>
    [CustomEditor(typeof(PrimaryItemType))]
    public class PrimaryItemTypeInspector : ItemTypeInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var primaryItemType = target as PrimaryItemType;
            if (primaryItemType == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            primaryItemType.ConsumableItem.ItemType = EditorGUILayout.ObjectField("Consumable Item", primaryItemType.ConsumableItem.ItemType, typeof(ConsumableItemType), false) as ConsumableItemType;
            DrawConsumableItem(primaryItemType.ConsumableItem);
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AdditionalConsumableItems"), true);
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DualWieldItems"), true);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(primaryItemType, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(primaryItemType);
            }
        }

        /// <summary>
        /// Draws the consumable item.
        /// </summary>
        private void DrawConsumableItem(PrimaryItemType.UseableConsumableItem consumableItem)
        {
            if (consumableItem.ItemType != null) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                var prevInfinity = (consumableItem.Capacity == int.MaxValue);
                GUI.enabled = !prevInfinity;
                consumableItem.Capacity = EditorGUILayout.IntField("Capacity", consumableItem.Capacity);
                GUI.enabled = true;
                var infinity = EditorGUILayout.ToggleLeft("Infinity", prevInfinity, GUILayout.Width(70));
                if (prevInfinity != infinity) {
                    if (infinity) {
                        consumableItem.Capacity = int.MaxValue;
                    } else {
                        consumableItem.Capacity = 1;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }
    }
}