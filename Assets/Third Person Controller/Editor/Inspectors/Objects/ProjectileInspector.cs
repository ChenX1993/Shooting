using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Projectile.
    /// </summary>
    [CustomEditor(typeof(Projectile))]
    public class ProjectileInspector : DestructableInspector
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var projectile = target as Projectile;
            if (projectile == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_InitialSpeed"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Speed"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Lifespan"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DestroyOnCollision"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(projectile, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(projectile);
            }
        }
    }
}