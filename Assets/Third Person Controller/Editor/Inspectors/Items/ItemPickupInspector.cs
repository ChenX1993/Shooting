using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ItemPickup.
    /// </summary>
    [CustomEditor(typeof(ItemPickup))]
    public class ItemPickupInspector : PickupObjectInspector
    {
        // Internal variables
        private ReorderableList m_ReordableItemAmount;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var itemPickup = target as ItemPickup;
            if (itemPickup == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (m_ReordableItemAmount == null) {
                var itemListProperty = PropertyFromName(serializedObject, "m_ItemList");
                m_ReordableItemAmount = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                m_ReordableItemAmount.drawHeaderCallback = OnItemAmountHeaderDraw;
                m_ReordableItemAmount.drawElementCallback = OnItemAmountElementDraw;
            }
            m_ReordableItemAmount.DoLayoutList();

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(itemPickup, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(itemPickup);
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
            ItemAmountInspector.OnItemAmountElementDraw(m_ReordableItemAmount, rect, index, isActive, isFocused);
        }
    }
}