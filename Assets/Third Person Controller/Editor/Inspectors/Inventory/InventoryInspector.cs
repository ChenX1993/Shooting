using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Inventory.
    /// </summary>
    [CustomEditor(typeof(Inventory))]
    public class InventoryInspector : InspectorBase
    {
        // Inventory
        [SerializeField] private static bool m_DefaultLoadoutFoldout = true;
        [SerializeField] private static bool m_ItemOrderFoldout = true;

        // Internal variables
        private ReorderableList m_ReordableDefaultLoadout;
        private ReorderableList m_ReordableItemOrder;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var inventory = target as Inventory;
            if (inventory == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UnlimitedAmmo"));
            var unequippedItemType = PropertyFromName(serializedObject, "m_UnequippedItemType");
            EditorGUILayout.PropertyField(unequippedItemType);
            if (unequippedItemType.objectReferenceValue != null) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanSwitchToUnequippedItemType"));
                EditorGUI.indentLevel--;
            }

            var dropItemsOnDeath = PropertyFromName(serializedObject, "m_DropItems");
            EditorGUILayout.PropertyField(dropItemsOnDeath);
            if (dropItemsOnDeath.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DroppedItemsParent"));
                EditorGUI.indentLevel--;
            }

            if ((m_DefaultLoadoutFoldout = EditorGUILayout.Foldout(m_DefaultLoadoutFoldout, "Default Loadout", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (m_ReordableDefaultLoadout == null) {
                    var defaultLoadoutProperty = PropertyFromName(serializedObject, "m_DefaultLoadout");
                    m_ReordableDefaultLoadout = new ReorderableList(serializedObject, defaultLoadoutProperty, true, true, true, true);
                    m_ReordableDefaultLoadout.drawHeaderCallback = OnItemAmountHeaderDraw;
                    m_ReordableDefaultLoadout.drawElementCallback = OnItemAmountElementDraw;
                }
                m_ReordableDefaultLoadout.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            if ((m_ItemOrderFoldout = EditorGUILayout.Foldout(m_ItemOrderFoldout, "Item Order", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (m_ReordableItemOrder == null) {
                    var itemOrderProperty = PropertyFromName(serializedObject, "m_ItemOrder");
                    m_ReordableItemOrder = new ReorderableList(serializedObject, itemOrderProperty, true, false, true, true);
                    m_ReordableItemOrder.drawElementCallback = OnItemOrderDraw;
                }
                m_ReordableItemOrder.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(inventory, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(inventory);
            }
        }

        /// <summary>
        /// Draws the ItemAmount ReordableList header.
        /// </summary>
        private void OnItemAmountHeaderDraw(Rect rect)
        {
            ItemAmountInspector.OnItemAmountHeaderDraw(rect);
        }

        /// <summary>
        /// Draws the ItemAmount ReordableList element.
        /// </summary>
        private void OnItemAmountElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemAmountInspector.OnItemAmountElementDraw(m_ReordableDefaultLoadout, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draws the ItemOrder ReordableList element.
        /// </summary>
        private void OnItemOrderDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            var order = m_ReordableItemOrder.serializedProperty.GetArrayElementAtIndex(index);
            order.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, rect.width, EditorGUIUtility.singleLineHeight),
                                                                        order.objectReferenceValue, typeof(ItemType), true) as ItemType;

            if (EditorGUI.EndChangeCheck()) {
                var serializedObject = m_ReordableItemOrder.serializedProperty.serializedObject;
                Undo.RecordObject(serializedObject.targetObject, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(serializedObject.targetObject);
            }
        }
    }
}