using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for Health.
    /// </summary>
    [CustomEditor(typeof(Health))]
    public class HealthInspector : InspectorBase
    {
        private static bool m_ShieldFoldout = true;
        private static bool m_DeathFoldout = true;
        private static bool m_DamageMultiplierFoldout = true;

        private ReorderableList m_DamageMultiplierList;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var health = target as Health;
            if (health == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (Application.isPlaying) {
                GUI.enabled = false;
                EditorGUILayout.FloatField("Current Health", health.CurrentHealth);
                EditorGUILayout.FloatField("Current Shield", health.CurrentShield);
                GUI.enabled = true;
            }

            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Invincible"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxHealth"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TimeInvincibleAfterSpawn"));
            if ((m_ShieldFoldout = EditorGUILayout.Foldout(m_ShieldFoldout, "Shield Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_MaxShield"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShieldRegenerativeInitialWait"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShieldRegenerativeAmount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShieldRegenerativeWait"));
                EditorGUI.indentLevel--;
            }

            if ((m_DamageMultiplierFoldout = EditorGUILayout.Foldout(m_DamageMultiplierFoldout, "Damage Multiplier Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (m_DamageMultiplierList == null) {
                    var damageMultiplierProperty = PropertyFromName(serializedObject, "m_DamageMultipliers");
                    m_DamageMultiplierList = new ReorderableList(serializedObject, damageMultiplierProperty, true, true, true, true);
                    m_DamageMultiplierList.drawHeaderCallback = OnDamageMultiplierHeaderDraw;
                    m_DamageMultiplierList.drawElementCallback = OnDamageMultiplierElementDraw;
                }
                // Indent the list so it lines up with the rest of the content.
                var rect = GUILayoutUtility.GetRect(0, m_DamageMultiplierList.GetHeight());
                rect.x += EditorGUI.indentLevel * 15;
                rect.xMax -= EditorGUI.indentLevel * 15;
                m_DamageMultiplierList.DoList(rect);

                EditorGUI.indentLevel--;
            }

            if ((m_DeathFoldout = EditorGUILayout.Foldout(m_DeathFoldout, "Death Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_SpawnedObjectsOnDeath"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DestroyedObjectsOnDeath"), true);
                var deactivateOnDeath = PropertyFromName(serializedObject, "m_DeactivateOnDeath");
                EditorGUILayout.PropertyField(deactivateOnDeath);
                if (deactivateOnDeath.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeactivateOnDeathDelay"));
                }
                var deathLayerProperty = PropertyFromName(serializedObject, "m_DeathLayer");
                deathLayerProperty.intValue = EditorGUILayout.LayerField("Death Layer", deathLayerProperty.intValue);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(health, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(health);
            }
        }

        /// <summary>
        /// Draws the DamageMultiplier ReordableList header.
        /// </summary>
        private void OnDamageMultiplierHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight), "GameObject");
            EditorGUI.LabelField(new Rect(rect.x + (rect.width - 90), rect.y, 90, EditorGUIUtility.singleLineHeight), "Multiplier");
        }

        /// <summary>
        /// Draws the DamageMultiplier ReordableList element.
        /// </summary>
        private void OnDamageMultiplierElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            var damageMultiplier = m_DamageMultiplierList.serializedProperty.GetArrayElementAtIndex(index);
            var gameObject = damageMultiplier.FindPropertyRelative("m_GameObject");
            var multiplier = damageMultiplier.FindPropertyRelative("m_Multiplier");
            EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, (rect.width - 90), EditorGUIUtility.singleLineHeight), gameObject, new GUIContent());
            multiplier.floatValue = EditorGUI.FloatField(new Rect(rect.x + (rect.width - 90), rect.y, 90, EditorGUIUtility.singleLineHeight), multiplier.floatValue);

            if (EditorGUI.EndChangeCheck()) {
                var serializedObject = m_DamageMultiplierList.serializedProperty.serializedObject;
                Undo.RecordObject(serializedObject.targetObject, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(serializedObject.targetObject);
            }
        }
    }
}