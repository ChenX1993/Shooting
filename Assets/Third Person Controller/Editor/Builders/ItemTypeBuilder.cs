using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// A wizard that will build a new ItemType.
    /// </summary>
    public class ItemTypeBuilder : EditorWindow
    {
        private GUIStyle m_HeaderLabelStyle;

        private enum ItemTypes { Primary, Consumable, Secondary, DualWield };
        private ItemTypes m_Type = ItemTypes.Primary;
        private ConsumableItemType m_UseableConsumableItem;
        private int m_Capacity = 1;

        [MenuItem("Tools/Third Person Controller/Item Type Builder", false, 12)]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<ItemTypeBuilder>(true, "Item Type Builder");
            window.minSize = new Vector2(520, 220);
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
        }

        /// <summary>
        /// Shows the Item Type Builder.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("This builder will guide you through the ItemType creation process. An ItemType is used by the Inventory and is used to map a particular Item to " +
                                       "the Inventory.", m_HeaderLabelStyle);
            GUILayout.Space(5);

            m_Type = (ItemTypes)EditorGUILayout.EnumPopup("Type", m_Type);

            ShowItemTypeGUI();

            EditorGUI.indentLevel++;
            if (m_Type == ItemTypes.Primary) {
                m_UseableConsumableItem = EditorGUILayout.ObjectField("Consumable Item", m_UseableConsumableItem, typeof(ConsumableItemType), false) as ConsumableItemType;
                if (m_UseableConsumableItem != null) {
                    EditorGUI.indentLevel++;
                    m_Capacity = EditorGUILayout.IntField("Capacity", m_Capacity);
                    EditorGUI.indentLevel--;
                }
            } else if (m_Type == ItemTypes.Secondary) {
                m_Capacity = EditorGUILayout.IntField("Capacity", m_Capacity);
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(3);
            if (GUILayout.Button("Build")) {
                BuildItemType();
            }
        }

        /// <summary>
        /// Shows the current items type's header.
        /// </summary>
        private void ShowItemTypeGUI()
        {
            var title = "";
            var description = "";
            switch (m_Type) {
                case ItemTypes.Primary:
                    title = "Primary";
                    description = "A Primary ItemType is any item that can be equipped by the character. ";
                    break;
                case ItemTypes.Consumable:
                    title = "Consumable";
                    description = "A Consumable ItemType is any items that can be used by the PrimaryItemType. These items cannot be equipped or used independently.";
                    break;
                case ItemTypes.Secondary:
                    title = "Secondary";
                    description = "Secondary items are items that are used by the non-dominant hand of the character.";
                    break;
                case ItemTypes.DualWield:
                    title = "Dual Wield";
                    description = "Dual Wield items can be used at the same time as another Primary Item.";
                    break;
            }

            GUILayout.Space(5);
            GUILayout.Label(title, "BoldLabel");
            EditorGUILayout.LabelField(description, m_HeaderLabelStyle);
            GUILayout.Space(5);
        }

        /// <summary>
        /// Builds the ItemType.
        /// </summary>
        private void BuildItemType()
        {
            var path = EditorUtility.SaveFilePanel("Save Character", "Assets", "ItemType.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                ItemType itemType = null;
                switch (m_Type) {
                    case ItemTypes.Primary:
                        var primaryItem = ScriptableObject.CreateInstance<Opsive.ThirdPersonController.Wrappers.PrimaryItemType>();
                        itemType = primaryItem;
                        if (m_UseableConsumableItem != null) {
                            var useableConsumableItem = new PrimaryItemType.UseableConsumableItem(m_UseableConsumableItem, m_Capacity);
                            primaryItem.ConsumableItem = useableConsumableItem;
                        }
                        break;
                    case ItemTypes.Consumable:
                        itemType = ScriptableObject.CreateInstance<Opsive.ThirdPersonController.Wrappers.ConsumableItemType>();
                        break;
                    case ItemTypes.Secondary:
                        var secondaryItem = ScriptableObject.CreateInstance<Opsive.ThirdPersonController.Wrappers.SecondaryItemType>();
                        secondaryItem.Capacity = m_Capacity;
                        itemType = secondaryItem;
                        break;
                    case ItemTypes.DualWield:
                        itemType = ScriptableObject.CreateInstance<Opsive.ThirdPersonController.Wrappers.DualWieldItemType>();
                        break;
                }

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
                Random.seed = System.Environment.TickCount;
#else
                Random.InitState(System.Environment.TickCount);
#endif
                itemType.ID = Random.Range(0, int.MaxValue);

                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(itemType, path);
                AssetDatabase.ImportAsset(path);
                Selection.activeObject = itemType;
            }
        }
    }
}