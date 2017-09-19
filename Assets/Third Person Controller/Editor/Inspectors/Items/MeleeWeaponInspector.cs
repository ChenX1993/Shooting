using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for MeleeWeapon.
    /// </summary>
    [CustomEditor(typeof(MeleeWeapon))]
    public class MeleeWeaponInspector : ItemInspector
    {
        // MeleeWeapon
        [SerializeField] private static bool m_AttackFoldout = true;
        [SerializeField] private static bool m_ImpactFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var meleeWeapon = target as MeleeWeapon;
            if (meleeWeapon == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // A collider must exist.
            var collider = meleeWeapon.GetComponent<Collider>();
            if (collider == null) {
                EditorGUILayout.HelpBox("A collider must exist on the weapon.", MessageType.Error);
            }
            // As does a Rigidbody.
            var rigidbody = meleeWeapon.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                EditorGUILayout.HelpBox("A Rigidbody must exist on the weapon.", MessageType.Error);
            }

            if ((m_AttackFoldout = EditorGUILayout.Foldout(m_AttackFoldout, "Attack Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanUseInAir"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AttackRate"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AttackLayer"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AttackHitboxes"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanInterruptAttack"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_SingleHitAttack"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_WaitForEndUseEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AttackSound"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AttackSoundDelay"));
                EditorGUI.indentLevel--;
            }

            if ((m_ImpactFoldout = EditorGUILayout.Foldout(m_ImpactFoldout, "Impact Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageAmount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ImpactForce"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultDust"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultImpactSound"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(meleeWeapon, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(meleeWeapon);
            }
        }
    }
}