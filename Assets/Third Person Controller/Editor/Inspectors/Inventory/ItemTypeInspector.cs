using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ItemType.
    /// </summary>
    [CustomEditor(typeof(ItemType))]
    public class ItemTypeInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (target == null || target.Equals(null) || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show the ID field.
            serializedObject.Update();

            // The ID field cannot be edited and is shown for information purposes only. If the ID is -1 then assign a new random id.
            GUI.enabled = false;
            var id = PropertyFromName(serializedObject, "m_ID");
            if (id.intValue == -1) {
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
                Random.seed = System.Environment.TickCount;
#else
                Random.InitState(System.Environment.TickCount);
#endif
                (target as ItemType).ID = Random.Range(0, int.MaxValue);
                InspectorUtility.SetObjectDirty(target);
            }
            EditorGUILayout.PropertyField(id);
            GUI.enabled = true;
        }
    }
}