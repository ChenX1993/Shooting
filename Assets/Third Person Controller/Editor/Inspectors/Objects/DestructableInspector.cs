using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Destructable.
    /// </summary>
    [CustomEditor(typeof(Destructable))]
    public class DestructableInspector : InspectorBase
    {
        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var destructable = target as Destructable;
            if (destructable == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ParentItemType"));
            // Show the damage amount and impact force fields if the explosion field is null. If an object is specified make sure it has a Explosion component added to it.
            var explosion = PropertyFromName(serializedObject, "m_Explosion");
            EditorGUILayout.PropertyField(explosion);
            if (explosion.objectReferenceValue == null) {
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageAmount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ImpactForce"));
            } else {
                if ((explosion.objectReferenceValue as GameObject).GetComponent<Explosion>() == null) {
                    EditorGUILayout.HelpBox("The explosion object must have an Explosion component added to it.", MessageType.Error);
                }
            }
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultDecal"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultDust"));

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(destructable, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(destructable);
            }
        }
    }
}