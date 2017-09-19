using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Opsive.ThirdPersonController
{
    /// <summary>
    /// Builds the Third Person Controller character. This component allows the character to be built at runtime.
    /// </summary>
    public class CharacterBuilder : MonoBehaviour
    {
        // Internal variables
        private static GameObject m_Character;
        private static bool m_AIAgent;
        private static bool m_IsNetworked;
        private static RigidbodyCharacterController.MovementType m_MovementType = RigidbodyCharacterController.MovementType.Combat;
        private static RuntimeAnimatorController m_AnimatorController;
        private static PhysicMaterial m_MaxFrictionMaterial;
        private static PhysicMaterial m_FrictionlessMaterial;

        public static void BuildHumanoidCharacter(GameObject character, bool aiAgent, bool isNetworked, RigidbodyCharacterController.MovementType movementType,
                                            RuntimeAnimatorController animatorController, PhysicMaterial maxFrictionMaterial, PhysicMaterial frictionlessMaterial)
        {
            var animator = character.GetComponent<Animator>();
            var hands = new Transform[] { animator.GetBoneTransform(HumanBodyBones.LeftHand), animator.GetBoneTransform(HumanBodyBones.RightHand) };
            var feet = new Transform[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) };
            BuildCharacter(character, aiAgent, isNetworked, movementType, animatorController, maxFrictionMaterial, frictionlessMaterial, hands, feet);
        }

        /// <summary>
        /// Builds the Third Person Controller character.
        /// </summary>
        public static void BuildCharacter(GameObject character, bool aiAgent, bool isNetworked, RigidbodyCharacterController.MovementType movementType, 
                                            RuntimeAnimatorController animatorController, PhysicMaterial maxFrictionMaterial, PhysicMaterial frictionlessMaterial,
                                            Transform[] itemTransforms, Transform[] feet)
        {
            // Set the internal variables.
            m_Character = character;
            m_AIAgent = aiAgent;
            m_IsNetworked = isNetworked;
            m_MovementType = movementType;
            m_AnimatorController = animatorController;
            m_MaxFrictionMaterial = maxFrictionMaterial;
            m_FrictionlessMaterial = frictionlessMaterial;

            // Build the character.
            BuildStandardComponents(feet);
            for (int i = 0; i < itemTransforms.Length; ++i) {
                BuildItemHands(itemTransforms[i]);
            }
            if (m_IsNetworked) {
                BuildNetwork();
            }
        }

        /// <summary>
        /// Adds the standard components to the character. These components do not have any custom settings associated with them.
        /// </summary>
        /// <param name="character"></param>
        private static void BuildStandardComponents(Transform[] feet)
        {
            if (m_Character.GetComponent<Animator>() == null) {
                m_Character.AddComponent<Animator>();
            }
            var animator = m_Character.GetComponent<Animator>();
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.runtimeAnimatorController = m_AnimatorController;
            if (animator.avatar == null) {
                Debug.LogWarning("Error: The Animator Avatar on " + m_Character + " is not assigned. Please assign an avatar within the inspector.");
            }
            CapsuleCollider capsuleCollider;
            if ((capsuleCollider = m_Character.GetComponent<CapsuleCollider>()) == null) {
                capsuleCollider = m_Character.AddComponent<CapsuleCollider>();
            }
            capsuleCollider.center = new Vector3(0, 0.9f, 0);
            capsuleCollider.radius = 0.3f;
            capsuleCollider.height = 1.8f;
            Rigidbody rigidbody;
            if ((rigidbody = m_Character.GetComponent<Rigidbody>()) == null) {
                rigidbody = m_Character.AddComponent<Rigidbody>();
            }
            rigidbody.angularDrag = 999;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            if (m_AIAgent) {
                m_Character.layer = LayerManager.Default;
            } else {
                // An human-controller character needs to be able to handle input.
                m_Character.AddComponent<Opsive.ThirdPersonController.Input.Wrappers.UnityInput>();
                m_Character.tag = "Player";
                m_Character.layer = LayerManager.Player;
            }

            if (m_Character.GetComponent<Opsive.ThirdPersonController.AnimatorMonitor>() == null) {
                m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.AnimatorMonitor>();
            }
            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.ControllerHandler>();
            RigidbodyCharacterController controller;
            // CharacterHandler requires a RigidbodyCharacterController so the component may already be added.
            if ((controller = m_Character.GetComponent<Opsive.ThirdPersonController.Wrappers.RigidbodyCharacterController>()) == null) {
                controller = m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.RigidbodyCharacterController>();
            } 
            controller.Movement = m_MovementType;
            controller.GroundedIdleFrictionMaterial = m_MaxFrictionMaterial;
            controller.GroundedMovingFrictionMaterial = controller.StepFrictionMaterial = controller.SlopeFrictionMaterial = controller.AirFrictionMaterial = m_FrictionlessMaterial;

            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.Inventory>();

            // Add a trigger and audio source to the feet for footsteps.
            var feetGameObject = new List<GameObject>();
            for (int i = 0; i < feet.Length; ++i) {
                feetGameObject.Add(feet[i].gameObject);
                feetGameObject[i].AddComponent<AudioSource>();
                feetGameObject[i].AddComponent<Opsive.ThirdPersonController.Wrappers.CharacterFootTrigger>();
                var footTrigger = feetGameObject[i].AddComponent<SphereCollider>();
                footTrigger.isTrigger = true;
                footTrigger.radius = 0.18f;
                Rigidbody footRigidbody;
                if ((footRigidbody = feetGameObject[i].GetComponent<Rigidbody>()) == null) {
                    footRigidbody = feetGameObject[i].AddComponent<Rigidbody>();
                }
                footRigidbody.isKinematic = true;
                footRigidbody.useGravity = false;
            }
            if (feetGameObject.Count > 0) {
                var footsteps = m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.CharacterFootsteps>();
                footsteps.Feet = feetGameObject.ToArray();
            }

            var childTransforms = m_Character.GetComponentsInChildren<Transform>();
            for (int i = 0; i < childTransforms.Length; ++i) {
                if (childTransforms[i].gameObject.Equals(m_Character)) {
                    continue;
                }
                childTransforms[i].gameObject.layer = LayerManager.IgnoreRaycast;
            }
            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.ItemHandler>();
            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.InventoryHandler>();
            // The CharacterIK component only works with humanoids.
            if (animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.CharacterIK>();
            }

            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.CharacterHealth>();
            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.CharacterRespawner>();
            if (m_MovementType == RigidbodyCharacterController.MovementType.PointClick) {
                m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.PointClickControllerHandler>();
            }
        }

        /// <summary>
        /// Adds the GameObject that items will be placed under.
        /// </summary>
        /// <param name="handTransform">The parent transform.</param>
        private static void BuildItemHands(Transform handTransform)
        {
            var addItems = true;
#if MORPH_CHARACTER_SYSTEM_PRESENT
            // The Morph 3D character already contains a mount GameObject.
            var attachmentPointType = System.Type.GetType("MORPH3D.COSTUMING.CIattachmentPoint, M3D_DLL");
            if (attachmentPointType != null) {
                var attachmentPoint = handTransform.GetComponentInChildren(attachmentPointType);
                if (attachmentPoint != null) {
                    attachmentPoint.gameObject.AddComponent<Opsive.ThirdPersonController.Wrappers.ItemPlacement>();
                    addItems = false;
                }
            }
#endif
            if (addItems) {
                var items = new GameObject("Items");
                items.AddComponent<Opsive.ThirdPersonController.Wrappers.ItemPlacement>();
                items.transform.parent = handTransform;
                items.transform.localPosition = Vector3.zero;
                items.transform.localRotation = Quaternion.identity;
            }
        }
        
        /// <summary>
        /// Adds the network components to the character.
        /// </summary>
        private static void BuildNetwork()
        {
            m_Character.AddComponent<Opsive.ThirdPersonController.Wrappers.NetworkMonitor>();
            var networkAnimator = m_Character.AddComponent<NetworkAnimator>();
            networkAnimator.animator = m_Character.GetComponent<Animator>();
            var networkTransform = m_Character.AddComponent<NetworkTransform>();
            networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
        }
    }
}