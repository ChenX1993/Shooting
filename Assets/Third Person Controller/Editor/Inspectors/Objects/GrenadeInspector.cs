using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Grenade.
    /// </summary>
    [CustomEditor(typeof(Grenade))]
    public class GrenadeInspector : DestructableInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var grenade = target as Grenade;
            if (grenade == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Lifespan"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(grenade, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(grenade);
            }
        }
    }
}