using UnityEngine;

namespace Opsive.ThirdPersonController
{
    public class ItemBuilder : MonoBehaviour
    {
        public enum ItemTypes { Shootable, Melee, Throwable, Magic, Static, Shield };
        public enum HandAssignment { Left, Right }

        private static ItemType m_ItemType;
        private static GameObject m_Base;
        private static string m_ItemName;
        private static ItemTypes m_Type = ItemTypes.Shootable;
        private static HandAssignment m_HandAssignment = HandAssignment.Right;

        /// <summary>
        /// Builds a new Third Person Controller Item.
        /// </summary>
        public static void BuildItem(GameObject item, ItemType itemType, string itemName, ItemTypes baseObjectType, HandAssignment handAssignment)
        {
            m_Base = item;
            m_ItemType = itemType;
            m_ItemName = itemName;
            m_Type = baseObjectType;
            m_HandAssignment = handAssignment;

            BuildItem();
        }

        /// <summary>
        /// Builds a new Item.
        /// </summary>
        private static void BuildItem()
        {
            m_Base.layer = LayerManager.IgnoreRaycast;
            switch (m_Type) {
                case ItemTypes.Shootable:
                    BuildShootableItem();
                    break;
                case ItemTypes.Melee:
                    BuildMeleeWeapon();
                    break;
                case ItemTypes.Throwable:
                    BuildThrowableItem();
                    break;
                case ItemTypes.Magic:
                    BuildMagicItem();
                    break;
                case ItemTypes.Static:
                    BuildStaticItem();
                    break;
                case ItemTypes.Shield:
                    BuildShield();
                    break;
            }
            var item = m_Base.GetComponent<Item>();
            item.ItemName = m_ItemName;

            // Ensure the correct animation state is defined.
            var hasRenderer = m_Base.GetComponent<Renderer>() != null;
            if (!hasRenderer || m_Type == ItemTypes.Magic) {
                item.DefaultStates.Idle.Groups[0].States[0].Layer = 0;
                item.DefaultStates.Movement.Groups[0].States[0].Layer = 0;
                item.DefaultStates.Idle.Groups[0].States[0].ItemNamePrefix = false;
                item.DefaultStates.Movement.Groups[0].States[0].ItemNamePrefix = false;
            } else {
                if (m_Type == ItemTypes.Shootable) {
                    item.DefaultStates.Idle.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.LeftArm | AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.DefaultStates.Movement.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.LeftArm | AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.AimStates.Idle.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm | 
                                                                    AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.AimStates.Movement.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm | 
                                                                        AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.EquipStates.Idle.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm |
                                                                        AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.EquipStates.Movement.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm |
                                                                             AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.UnequipStates.Idle.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm |
                                                                        AnimatorItemStateData.AnimatorLayer.RightArm;
                    item.UnequipStates.Movement.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm |
                                                                             AnimatorItemStateData.AnimatorLayer.RightArm;
                    (item as ShootableWeapon).ReloadStates.Idle.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm | 
                                                                                            AnimatorItemStateData.AnimatorLayer.RightArm;
                    (item as ShootableWeapon).ReloadStates.Movement.Groups[0].States[0].Layer = AnimatorItemStateData.AnimatorLayer.UpperBody | AnimatorItemStateData.AnimatorLayer.LeftArm | 
                                                                                                AnimatorItemStateData.AnimatorLayer.RightArm;
                } else if (m_Type == ItemTypes.Melee || m_Type == ItemTypes.Throwable) {
                    var handLayer = m_HandAssignment == HandAssignment.Right ? AnimatorItemStateData.AnimatorLayer.RightHand : AnimatorItemStateData.AnimatorLayer.LeftHand;
                    item.DefaultStates.Idle.Groups[0].States[0].Layer = handLayer;
                    item.DefaultStates.Movement.Groups[0].States[0].Layer = handLayer;
                } else if (m_Type == ItemTypes.Static) {
                    var handLayer = m_HandAssignment == HandAssignment.Right ? AnimatorItemStateData.AnimatorLayer.RightArm : AnimatorItemStateData.AnimatorLayer.LeftArm;
                    item.DefaultStates.Idle.Groups[0].States[0].Layer = handLayer;
                    item.DefaultStates.Movement.Groups[0].States[0].Layer = handLayer;
                }
            }
            item.AimStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
            item.AimStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
            if (m_Type == ItemTypes.Shootable) {
                (item as ShootableWeapon).UseStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as ShootableWeapon).UseStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as ShootableWeapon).ReloadStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as ShootableWeapon).ReloadStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
            } else if (m_Type == ItemTypes.Melee) {
                (item as MeleeWeapon).UseStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as MeleeWeapon).UseStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
            } else if (m_Type == ItemTypes.Throwable) {
                (item as ThrowableItem).UseStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as ThrowableItem).UseStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
            } else if (m_Type == ItemTypes.Magic) {
                (item as MagicItem).UseStates.Idle.Groups[0].States[0].IgnoreLowerPriority = true;
                (item as MagicItem).UseStates.Movement.Groups[0].States[0].IgnoreLowerPriority = true;
            }

            var audioSource = m_Base.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        /// <summary>
        /// Adds the ShootableWeapon component to the Item.
        /// </summary>
        private static void BuildShootableItem()
        {
            m_Base.AddComponent<BoxCollider>();
            var rigidbody = m_Base.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            var itemTransform = m_Base.transform;
            var shootableWeapon = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.ShootableWeapon>();

            shootableWeapon.ItemType = m_ItemType;
            var firePoint = CreateChildObject(itemTransform, "Fire Point", Vector3.zero);
            firePoint.transform.localEulerAngles = new Vector3(0, 180, 0);
            shootableWeapon.FirePoint = firePoint;
        }

        /// <summary>
        /// Create a new GameObject relative to the parent with the specified name and offset.
        /// </summary>
        private static Transform CreateChildObject(Transform parent, string name, Vector3 offset)
        {
            var child = new GameObject(name).transform;
            child.parent = parent;
            child.localPosition = offset;
            return child;
        }

        /// <summary>
        /// Adds the MeleeWeapon component to the Item.
        /// </summary>
        /// <param name="item"></param>
        private static void BuildMeleeWeapon()
        {
            m_Base.AddComponent<BoxCollider>();

            if (m_Base.GetComponent<Renderer>() != null) {
                var rigidbody = m_Base.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }

            var meleeWeapon = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.MeleeWeapon>();
            meleeWeapon.ItemType = m_ItemType;
        }

        /// <summary>
        /// Adds the ThrowableItem component to the Item.
        /// </summary>
        /// <param name="item"></param>
        private static void BuildThrowableItem()
        {
            var throwableItem = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.ThrowableItem>();
            throwableItem.ItemType = m_ItemType;
        }

        /// <summary>
        /// Adds the MagicItem component to the Item.
        /// </summary>
        private static void BuildMagicItem()
        {
            var magicItem = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.MagicItem>();
            magicItem.ItemType = m_ItemType;
        }

        /// <summary>
        /// Adds the StaticItem component to the Item.
        /// </summary>
        private static void BuildStaticItem()
        {
            var staticItem = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.StaticItem>();
            staticItem.ItemType = m_ItemType;
        }

        /// <summary>
        /// Adds the Shield component to the Item.
        /// </summary>
        private static void BuildShield()
        {
            var staticItem = m_Base.AddComponent<Opsive.ThirdPersonController.Wrappers.Shield>();
            staticItem.ItemType = m_ItemType;
        }
    }
}