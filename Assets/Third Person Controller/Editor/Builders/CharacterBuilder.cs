using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
using UnityEngine.Networking;
#endif

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a wizard that will build a new Third Person Controller character.
    /// </summary>
    public class CharacterBuilder : EditorWindow
    {
        // Window variables
        private Vector2 m_ScrollPosition;
        private GUIStyle m_HeaderLabelStyle;

        // Character variables
        private enum ModelType { Humanoid, Generic }
        private SerializedObject m_SerializedObject;
        private ModelType m_ModelType;
        private GameObject m_Character;
        private bool m_AIAgent;
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
        private bool m_IsNetworked;
#endif
        private bool m_AddStandardAbilities = true;
        private bool m_AddRagdoll = true;
        private bool m_IsHumanoid;
#if DEATHMATCH_AI_KIT_PRESENT
        private bool m_DeathmatchAgent;
#endif
        private RigidbodyCharacterController.MovementType m_MovementType = RigidbodyCharacterController.MovementType.Combat;
        private RuntimeAnimatorController m_AnimatorController;
        [SerializeField] private Transform[] m_ItemTransforms = new Transform[0];
        [SerializeField] private Transform[] m_FootTransforms = new Transform[0];
        private bool m_CharacterUpdate = false;

        // Exposed properties
        public GameObject Character { set { m_Character = value; m_CharacterUpdate = true; } }
        public bool AIAgent { set { m_AIAgent = value; } }

        [MenuItem("Tools/Third Person Controller/Character Builder", false, 11)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<CharacterBuilder>(true, "Character Builder");
            window.minSize = window.maxSize = new Vector2(520, 355);
        }

        /// <summary>
        /// Initializes the GUIStyle used by the header.
        /// </summary>
        private void OnEnable()
        {
            if (m_HeaderLabelStyle == null) {
                m_HeaderLabelStyle = new GUIStyle(EditorStyles.label);
                m_HeaderLabelStyle.wordWrap = true;
            }
            if (m_SerializedObject == null) {
                m_SerializedObject = new SerializedObject(this);
            }
            if (m_AnimatorController == null && m_ModelType == ModelType.Humanoid) {
                UpdateAnimatorController();
            }
        }

        /// <summary>
        /// Shows the Character Builder.
        /// </summary>
        private void OnGUI()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);

            ShowHeaderGUI();

            var canBuild = ShowGUI();

            GUILayout.EndScrollView();

            GUILayout.Space(3);
            GUI.enabled = canBuild;
            if (GUILayout.Button("Build")) {
                BuildCharacter();
            }
        }

        /// <summary>
        /// Shows the current section's header.
        /// </summary>
        private void ShowHeaderGUI()
        {
            var description = "This wizard will assist in the creation of a new character.";
            EditorGUILayout.LabelField(description, m_HeaderLabelStyle);
            GUILayout.Space(5);
        }

        /// <summary>
        /// Shows the Character Builder options.
        /// </summary>
        private bool ShowGUI()
        {
            var modelType = (ModelType)EditorGUILayout.EnumPopup("Model Type", m_ModelType);
            if (modelType != m_ModelType) {
                m_ModelType = modelType;
                UpdateAnimatorController();
            }
            var canContinue = true;
            m_Character = EditorGUILayout.ObjectField("Character", m_Character, typeof(GameObject), true) as GameObject;
            if (m_Character == null) {
                EditorGUILayout.HelpBox("Select the GameObject which will be used as the character. This object will have the majority of the components added to it.",
                                    MessageType.Error);
                canContinue = false;
            } else if (PrefabUtility.GetPrefabType(m_Character) == PrefabType.Prefab) {
                EditorGUILayout.HelpBox("Please drag your character into the scene. The Character Builder cannot add components to prefabs.",
                                    MessageType.Error);
                canContinue = false;
            }

            if (m_ModelType == ModelType.Humanoid) {
                // Ensure the character is a humanoid.
                if (GUI.changed || m_CharacterUpdate) {
                    if (m_Character != null) {
                        var character = m_Character;
                        var spawnedCharacter = false;
                        // The character has to be spawned in order to be able to detect if it is a Humanoid.
                        if (AssetDatabase.GetAssetPath(m_Character).Length > 0) {
                            character = GameObject.Instantiate(character) as GameObject;
                            spawnedCharacter = true;
                        }
                        var animator = character.GetComponent<Animator>();
                        var hasAnimator = animator != null;
                        if (!hasAnimator) {
                            animator = character.AddComponent<Animator>();
                        }
                        // A human will have a head.
                        m_IsHumanoid = animator.GetBoneTransform(HumanBodyBones.Head) != null;
                        // Clean up.
                        if (!hasAnimator) {
                            DestroyImmediate(animator, true);
                        }
                        if (spawnedCharacter) {
                            DestroyImmediate(character, true);
                        }
                    }
                    m_CharacterUpdate = false;
                }

                if (m_Character != null && !m_IsHumanoid) {
                    EditorGUILayout.HelpBox(m_Character.name + " is not a humanoid. Please select the Humanoid Animation Type within the Rig Import Settings. " +
                                                               "In addition, ensure all of the bones are configured correctly.", MessageType.Error);
                    canContinue = false;
                }
            }

            var movementType = (RigidbodyCharacterController.MovementType)EditorGUILayout.EnumPopup("Movement Type", m_MovementType);
            if (m_MovementType != movementType) {
                m_MovementType = movementType;
                UpdateAnimatorController();
            }
            m_AIAgent = EditorGUILayout.Toggle("Is AI Agent", m_AIAgent);
            EditorGUILayout.HelpBox("Is this character going to be used for AI? Some components (such as PlayerInput) do not need to be added if the character is an AI agent.", 
                                MessageType.Info);
#if DEATHMATCH_AI_KIT_PRESENT
            if (m_AIAgent) {
                m_DeathmatchAgent = EditorGUILayout.Toggle("Is Deathmatch Agent", m_DeathmatchAgent);
                EditorGUILayout.HelpBox("Is this character going to be used by the Deathmatch AI Kit?", MessageType.Info);
            }
#endif
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
            m_IsNetworked = EditorGUILayout.Toggle("Is Networked", m_IsNetworked);
            EditorGUILayout.HelpBox("Is this character going to be used on the network with Unity 5's multiplayer implementation?", MessageType.Info);
#endif
            if (m_ModelType == ModelType.Humanoid) {
                m_AddStandardAbilities = EditorGUILayout.Toggle("Add Standard Abilities", m_AddStandardAbilities);
                m_AddRagdoll = EditorGUILayout.Toggle("Add Ragdoll", m_AddRagdoll);
                if (m_AddRagdoll) {
                    EditorGUILayout.HelpBox("Unity's Ragdoll Builder will open when this wizard is complete.", MessageType.Info);
                }
            }
            if (m_ModelType == ModelType.Generic || m_AnimatorController == null) {
                if (m_ModelType == ModelType.Humanoid) {
                    EditorGUILayout.HelpBox("Unable to find the built-in Animator Controller. Please select the controlled that your character should use.", MessageType.Warning);
                }
                m_AnimatorController = EditorGUILayout.ObjectField("Animator Controller", m_AnimatorController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
                canContinue = canContinue && m_ModelType == ModelType.Generic;
            }
            if (m_ModelType == ModelType.Generic) {
                m_SerializedObject.Update();
                EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("m_ItemTransforms"), true);
                EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("m_FootTransforms"), true);
                m_SerializedObject.ApplyModifiedProperties();
            }

            return canContinue;
        }

        /// <summary>
        /// Updates the animator controller based on the model and movement type.
        /// </summary>
        private void UpdateAnimatorController()
        {
            if (m_ModelType == ModelType.Humanoid) {
                var baseDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))).Replace("Editor/Builders", "");
                if (m_MovementType != RigidbodyCharacterController.MovementType.Adventure) {
                    m_AnimatorController = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demos/Third Person Shooter/Animator/Shooter.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                } else {
                    m_AnimatorController = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demos/Adventure/Animator/Adventure.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                }
            } else {
                m_AnimatorController = null;
            }
        }

        /// <summary>
        /// Builds the character.
        /// </summary>
        private void BuildCharacter()
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m_Character))) {
                var name = m_Character.name;
                m_Character = GameObject.Instantiate(m_Character) as GameObject;
                m_Character.name = name;
            }
            // Call a runtime component to build the character so the character can be built at runtime.
            var isNetworked = false;
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
            isNetworked = m_IsNetworked;
