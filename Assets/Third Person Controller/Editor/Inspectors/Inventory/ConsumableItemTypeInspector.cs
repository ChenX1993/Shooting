using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ConsumableItemType.
    /// </summary>
    [CustomEditor(typeof(ConsumableItemType))]
    public class ConsumableItemTypeInspector : ItemTypeInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Intentionally left blank.
        }
    }
}