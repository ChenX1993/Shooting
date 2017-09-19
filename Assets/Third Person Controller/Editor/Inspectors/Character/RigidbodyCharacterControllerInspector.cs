using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Opsive.ThirdPersonController.Abilities;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for RigidbodyCharacterController.
    /// </summary>
    [CustomEditor(typeof(RigidbodyCharacterController))]
    public class RigidbodyCharacterControllerInspector : InspectorBase
    {
        private static string s_MultiplayerSymbol = "ENABLE_MULTIPLAYER";
        private static Regex s_CamelCaseRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        // RigidbodyCharacterController
        [SerializeField] private static bool m_MovementFoldout = true;
        [SerializeField] private static bool m_RestrictionsFoldout = true;
        [SerializeField] private static bool m_ItemFoldout = true;
        [SerializeField] private static bool m_CollisionFoldout = true;
        [SerializeField] private static bool m_PhysicsFoldout = true;
        [SerializeField] private static bool m_AbilityFoldout = true;
        private Dictionary<int, SerializedObject> m_SerializedObjectMap = new Dictionary<int, SerializedObject>();
        private RigidbodyCharacterController m_Controller;
        private static List<Type> m_AbilityTypes = new List<Type>();
        private Dictionary<Type, bool> m_AbilityIsUniqueMap = new Dictionary<Type, bool>();
        private Dictionary<string, string> m_CamelCaseSplit = new Dictionary<string, string>();
        private SerializedProperty m_Abilities;
        private ReorderableList m_ReorderableAbilityList;
        private GenericMenu m_AddMenu;

        /// <summary>
        /// Initializes the InspectorUtility and searches for ability types.
        /// </summary>
        private void OnEnable()
        {
            // Search through all of the assemblies to find any types that derive from Ability.
            m_AbilityTypes.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Loop through and store all of the possible ability types.
            var allAbilityTypes = new List<Type>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from Ability.
                    if (!typeof(Ability).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    allAbilityTypes.Add(assemblyTypes[j]);
                }
            }

            // Add the ability to the ability types list if there is not a wrapper class which goes along with the current ability.
            for (int i = 0; i < allAbilityTypes.Count; ++i) {
                if (!string.IsNullOrEmpty(allAbilityTypes[i].Namespace) && allAbilityTypes[i].Namespace.Equals("Opsive.ThirdPersonController.Abilities")) {
                    var hasWrapper = false;
                    for (int j = 0; j < allAbilityTypes.Count; ++j) {
                        if (allAbilityTypes[i].Name.Equals(allAbilityTypes[j].Name) &&
                            !string.IsNullOrEmpty(allAbilityTypes[j].Namespace) && allAbilityTypes[j].Namespace.Equals("Opsive.ThirdPersonController.Wrappers.Abilities")) {
                            hasWrapper = true;
                            break;
                        }
                    }
                    if (hasWrapper) {
                        continue;
                    }
                }

                m_AbilityTypes.Add(allAbilityTypes[i]);
            }
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            m_Controller = target as RigidbodyCharacterController;
            if (m_Controller == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Ensure the correct multiplayer symbol is defined.
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
            var hasNetworkIdentity = (target as MonoBehaviour).GetComponent<UnityEngine.Networking.NetworkIdentity>() != null;
            var showNetworkToggle = false;
            var removeSymbol = false;
#if ENABLE_MULTIPLAYER
            if (!hasNetworkIdentity) {
                EditorGUILayout.HelpBox("ENABLE_MULTIPLAYER is defined but no NetworkIdentity can be found. This symbol needs to be removed.", MessageType.Error);
                showNetworkToggle = true;
                removeSymbol = true;
            }
#else
            if (hasNetworkIdentity) {
                EditorGUILayout.HelpBox("A NetworkIdentity was found but ENABLE_MULTIPLAYER is not defined. This symbol needs to be defined.", MessageType.Error);
                showNetworkToggle = true;
            }
#endif
            if (!EditorApplication.isCompiling && showNetworkToggle && GUILayout.Button((removeSymbol ? "Remove" : "Add") + " Multiplayer Symbol")) {
                ToggleMultiplayerSymbol();
            }
#endif

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MovementType"));
            if ((m_MovementFoldout = EditorGUILayout.Foldout(m_MovementFoldout, "Movement Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var useRootMotion = PropertyFromName(serializedObject, "m_UseRootMotion");
                EditorGUILayout.PropertyField(useRootMotion);
                if (!useRootMotion.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_GroundSpeed"));
                } else {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RootMotionSpeedMultiplier"));
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_GroundDampening"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AirSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AirDampening"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RotationSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AimRotationSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TorsoLookThreshold"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_LocalCoopCharacter"));
                if (m_Controller.Movement == RigidbodyCharacterController.MovementType.Pseudo3D || m_Controller.Movement == RigidbodyCharacterController.MovementType.TopDown) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_LookInMoveDirection"));
                }
                var alignToGround = PropertyFromName(serializedObject, "m_AlignToGround");
                EditorGUILayout.PropertyField(alignToGround);
                if (alignToGround.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AlignToGroundRotationSpeed"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AlignToGroundDepthOffset"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_CollisionFoldout = EditorGUILayout.Foldout(m_CollisionFoldout, "Collision Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxStepHeight"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_StepOffset"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_StepSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_SlopeLimit"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_StopMovementThreshold"));
                EditorGUI.indentLevel--;
            }

            if ((m_PhysicsFoldout = EditorGUILayout.Foldout(m_PhysicsFoldout, "Physics Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_SkinWidth"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MovingPlatformSkinWidth"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_GroundStickiness"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CollisionPointLayerIgnore"));
                if (m_Controller.GetComponent<CapsuleCollider>() == null) {
                    var capsuleCollider = PropertyFromName(serializedObject, "m_CapsuleCollider");
                    EditorGUILayout.PropertyField(capsuleCollider);
                    if (capsuleCollider.objectReferenceValue == null) {
                        EditorGUILayout.HelpBox("This field is required if no CapsuleColliders exist on the base character object.", MessageType.Error);
                    }
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_LinkedColliders"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_GroundedIdleFrictionMaterial"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_GroundedMovingFrictionMaterial"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_StepFrictionMaterial"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_SlopeFrictionMaterial"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AirFrictionMaterial"));
                EditorGUI.indentLevel--;
            }

            if ((m_RestrictionsFoldout = EditorGUILayout.Foldout(m_RestrictionsFoldout, "Constraint Options", InspectorUtility.BoldFoldout))) {
                var movementRestriction = PropertyFromName(serializedObject, "m_MovementConstraint");
                EditorGUILayout.PropertyField(movementRestriction);
                if ((RigidbodyCharacterController.MovementConstraint)movementRestriction.enumValueIndex == RigidbodyCharacterController.MovementConstraint.RestrictX ||
                    (RigidbodyCharacterController.MovementConstraint)movementRestriction.enumValueIndex == RigidbodyCharacterController.MovementConstraint.RestrictXZ) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MinXPosition"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxXPosition"));
                }
                if ((RigidbodyCharacterController.MovementConstraint)movementRestriction.enumValueIndex == RigidbodyCharacterController.MovementConstraint.RestrictZ ||
                    (RigidbodyCharacterController.MovementConstraint)movementRestriction.enumValueIndex == RigidbodyCharacterController.MovementConstraint.RestrictXZ) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MinZPosition"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxZPosition"));
                }
            }

            if ((m_ItemFoldout = EditorGUILayout.Foldout(m_ItemFoldout, "Item Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                // Explicitly update always aim so the property updates the Animator Monitor.
                var alwaysAim = EditorGUILayout.Toggle("Always Aim", m_Controller.AlwaysAim);
                if (m_Controller.AlwaysAim != alwaysAim) {
                    m_Controller.AlwaysAim = alwaysAim;
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CombatMovementOnAim"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ItemUseRotationThreshold"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ItemForciblyUseDuration"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DualWieldItemForciblyUseDuration"));
                EditorGUI.indentLevel--;
            }

            if ((m_AbilityFoldout = EditorGUILayout.Foldout(m_AbilityFoldout, "Abilities", InspectorUtility.BoldFoldout))) {
                if (m_ReorderableAbilityList == null) {
                    m_Abilities = PropertyFromName(serializedObject, "m_Abilities");
                    m_ReorderableAbilityList = new ReorderableList(serializedObject, m_Abilities, true, true, true, true);
                    m_ReorderableAbilityList.drawElementCallback = OnAbilityListDraw;
                    m_ReorderableAbilityList.drawHeaderCallback = OnAbilityListDrawHeader;
                    m_ReorderableAbilityList.onReorderCallback = OnAbilityListReorder;
                    m_ReorderableAbilityList.onAddCallback = OnAbilityListAdd;
                    m_ReorderableAbilityList.onRemoveCallback = OnAbilityListRemove;
                    m_ReorderableAbilityList.onSelectCallback = OnAbilityListSelect;
                    if (m_Controller.SelectedAbility != -1) {
                        m_ReorderableAbilityList.index = m_Controller.SelectedAbility;
                    }
                }
                m_ReorderableAbilityList.DoLayoutList();
                if (m_ReorderableAbilityList.index != -1) {
                    if (m_ReorderableAbilityList.index < m_Abilities.arraySize) {
                        DrawSelectedAbility(m_Abilities.GetArrayElementAtIndex(m_ReorderableAbilityList.index).objectReferenceValue as Ability);
                    } else {
                        m_ReorderableAbilityList.index = m_Controller.SelectedAbility = -1;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(m_Controller, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(m_Controller);
            }
        }

        /// <summary>
        /// Abilities are unique if only one of that ability type can be added.
        /// </summary>
        /// <param name="abilityType">The type of ability.</param>
        /// <returns>Is the ability unique?</returns>
        private bool IsUniqueAbility(Type abilityType)
        {
            bool isUnique;
            if (m_AbilityIsUniqueMap.TryGetValue(abilityType, out isUnique)) {
                return isUnique;
            }

            // Non-unique abilities will have the IsUniqueAbility method. Search through the base classes to determine if this method exists.
            // If the method does not exist then the ability is unique.
            isUnique = true;
            var type = abilityType;
            while (!type.Equals(typeof(object))) {
                var isUniqueMethod = type.GetMethod("IsUniqueAbility", BindingFlags.Public | BindingFlags.Static);
                if (isUniqueMethod != null) {
                    isUnique = (bool)isUniqueMethod.Invoke(null, null);
                    m_AbilityIsUniqueMap.Add(abilityType, isUnique);
                    return isUnique;
                }
                type = type.BaseType;
            }
            m_AbilityIsUniqueMap.Add(abilityType, isUnique);
            return isUnique;
        }

        /// <summary>
        /// Draws all of the added abilities.
        /// </summary>
        private void OnAbilityListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            var ability = m_Abilities.GetArrayElementAtIndex(index).objectReferenceValue as Ability;
            // Remove the ability if it no longer exists.
            if (ReferenceEquals(ability, null)) {
                m_Abilities.DeleteArrayElementAtIndex(index);
                m_ReorderableAbilityList.index = m_Controller.SelectedAbility = -1;
                m_Abilities.serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(m_Controller);

                // It's not easy removing a null component.
                var components = m_Controller.GetComponents<Component>();
                for (int i = components.Length - 1; i > -1; --i) {
                    if (ReferenceEquals(components[i], null)) {
                        var serializedObject = new SerializedObject(m_Controller.gameObject);
                        var componentProperty = serializedObject.FindProperty("m_Component");
                        componentProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                return;
            }
            if (ability.hideFlags != HideFlags.HideInInspector) {
                ability.hideFlags = HideFlags.HideInInspector;
            }
            var label = SplitCamelCase(ability.GetType().Name);
            if (ability.IsActive) {
                label += " (Active)";
            }
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 34;
            activeRect.width = 20;
            ability.enabled = EditorGUI.Toggle(activeRect, ability.enabled);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(ability, "Inspector");
                InspectorUtility.SetObjectDirty(ability);
            }
        }

        /// <summary>
        /// Draws the header for the ability list.
        /// </summary>
        private void OnAbilityListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Ability");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, "Enabled");
        }

        /// <summary>
        /// The ReorderableList has been reordered. Update the Ability index.
        /// </summary>
        private void OnAbilityListReorder(ReorderableList list)
        {
            for (int i = 0; i < m_Abilities.arraySize; ++i) {
                (m_Abilities.GetArrayElementAtIndex(i).objectReferenceValue as Ability).Index = i;
                InspectorUtility.SetObjectDirty(m_Abilities.GetArrayElementAtIndex(i).objectReferenceValue as Ability);
            }
            InspectorUtility.SetObjectDirty(m_Controller);
        }

        /// <summary>
        /// The ReordableList add button has been pressed. Show the menu listing all of the possible abilities.
        /// </summary>
        private void OnAbilityListAdd(ReorderableList list)
        {
            m_AddMenu = new GenericMenu();
            for (int i = 0; i < m_AbilityTypes.Count; ++i) {
                if (m_Controller.GetComponent(m_AbilityTypes[i]) != null && IsUniqueAbility(m_AbilityTypes[i])) {
                    continue;
                }

                m_AddMenu.AddItem(new GUIContent(SplitCamelCase(m_AbilityTypes[i].Name)), false, AddAbility, m_AbilityTypes[i]);
            }

            m_AddMenu.ShowAsContext();
        }

        /// <summary>
        /// Adds a new ability of the specified type.
        /// </summary>
        private void AddAbility(object obj)
        {
            var ability = Undo.AddComponent(m_Controller.gameObject, obj as Type) as Ability;
            ability.Index = m_Abilities.arraySize;
            ability.hideFlags = HideFlags.HideInInspector;
            m_Abilities.InsertArrayElementAtIndex(m_Abilities.arraySize);
            m_Abilities.GetArrayElementAtIndex(m_Abilities.arraySize - 1).objectReferenceValue = ability;
            m_Abilities.serializedObject.ApplyModifiedProperties();
            m_Controller.SelectedAbility = m_ReorderableAbilityList.index = m_Abilities.arraySize - 1;
            InspectorUtility.SetObjectDirty(m_Controller);
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected ability.
        /// </summary>
        private void OnAbilityListRemove(ReorderableList list)
        {
            var ability = m_Abilities.GetArrayElementAtIndex(list.index).objectReferenceValue as Ability;
            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            m_Abilities.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
            m_Abilities.DeleteArrayElementAtIndex(list.index);
            Undo.DestroyObjectImmediate(ability);
            list.index = list.index - 1;
            if (list.index == -1 && m_Abilities.arraySize > 0) {
                list.index = 0;
            }
            m_Controller.SelectedAbility = list.index;
            InspectorUtility.SetObjectDirty(m_Controller);
        }

        /// <summary>
        /// An ability has been selected from the ability list.
        /// </summary>
        private void OnAbilityListSelect(ReorderableList list)
        {
            m_Controller.SelectedAbility = list.index;
        }

        /// <summary>
        /// Draws all of the fields for the selected ability.
        /// </summary>
        private void DrawSelectedAbility(Ability ability)
        {
            SerializedObject abilitySerializedObject;
            if (!m_SerializedObjectMap.TryGetValue(ability.GetInstanceID(), out abilitySerializedObject) || abilitySerializedObject.targetObject == null) {
                abilitySerializedObject = new SerializedObject(ability);
                m_SerializedObjectMap.Remove(ability.GetInstanceID());
                m_SerializedObjectMap.Add(ability.GetInstanceID(), abilitySerializedObject);
            }
            abilitySerializedObject.Update();
            EditorGUILayout.LabelField(SplitCamelCase(ability.GetType().Name), EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel++;
            var property = abilitySerializedObject.GetIterator();
            property.NextVisible(true);
            do {
                EditorGUILayout.PropertyField(property, true);
            } while (property.NextVisible(false));
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(ability, "Inspector");
                abilitySerializedObject.ApplyModifiedProperties();
                if (ability != null) {
                    InspectorUtility.SetObjectDirty(ability);
                }
            }
        }

        /// <summary>
        /// Toggles the platform dependent multiplayer compiler symbole.
        /// </summary>
        public static void ToggleMultiplayerSymbol()
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (symbols.Contains(s_MultiplayerSymbol + ";")) {
                symbols = symbols.Replace(s_MultiplayerSymbol + ";", "");
            } else if (symbols.Contains(s_MultiplayerSymbol)) {
                symbols = symbols.Replace(s_MultiplayerSymbol, "");
            } else {
                symbols += (";" + s_MultiplayerSymbol);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Places a space before each capital letter in a word.
        /// </summary>
        private string SplitCamelCase(string s)
        {
            if (s.Equals(""))
                return s;
            if (m_CamelCaseSplit.ContainsKey(s)) {
                return m_CamelCaseSplit[s];
            }

            var origString = s;
            s = s_CamelCaseRegex.Replace(s, " ");
            s = s.Replace("_", " ");
            s = (char.ToUpper(s[0]) + s.Substring(1)).Trim();
            m_CamelCaseSplit.Add(origString, s);
            return s;
        }
    }
}