#endif
            var baseDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))).Replace("Editor/Builders", "");
            var maxFrictionMaterial = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demos/Shared/Physic Materials/MaxFriction.physicMaterial", typeof(PhysicMaterial)) as PhysicMaterial;
            var frictionlessMaterial = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demos/Shared/Physic Materials/Frictionless.physicMaterial", typeof(PhysicMaterial)) as PhysicMaterial;
            if (m_ModelType == ModelType.Humanoid) {
                ThirdPersonController.CharacterBuilder.BuildHumanoidCharacter(m_Character, m_AIAgent, isNetworked, m_MovementType, m_AnimatorController, maxFrictionMaterial, frictionlessMaterial);
            } else {
                ThirdPersonController.CharacterBuilder.BuildCharacter(m_Character, m_AIAgent, isNetworked, m_MovementType, m_AnimatorController, maxFrictionMaterial, frictionlessMaterial, m_ItemTransforms, m_FootTransforms);
            }
            if (isNetworked) {
#if !ENABLE_MULTIPLAYER
                // The character is networked so enable the multiplayer symbol.
                RigidbodyCharacterControllerInspector.ToggleMultiplayerSymbol();
#endif
            } else {
#if ENABLE_MULTIPLAYER
                // The character isn't networked so disable the multiplayer symbol.
                RigidbodyCharacterControllerInspector.ToggleMultiplayerSymbol();
#endif
            }
            Selection.activeGameObject = m_Character;
            if (m_ModelType == ModelType.Humanoid) {
                // Add the Fall and Jump abilities.
                if (m_AddStandardAbilities) {
                    var controller = m_Character.GetComponent<RigidbodyCharacterController>();
                    AddAbility(controller, typeof(Opsive.ThirdPersonController.Wrappers.Abilities.Fall), "", Abilities.Ability.AbilityStartType.Automatic, Abilities.Ability.AbilityStopType.Manual);
                    AddAbility(controller, typeof(Opsive.ThirdPersonController.Wrappers.Abilities.Jump), "Jump", Abilities.Ability.AbilityStartType.ButtonDown, Abilities.Ability.AbilityStopType.Automatic);
                    AddAbility(controller, typeof(Opsive.ThirdPersonController.Wrappers.Abilities.SpeedChange), "Change Speeds", Abilities.Ability.AbilityStartType.ButtonDown, Abilities.Ability.AbilityStopType.ButtonUp);
                }

                // Open up the ragdoll builder. This class is internal to the Unity editor so reflection must be used to access it.
                if (m_AddRagdoll) {
                    AddRagdoll();
                }
            }

