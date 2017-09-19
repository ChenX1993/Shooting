using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using Opsive.ThirdPersonController.Abilities;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Base class for a custom Item inspector.
    /// </summary>
    public abstract class ItemInspector : InspectorBase
    {
        private static readonly string EditorPrefsKey = "Opsive.ThirdPersonController.Editor";

        // Item
        private GameObject m_AssignTo;
        private enum HandAssignment { Left, Right }
        private HandAssignment m_HandAssignment = HandAssignment.Right;
        private ItemPlacement m_ItemPlacement;

        [SerializeField] private static bool m_CharacterAnimatorFoldout = true;
        [SerializeField] private static bool m_UIFoldout = true;
        [SerializeField] private static bool m_InputFoldout = true;
        [SerializeField] private static bool m_GeneralFoldout = true;

        private Dictionary<string, ReorderableList> m_ReorderableListMap = new Dictionary<string, ReorderableList>();
        private List<ReorderableList> m_ReordableLists = new List<ReorderableList>();

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var item = target as Item;
            if (item == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Allow the user to assign the item if it isn't already assigned
            if (item.transform.parent == null) {
                m_AssignTo = EditorGUILayout.ObjectField("Assign To", m_AssignTo, typeof(GameObject), true) as GameObject;
                var enableGUI = m_AssignTo != null && m_AssignTo.GetComponent<Animator>() != null;
                if (enableGUI) {
                    if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m_AssignTo))) {
                        EditorGUILayout.HelpBox("The character must be located within the scene.", MessageType.Error);
                        enableGUI = false;
                    } else {
                        if (m_AssignTo.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand) == null) {
                            // The ItemPlacement component must be specified for generic models.
                            m_ItemPlacement = EditorGUILayout.ObjectField("Item Placement", m_ItemPlacement, typeof(ItemPlacement), true) as ItemPlacement;
                            if (m_ItemPlacement == null) {
                                EditorGUILayout.HelpBox("The ItemPlacement GameObject must be specified for Generic models.", MessageType.Error);
                                enableGUI = false;
                            }
                        } else {
                            m_HandAssignment = (HandAssignment)EditorGUILayout.EnumPopup("Hand", m_HandAssignment);
                        }
                    }
                }

                GUI.enabled = enableGUI;
                if (GUILayout.Button("Assign")) {
                    Transform itemPlacement = null;
                    if (m_AssignTo.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand) == null) {
                        itemPlacement = m_ItemPlacement.transform;
                    } else {
                        var handTransform = m_AssignTo.GetComponent<Animator>().GetBoneTransform(m_HandAssignment == HandAssignment.Left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                        itemPlacement = handTransform.GetComponentInChildren<ItemPlacement>().transform;
                    }
                    AssignItem(item.gameObject, itemPlacement);
                }
                GUI.enabled = true;
            }

            var itemTypeProperty = PropertyFromName(serializedObject, "m_ItemType");
            EditorGUILayout.PropertyField(itemTypeProperty);
            if (itemTypeProperty.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("This field is required. The Inventory uses the Item Type to determine the type of item.", MessageType.Error);
            } 

            var itemName = PropertyFromName(serializedObject, "m_ItemName");
            EditorGUILayout.PropertyField(itemName);
            if (string.IsNullOrEmpty(itemName.stringValue)) {
                EditorGUILayout.HelpBox("The Item Name specifies the name of the Animator substate machine. It should not be empty unless you only have one item type.", MessageType.Warning);
            }

            if ((m_CharacterAnimatorFoldout = EditorGUILayout.Foldout(m_CharacterAnimatorFoldout, "Character Animator Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var canAim = PropertyFromName(serializedObject, "m_CanAim");
                EditorGUILayout.PropertyField(canAim);
                if (canAim.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RequireAim"));
                }
                DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_DefaultStates"));
                if (canAim.boolValue) {
                    DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_AimStates"));
                }
                if (target is IUseableItem) {
                    DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_UseStates"));
                }
                if (target is IReloadableItem) {
                    DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_ReloadStates"));
                }
                if (target is MeleeWeapon || target is Shield) {
                    DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_RecoilStates"));
                }
                DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_EquipStates"));
                DrawAnimatorStateSet(item, m_ReorderableListMap, m_ReordableLists, OnListSelectInternal, OnListAddInternal, OnListRemoveInternal, PropertyFromName(serializedObject, "m_UnequipStates"));
                EditorGUI.indentLevel--;
            }

            if ((m_UIFoldout = EditorGUILayout.Foldout(m_UIFoldout, "UI Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ItemSprite"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RightItemSprite"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CrosshairsSprite"), true);
                EditorGUI.indentLevel--;
            }

            if ((m_InputFoldout = EditorGUILayout.Foldout(m_InputFoldout, "Input Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (item is IUseableItem) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UseInputName"));
                    if (serializedObject.FindProperty("m_DualWieldUseInputName") != null) {
                        EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DualWieldUseInputName"));
                    }
                }
                if (item is IReloadableItem) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ReloadInputName"));
                }
                if (item is IFlashlightUseable) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ToggleFlashlightInputName"));
                }
                if (item is ILaserSightUseable) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ToggleLaserSightInputName"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_GeneralFoldout = EditorGUILayout.Foldout(m_GeneralFoldout, "General Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TwoHandedItem"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_NonDominantHandPosition"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HolsterTarget"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ItemPickup"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AimCameraState"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CollisionNotification"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(item, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(item);
            }
        }

        /// <summary>
        /// Assigns the item as a child to the specified hand Transform.
        /// </summary>
        /// <param name="itemGameObject">The Item GameObject</param>
        private void AssignItem(GameObject itemGameObject, Transform itemPlacement)
        {
            var item = GameObject.Instantiate(itemGameObject) as GameObject;
            item.transform.parent = itemPlacement;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            Selection.activeGameObject = item;
        }

        /// <summary>
        /// Draws the itnerface for an AnimatorItemCollectionData.
        /// </summary>
        public static void DrawAnimatorStateSet(MonoBehaviour target, Dictionary<string, ReorderableList> reordableListMap, List<ReorderableList> reordableLists,
                                                            ReorderableList.SelectCallbackDelegate selectCallback, ReorderableList.AddCallbackDelegate addCallback,
                                                            ReorderableList.RemoveCallbackDelegate removeCallback, SerializedProperty itemGroupDataProperty)
        {
            if (EditorGUILayout.PropertyField(itemGroupDataProperty, false)) {
                EditorGUI.indentLevel++;
                var itemStateOrderDataProperty = itemGroupDataProperty.FindPropertyRelative("m_Idle");
                var indexPath = EditorPrefsKey + target + "." + itemStateOrderDataProperty.propertyPath;
                var foldout = false;
                if ((foldout = EditorGUILayout.Foldout(EditorPrefs.GetBool(indexPath, true), "Idle"))) {
                    EditorGUI.indentLevel++;
                    DrawAnimatorSet(target, reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, itemStateOrderDataProperty, false);
                    EditorGUI.indentLevel--;
                }
                EditorPrefs.SetBool(indexPath, foldout);

                itemStateOrderDataProperty = itemGroupDataProperty.FindPropertyRelative("m_Movement");
                indexPath = EditorPrefsKey + target + "." + itemStateOrderDataProperty.propertyPath;
                if ((foldout = EditorGUILayout.Foldout(EditorPrefs.GetBool(indexPath, true), "Movement"))) {
                    EditorGUI.indentLevel++;
                    DrawAnimatorSet(target, reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, itemStateOrderDataProperty, false);
                    EditorGUI.indentLevel--;
                }
                EditorPrefs.SetBool(indexPath, foldout);

                itemStateOrderDataProperty = itemGroupDataProperty.FindPropertyRelative("m_Abilities");
                indexPath = EditorPrefsKey + target + "." + itemStateOrderDataProperty.propertyPath;
                if ((foldout = EditorGUILayout.Foldout(EditorPrefs.GetBool(indexPath, true), "Abilities"))) {
                    EditorGUI.indentLevel++;
                    DrawAnimatorAbilitySet(target, reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, itemStateOrderDataProperty);
                    EditorGUI.indentLevel--;
                }
                EditorPrefs.SetBool(indexPath, foldout);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the interface for an AnimatorItemSetData.
        /// </summary>
        private static void DrawAnimatorSet(MonoBehaviour target, Dictionary<string, ReorderableList> reordableListMap, List<ReorderableList> reordableLists,
                                                            ReorderableList.SelectCallbackDelegate selectCallback, ReorderableList.AddCallbackDelegate addCallback,
                                                            ReorderableList.RemoveCallbackDelegate removeCallback, SerializedProperty itemStateOrderDataProperty, bool abilitySet)
        {
            var itemGroupDataProperty = itemStateOrderDataProperty.FindPropertyRelative("m_Groups");
            var groupCount = itemGroupDataProperty.arraySize;
            // Add an extra element for the "Add New Group" element. If there are no states then add an extra element to be able to display "No States".
            var groupIndexes = new string[groupCount + 1 + (groupCount == 0 ? 1 : 0)];
            if (groupCount > 0) {
                for (int i = 0; i < groupCount; ++i) {
                    groupIndexes[i] = i.ToString();
                }
            } else {
                groupIndexes[0] = "(No Groups)";
            }
            groupIndexes[groupIndexes.Length - 1] = "Add New Group...";
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            var indexPath = EditorPrefsKey + target + "." + itemGroupDataProperty.propertyPath;
            var index = EditorGUILayout.Popup("Group Index", EditorPrefs.GetInt(indexPath, 0), groupIndexes);
            GUI.enabled = itemGroupDataProperty.arraySize > 1;
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                itemGroupDataProperty.DeleteArrayElementAtIndex(index);
                itemGroupDataProperty.serializedObject.ApplyModifiedProperties();
                index = Mathf.Max(index - 1, 0);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) {
                // "Add New" was clicked.
                if (index == groupIndexes.Length - 1) {
                    itemGroupDataProperty.InsertArrayElementAtIndex(itemGroupDataProperty.arraySize);
                    index = itemGroupDataProperty.arraySize - 1;
                }
                itemGroupDataProperty.serializedObject.ApplyModifiedProperties();
                EditorPrefs.SetInt(indexPath, index);
            }
            if (itemGroupDataProperty.arraySize > 0) {
                if (itemGroupDataProperty.arraySize > 1) {
                    EditorGUILayout.PropertyField(itemStateOrderDataProperty.FindPropertyRelative("m_GroupOrder"), false);
                }
                GUILayout.Space(7);
                index = Mathf.Clamp(index, 0, itemGroupDataProperty.arraySize - 1);
                DrawGroup(target, reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, itemGroupDataProperty.GetArrayElementAtIndex(index), abilitySet);
            }
        }

        /// <summary>
        /// Draws the interface for an AnimatorItemAbilitySetData.
        /// </summary>
        private static void DrawAnimatorAbilitySet(MonoBehaviour target, Dictionary<string, ReorderableList> reordableListMap, List<ReorderableList> reordableLists,
                                                            ReorderableList.SelectCallbackDelegate selectCallback, ReorderableList.AddCallbackDelegate addCallback,
                                                            ReorderableList.RemoveCallbackDelegate removeCallback, SerializedProperty abilityStatesProperty)
        {
            // The item must be attached to a RigidbodyCharacterController.
            RigidbodyCharacterController controller = null;
            var parent = target.transform.parent;
            while (parent != null) {
                if ((controller = parent.GetComponent<RigidbodyCharacterController>()) != null) {
                    break;
                }
                parent = parent.parent;
            }
            if (controller == null) {
                // Remove all of the ability states as they are pointing to another character.
                if (abilityStatesProperty.arraySize > 0) {
                    abilityStatesProperty.ClearArray();
                    abilityStatesProperty.serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.LabelField("Please assign the item to a character.");
                return;
            }

            // Ensure the ability states are pointing to the current character. They may not be pointing to the current character if the item was switched from one
            // GameObject to another.
            if (abilityStatesProperty.arraySize > 0) {
                var ability = abilityStatesProperty.GetArrayElementAtIndex(0).FindPropertyRelative("m_Ability").objectReferenceValue as Ability;
                if (ability == null || ability.gameObject != controller.gameObject) {
                    abilityStatesProperty.ClearArray();
                    abilityStatesProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            var abilitySet = new HashSet<Ability>();
            var duplicateAbilityNameSet = new HashSet<string>();
            var abilityStates = new List<string>();
            var newAbilityStates = new List<Ability>();
            if (abilityStatesProperty.arraySize == 0) {
                abilityStates.Add("(No Ability Groups)");
            } else {
                for (int i = 0; i < abilityStatesProperty.arraySize; ++i) {
                    var ability = abilityStatesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_Ability").objectReferenceValue;
                    // Add the priority index if the list already contains a name with the same ability name.
                    var name = ability.GetType().Name;
                    if (duplicateAbilityNameSet.Contains(name)) {
                        name += string.Format("(Priority {0})", (ability as Ability).Index);
                    } else {
                        duplicateAbilityNameSet.Add(name);
                    }
                    abilityStates.Add(name);
                    abilitySet.Add(ability as Ability);
                }
            }

            // List any abilities that exist on the controller but have not been added to the ability list yet.
            duplicateAbilityNameSet.Clear();
            var newAbilityStartIndex = abilityStates.Count;
            for (int i = 0; i < controller.Abilities.Length; ++i) {
                var ability = controller.Abilities[i];
                if (abilitySet.Contains(ability)) {
                    continue;
                }
                // Add the priority index if the list already contains a name with the same ability name.
                var name = ability.GetType().Name;
                if (duplicateAbilityNameSet.Contains(name)) {
                    name += string.Format("(Priority {0})", (ability as Ability).Index);
                } else {
                    duplicateAbilityNameSet.Add(name);
                }
                abilityStates.Add("Add Ability Group/" + name);
                newAbilityStates.Add(ability);
            }

            // Save the selected index out to EditorPrefs to ensure the selected item stays selected across serialization reloads.
            var foldoutPath = EditorPrefsKey + controller.gameObject + "." + abilityStatesProperty.propertyPath;
            var selectedIndex = EditorPrefs.GetInt(foldoutPath, 0);

            // Show the popup.
            EditorGUILayout.BeginHorizontal();
            selectedIndex = EditorGUILayout.Popup("Ability", selectedIndex, abilityStates.ToArray());

            // Only allow the delete button if the array contains at least one state.
            GUI.enabled = abilityStatesProperty.arraySize > 0;
            if (GUILayout.Button("X", GUILayout.Width(20))) {
                abilityStatesProperty.DeleteArrayElementAtIndex(selectedIndex);
                abilityStatesProperty.serializedObject.ApplyModifiedProperties();
                selectedIndex = 0;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Add a new ability state if the selected index is greater than the number of existing abilities.
            if (selectedIndex >= newAbilityStartIndex) {
                abilityStatesProperty.InsertArrayElementAtIndex(abilityStatesProperty.arraySize);
                var abilitySetProperty = abilityStatesProperty.GetArrayElementAtIndex(abilityStatesProperty.arraySize - 1);
                abilitySetProperty.FindPropertyRelative("m_Ability").objectReferenceValue = newAbilityStates[selectedIndex - newAbilityStartIndex];
                var groups = abilitySetProperty.FindPropertyRelative("m_Groups");
                // Add the default group and group state.
                if (groups.arraySize == 0) {
                    abilitySetProperty.FindPropertyRelative("m_Groups").InsertArrayElementAtIndex(0);
                    var group = abilitySetProperty.FindPropertyRelative("m_Groups").GetArrayElementAtIndex(0);
                    AddGroupState(group.FindPropertyRelative("m_States"));
                }
                abilityStatesProperty.serializedObject.ApplyModifiedProperties();
                selectedIndex = abilityStatesProperty.arraySize - 1;
            } else if (abilityStatesProperty.arraySize > 0) {
                // The ability isn't being added or removed. Show the selected ability set.
                DrawAnimatorSet(target, reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, abilityStatesProperty.GetArrayElementAtIndex(selectedIndex), true);
            }
            EditorPrefs.SetInt(foldoutPath, selectedIndex);
        }

        /// <summary>
        /// Draws the item group. Will show a ReordableList along with state details if an element is selected.
        /// </summary>
        private static void DrawGroup(MonoBehaviour target, Dictionary<string, ReorderableList> reordableListMap, List<ReorderableList> reordableLists,
                                                            ReorderableList.SelectCallbackDelegate selectCallback, ReorderableList.AddCallbackDelegate addCallback,
                                                            ReorderableList.RemoveCallbackDelegate removeCallback, SerializedProperty itemStateDataProperty, bool abilitySet)
        {
            EditorGUI.BeginChangeCheck();
            var stateOrderProperty = itemStateDataProperty.FindPropertyRelative("m_StateOrder");
            EditorGUILayout.PropertyField(stateOrderProperty);
            if (stateOrderProperty.enumValueIndex == (int)AnimatorItemGroupData.Order.Combo) {
                EditorGUILayout.PropertyField(itemStateDataProperty.FindPropertyRelative("m_ComboTimeout"));
            }
            var list = GetReorderableList(reordableListMap, reordableLists, selectCallback, addCallback, removeCallback, itemStateDataProperty.FindPropertyRelative("m_States"));

            // Indent the list so it lines up with the rest of the content.
            var rect = GUILayoutUtility.GetRect(0, list.GetHeight());
            rect.x += EditorGUI.indentLevel * 15;
            rect.xMax -= EditorGUI.indentLevel * 15;
            list.DoList(rect);

            // Show the selected state details.
            if (list.index != -1 && list.count > 0) {
                var elementProperty = itemStateDataProperty.FindPropertyRelative("m_States").GetArrayElementAtIndex(list.index);
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_Name"), false);
                if (abilitySet) {
                    EditorGUILayout.HelpBox("Leave the name empty to use the state name determined by the ability.", MessageType.Info);
                }
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_TransitionDuration"), false);
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_SpeedMultiplier"), false);
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_CanReplay"), false);
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_ItemNamePrefix"), false);
                var layerProperty = elementProperty.FindPropertyRelative("m_Layer");
                layerProperty.intValue = EditorGUILayout.MaskField(new GUIContent("Layer", layerProperty.tooltip), layerProperty.intValue, layerProperty.enumDisplayNames);
                EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_IgnoreLowerPriority"), false);
                var controller = target.GetComponentInParent<RigidbodyCharacterController>();
                if (controller != null && controller.UseRootMotion == false) {
                    EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("m_ForceRootMotion"), false);
                } else {
                    elementProperty.FindPropertyRelative("m_ForceRootMotion").boolValue = false;
                }
            }
            if (EditorGUI.EndChangeCheck()) {
                itemStateDataProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Retrieve a ReorderableList for the specified property.
        /// </summary>
        private static ReorderableList GetReorderableList(Dictionary<string, ReorderableList> reordableListMap, List<ReorderableList> reordableLists, 
                                                            ReorderableList.SelectCallbackDelegate selectCallback, ReorderableList.AddCallbackDelegate addCallback,
                                                            ReorderableList.RemoveCallbackDelegate removeCallback, SerializedProperty property)
        {
            ReorderableList list;
            if (reordableListMap.TryGetValue(property.propertyPath, out list)) {
                return list;
            }
            list = new ReorderableList(property.serializedObject, property, true, false, true, true);
            list.onSelectCallback += selectCallback;
            list.onAddCallback += addCallback;
            list.onRemoveCallback += removeCallback;
            reordableListMap.Add(property.propertyPath, list);
            reordableLists.Add(list);
            return list;
        }

        /// <summary>
        /// When one ReorderableList is selected, deselect the rest.
        /// </summary>
        private void OnListSelectInternal(ReorderableList list)
        {
            OnListSelect(m_ReordableLists, list);
        }

        /// <summary>
        /// When one ReorderableList is selected, deselect the rest.
        /// </summary>
        public static void OnListSelect(List<ReorderableList> reordableLists, ReorderableList list)
        {
            for (int i = 0; i < reordableLists.Count; ++i) {
                if (reordableLists[i] == list) {
                    continue;
                }

                reordableLists[i].index = -1;
            }
        }

        /// <summary>
        /// Fill in default values if they are not already filled in for the new element.
        /// </summary>
        private void OnListAddInternal(ReorderableList list)
        {
            OnListAdd(list);
        }

        /// <summary>
        /// Fill in default values if they are not already filled in for the new element.
        /// </summary>
        public static void OnListAdd(ReorderableList list)
        {
            var groupProperty = list.serializedProperty;
            AddGroupState(groupProperty);
            list.index = groupProperty.arraySize - 1;
        }

        /// <summary>
        /// Adds a new AnimatorStateData entry
        /// </summary>
        private static void AddGroupState(SerializedProperty groupProperty)
        {
            groupProperty.InsertArrayElementAtIndex(groupProperty.arraySize);
            // If the array size is 1 then the default values need to be filled in.
            if (groupProperty.arraySize == 1) {
                var state = groupProperty.GetArrayElementAtIndex(0);
                state.FindPropertyRelative("m_Name").stringValue = "Movement";
                state.FindPropertyRelative("m_TransitionDuration").floatValue = 0.2f;
                state.FindPropertyRelative("m_SpeedMultiplier").floatValue = 1;
                var itemNamePrefixProperty = state.FindPropertyRelative("m_ItemNamePrefix");
                // If the item name property is not null then the state type is AnimatorItemStateData
                if (itemNamePrefixProperty != null) {
                    itemNamePrefixProperty.boolValue = true;
                    state.FindPropertyRelative("m_Layer").enumValueIndex = (int)AnimatorItemStateData.AnimatorLayer.UpperBody;
                }
            }
        }

        /// <summary>
        /// Do not allow a list element to be removed if there is only one element left.
        /// </summary>
        private void OnListRemoveInternal(ReorderableList list)
        {
            OnListRemove(list);
        }

        /// <summary>
        /// Do not allow a list element to be removed if there is only one element left.
        /// </summary>
        public static void OnListRemove(ReorderableList list)
        {
            var groupProperty = list.serializedProperty;
            if (groupProperty.arraySize > 1) {
                groupProperty.DeleteArrayElementAtIndex(list.index);
                list.index = Mathf.Clamp(list.index - 1, 0, groupProperty.arraySize - 1);
            }
        }

        public static int DrawBitMaskField(int aMask, System.Type aType, GUIContent aLabel)
        {
            var itemNames = System.Enum.GetNames(aType);
            var itemValues = System.Enum.GetValues(aType) as int[];

            int val = aMask;
            int maskVal = 0;
            for (int i = 0; i < itemValues.Length; i++) {
                if (itemValues[i] != 0) {
                    if ((val & itemValues[i]) == itemValues[i])
                        maskVal |= 1 << i;
                } else if (val == 0)
                    maskVal |= 1 << i;
            }
            int newMaskVal = EditorGUILayout.MaskField(aLabel, maskVal, itemNames);
            int changes = maskVal ^ newMaskVal;

            for (int i = 0; i < itemValues.Length; i++) {
                if ((changes & (1 << i)) != 0)            // has this list item changed?
                {
                    if ((newMaskVal & (1 << i)) != 0)     // has it been set?
                    {
                        if (itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
                        {
                            val = 0;
                            break;
                        } else
                            val |= itemValues[i];
                    } else                                  // it has been reset
                      {
                        val &= ~itemValues[i];
                    }
                }
            }
            return val;
        }
    }
}