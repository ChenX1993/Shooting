using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Inventory.ItemAmount.
    /// </summary>
    public static class ItemAmountInspector
    {
        /// <summary>
        /// Draws the ItemAmount ReordableList header.
        /// </summary>
        public static void OnItemAmountHeaderDraw(Rect rect)
        {
            var objFieldWidth = rect.width - 190;
            EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, objFieldWidth, EditorGUIUtility.singleLineHeight), "Type");
            EditorGUI.LabelField(new Rect(rect.x + objFieldWidth, rect.y, 50, EditorGUIUtility.singleLineHeight), "Equip");
            EditorGUI.LabelField(new Rect(rect.x + objFieldWidth + 50, rect.y, 70, EditorGUIUtility.singleLineHeight), "Amount");
            EditorGUI.LabelField(new Rect(rect.x + objFieldWidth + 120, rect.y, 70, EditorGUIUtility.singleLineHeight), "Infinity");
        }

        /// <summary>
        /// Draws the ItemAmount ReordableList element.
        /// </summary>
        public static void OnItemAmountElementDraw(ReorderableList reordableList, Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            var loadout = reordableList.serializedProperty.GetArrayElementAtIndex(index);
            var itemType = loadout.FindPropertyRelative("m_ItemType");
            var amount = loadout.FindPropertyRelative("m_Amount");
            var objFieldWidth = rect.width - 190;
            itemType.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, objFieldWidth, EditorGUIUtility.singleLineHeight),
                                                                        itemType.objectReferenceValue, typeof(ItemType), true) as ItemType;
            if (GUI.changed && itemType.objectReferenceValue is PrimaryItemType) {
                loadout.FindPropertyRelative("m_Equip").boolValue = true;
            }
            // DualWieldItemTypes cannot be directly picked up.
            if (itemType.objectReferenceValue is DualWieldItemType) {
                itemType.objectReferenceValue = null;
            }
            GUI.enabled = itemType.objectReferenceValue is PrimaryItemType;
            var equip = loadout.FindPropertyRelative("m_Equip");
            equip.boolValue = EditorGUI.Toggle(new Rect(rect.x + objFieldWidth, rect.y, 50, EditorGUIUtility.singleLineHeight), equip.boolValue);
            var prevInfinity = (amount.intValue == int.MaxValue);
            GUI.enabled = !prevInfinity;
            if (itemType.objectReferenceValue is PrimaryItemType) {
                amount.intValue = Mathf.Min(EditorGUI.IntField(new Rect(rect.x + objFieldWidth + 50, rect.y, 70, EditorGUIUtility.singleLineHeight),
                                                                    Mathf.Max(amount.intValue, 1)), 2);
            } else {
                amount.intValue = EditorGUI.IntField(new Rect(rect.x + objFieldWidth + 50, rect.y, 70, EditorGUIUtility.singleLineHeight),
                                                                    amount.intValue);
            }

            GUI.enabled = !(itemType.objectReferenceValue is PrimaryItemType);
            var infinity = EditorGUI.Toggle(new Rect(rect.x + objFieldWidth + 120, rect.y, 70, EditorGUIUtility.singleLineHeight), prevInfinity);
            if (prevInfinity != infinity) {
                if (infinity) {
                    amount.intValue = int.MaxValue;
                } else {
                    amount.intValue = 1;
                }
            }
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck()) {
                var serializedObject = reordableList.serializedProperty.serializedObject;
                Undo.RecordObject(serializedObject.targetObject, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(serializedObject.targetObject);
            }
        }
    }
}