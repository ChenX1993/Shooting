using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for CharacterHealth.
    /// </summary>
    [CustomEditor(typeof(CharacterHealth))]
    public class CharacterHealthInspector : HealthInspector
    {
        [SerializeField] private static bool m_FallDamageFoldout = true;
        [SerializeField] private static bool m_DamageFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var characterHealth = target as CharacterHealth;
            if (characterHealth == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();
            
            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if ((m_FallDamageFoldout = EditorGUILayout.Foldout(m_FallDamageFoldout, "Fall Damage Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var useFallDamage = PropertyFromName(serializedObject, "m_UseFallDamage");
                EditorGUILayout.PropertyField(useFallDamage);
                if (useFallDamage.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MinFallDamageHeight"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeathHeight"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MinFallDamage"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxFallDamage"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageCurve"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_DamageFoldout = EditorGUILayout.Foldout(m_DamageFoldout, "Damage Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageSound"), true);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(characterHealth, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(characterHealth);
            }
        }
    }
}