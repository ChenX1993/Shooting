using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for MagicItem.
    /// </summary>
    [CustomEditor(typeof(MagicItem))]
    public class MagicItemInspector : ItemInspector
    {
        // MagicItem
        [SerializeField] private static bool m_MagicFoldout = true;
        [SerializeField] private static bool m_ImpactFoldout = true;
        [SerializeField] private static bool m_RegenerativeFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var magicItem = target as MagicItem;
            if (magicItem == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if ((m_MagicFoldout = EditorGUILayout.Foldout(m_MagicFoldout, "Magic Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanUseInAir"));
                var useMode = PropertyFromName(serializedObject, "m_CastMode");
                EditorGUILayout.PropertyField(useMode);
                if ((MagicItem.CastMode)useMode.enumValueIndex == MagicItem.CastMode.Continuous) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ContinuousMinUseDuration"));
                }
                var useShape = PropertyFromName(serializedObject, "m_CastShape");
                EditorGUILayout.PropertyField(useShape);
                if ((MagicItem.CastShape)useShape.enumValueIndex == MagicItem.CastShape.Linear) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastDistance"));
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastRate"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastPoint"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastAmount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastRadius"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TargetLayer"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_WaitForEndUseEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanStopBeforeUse"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastParticles"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastSound"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CastSoundDelay"));
                EditorGUI.indentLevel--;
            }

            if ((m_ImpactFoldout = EditorGUILayout.Foldout(m_ImpactFoldout, "Impact Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DamageAmount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_NormalizeDamage"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ImpactForce"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultImpactSound"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultSpark"));
                EditorGUI.indentLevel--;
            }

            if ((m_RegenerativeFoldout = EditorGUILayout.Foldout(m_RegenerativeFoldout, "Regenerative Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RegenerateRate"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RegenerateAmount"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(magicItem, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(magicItem);
            }
        }
    }
}