#if DEATHMATCH_AI_KIT_PRESENT
            // Open the Agent Builder.
            if (m_AIAgent && m_DeathmatchAgent) {
                DeathmatchAIKit.Editor.AgentBuilder.ShowWindow();
                var windows = Resources.FindObjectsOfTypeAll(typeof(DeathmatchAIKit.Editor.AgentBuilder));
                if (windows != null && windows.Length > 0) {
                    var agentBuilder = windows[0] as DeathmatchAIKit.Editor.AgentBuilder;
                    agentBuilder.Agent = m_Character;
                }
            }
#endif
            InspectorUtility.SetObjectDirty(m_Character);
            Close();
        }

        /// <summary>
        /// Adds the ability to the RigidbodyCharacterController.
        /// </summary>
        /// <param name="controller">A reference to the RigidbodyCharacterController.</param>
        /// <param name="type">The type of ability to add.</param>
        /// <param name="inputName">The ability input name. Can be empty.</param>
        /// <param name="startType">The ability StartType.</param>
        /// <param name="stopType">The ability StopType.</param>
        private void AddAbility(RigidbodyCharacterController controller, Type type, string inputName, Abilities.Ability.AbilityStartType startType, Abilities.Ability.AbilityStopType stopType)
        {
            var ability = controller.gameObject.AddComponent(type) as Abilities.Ability;
            // The RigidbodyCharacterController will show the ability inspector.
            ability.hideFlags = HideFlags.HideInInspector;

            // Set the base class values.
            ability.StartType = startType;
            ability.StopType = stopType;
            ability.InputName = inputName;

            // Add the ability to the RigidbodyCharacterController.
            var abilities = controller.Abilities;
            ability.Index = abilities.Length;
            Array.Resize(ref abilities, abilities.Length + 1);
            abilities[abilities.Length - 1] = ability;
            controller.Abilities = abilities;
        }

        /// <summary>
        /// Opens Unity's Ragdoll Builder and populates as many fields as it can.
        /// </summary>
        private void AddRagdoll()
        {
            var controller = m_Character.GetComponent<RigidbodyCharacterController>();
            AddAbility(controller, typeof(Opsive.ThirdPersonController.Wrappers.Abilities.Die), string.Empty, Abilities.Ability.AbilityStartType.Manual, Abilities.Ability.AbilityStopType.Manual);

            var ragdollBuilderType = Type.GetType("UnityEditor.RagdollBuilder, UnityEditor");
            var windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            // Open the Ragdoll Builder if it isn't already opened.
            if (windows == null || windows.Length == 0) {
                EditorApplication.ExecuteMenuItem("GameObject/3D Object/Ragdoll...");
                windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            }

            if (windows != null && windows.Length > 0) {
                var ragdollWindow = windows[0] as ScriptableWizard;
                var animator = m_Character.GetComponent<Animator>();
#if UNITY_4_6 || UNITY_4_7
                SetFieldValue(ragdollWindow, "root", animator.GetBoneTransform(HumanBodyBones.Hips));
#else
                SetFieldValue(ragdollWindow, "pelvis", animator.GetBoneTransform(HumanBodyBones.Hips));
#endif
                SetFieldValue(ragdollWindow, "leftHips", animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
                SetFieldValue(ragdollWindow, "leftKnee", animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
                SetFieldValue(ragdollWindow, "leftFoot", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                SetFieldValue(ragdollWindow, "rightHips", animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
                SetFieldValue(ragdollWindow, "rightKnee", animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
                SetFieldValue(ragdollWindow, "rightFoot", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                SetFieldValue(ragdollWindow, "leftArm", animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
                SetFieldValue(ragdollWindow, "leftElbow", animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
                SetFieldValue(ragdollWindow, "rightArm", animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
                SetFieldValue(ragdollWindow, "rightElbow", animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                SetFieldValue(ragdollWindow, "middleSpine", animator.GetBoneTransform(HumanBodyBones.Spine));
                SetFieldValue(ragdollWindow, "head", animator.GetBoneTransform(HumanBodyBones.Head));

                var method = ragdollWindow.GetType().GetMethod("CheckConsistency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) {
                    ragdollWindow.errorString = (string)method.Invoke(ragdollWindow, null);
                    ragdollWindow.isValid = string.IsNullOrEmpty(ragdollWindow.errorString);
                }
            }
        }

        /// <summary>
        /// Use reflection to set the value of the field.
        /// </summary>
        private void SetFieldValue(ScriptableWizard obj, string name, object value)
        {
            if (value == null) {
                return;
            }

            var field = obj.GetType().GetField(name);
            if (field != null) {
                field.SetValue(obj, value);
            }
        }
    }
}