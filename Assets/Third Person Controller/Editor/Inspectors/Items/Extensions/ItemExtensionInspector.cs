using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Opsive.ThirdPersonController.Abilities;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Class for a custom ItemExtension inspector.
    /// </summary>
    public class ItemExtensionInspector : InspectorBase
    {
        [SerializeField] private static bool m_CharacterAnimatorFoldout = true;
        [SerializeField] private static bool m_InputFoldout = true;

        private Dictionary<string, ReorderableList> m_ReorderableListMap = new Dictionary<string, ReorderableList>();
        private List<ReorderableList> m_ReordableLists = new List<ReorderableList>();

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var itemExtension = target as ItemExtension;
            if (itemExtension == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // An item must exist to be able to extend it.
            if (itemExtension.GetComponent<Item>() == null) {
                EditorGUILayout.HelpBox("An Item component is required. Please run the Item Builder and create your item.", MessageType.Error);
                return;
            }

            var itemTypeProperty = PropertyFromName(serializedObject, "m_ConsumableItemType");
            EditorGUILayout.PropertyField(itemTypeProperty);
            if (itemTypeProperty.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("This field is required. The Inventory uses the Consumable Item Type to determine the type of item.", MessageType.Error);
            }

            if ((m_CharacterAnimatorFoldout = EditorGUILayout.Foldout(m_CharacterAnimatorFoldout, "Character Animator Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                ItemInspector.DrawAnimatorStateSet(itemExtension, m_ReorderableListMap, m_ReordableLists, OnListSelect, OnListAdd, OnListRemove, PropertyFromName(serializedObject, "m_UseStates"));
                if (itemExtension is IReloadableItem) {
                    ItemInspector.DrawAnimatorStateSet(itemExtension, m_ReorderableListMap, m_ReordableLists, OnListSelect, OnListAdd, OnListRemove, PropertyFromName(serializedObject, "m_ReloadStates"));
                }
                if (target is MeleeWeaponExtension) {
                    ItemInspector.DrawAnimatorStateSet(itemExtension, m_ReorderableListMap, m_ReordableLists, OnListSelect, OnListAdd, OnListRemove, PropertyFromName(serializedObject, "m_RecoilStates"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_InputFoldout = EditorGUILayout.Foldout(m_InputFoldout, "Input Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (itemExtension is IUseableItem) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UseInputName"));
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(itemExtension, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(itemExtension);
            }
        }

        /// <summary>
        /// When one ReorderableList is selected, deslect the rest.
        /// </summary>
        private void OnListSelect(ReorderableList list)
        {
            ItemInspector.OnListSelect(m_ReordableLists, list);
        }

        /// <summary>
        /// When one ReorderableList is selected, deslect the rest.
        /// </summary>
        private void OnListAdd(ReorderableList list)
        {
            ItemInspector.OnListAdd(list);
        }

        /// <summary>
        /// Do not allow a list element to be removed if there is only one element left.
        /// </summary>
        private void OnListRemove(ReorderableList list)
        {
            ItemInspector.OnListRemove(list);
        }
    }
}