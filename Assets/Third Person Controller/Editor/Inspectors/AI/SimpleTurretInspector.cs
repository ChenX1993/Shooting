using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for SimpleTurret.
    /// </summary>
    [CustomEditor(typeof(SimpleTurret))]
    public class SimpleTurretInspector : InspectorBase
    {
        private static bool m_FireFoldout = true;
        private static bool m_MuzzleFlashFoldout = true;
        private static bool m_SmokeFoldout = true;
        private static bool m_AudioFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var turret = target as SimpleTurret;
            if (turret == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            var turretHeadProperty = PropertyFromName(serializedObject, "m_TurretHead");
            turretHeadProperty.objectReferenceValue = EditorGUILayout.ObjectField("Turret Head", turretHeadProperty.objectReferenceValue, typeof(GameObject), true, GUILayout.MinWidth(80)) as GameObject;
            if (turretHeadProperty.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("This field is required. The turret head specifies the GameObject that can rotate to aim at the target.", MessageType.Error);
            }
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RotationSpeed"));
            EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_TargetLayers"));

            if ((m_FireFoldout = EditorGUILayout.Foldout(m_FireFoldout, "Fire Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var projectile = PropertyFromName(serializedObject, "m_Projectile");
                EditorGUILayout.PropertyField(projectile);
                if (projectile.objectReferenceValue == null) {
                    EditorGUILayout.HelpBox("This field is required. The projectile specifies the GameObject that will damage the target.", MessageType.Error);
                }

                var firePoint = PropertyFromName(serializedObject, "m_FirePoint");
                EditorGUILayout.PropertyField(firePoint);
                if (firePoint.objectReferenceValue == null) {
                    EditorGUILayout.HelpBox("This field is required. The fire point specifies where the bullet should leave the weapon.", MessageType.Error);
                }

                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireRange"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireRate"));
                EditorGUI.indentLevel--;
            }

            if ((m_AudioFoldout = EditorGUILayout.Foldout(m_AudioFoldout, "Audio Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireSound"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireSoundDelay"));
                EditorGUI.indentLevel--;
            }

            if ((m_MuzzleFlashFoldout = EditorGUILayout.Foldout(m_MuzzleFlashFoldout, "Muzzle Flash Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var muzzleFlash = PropertyFromName(serializedObject, "m_MuzzleFlash");
                EditorGUILayout.PropertyField(muzzleFlash);
                if (muzzleFlash.objectReferenceValue != null) {
                    var muzzleFlashLocation = PropertyFromName(serializedObject, "m_MuzzleFlashLocation");
                    EditorGUILayout.PropertyField(muzzleFlashLocation);
                    if (muzzleFlashLocation == null) {
                        EditorGUILayout.HelpBox("This field is required. The muzzle flash location specifies where the muzzle flash should appear from weapon.", MessageType.Error);
                    }
                }
                EditorGUI.indentLevel--;
            }

            if ((m_SmokeFoldout = EditorGUILayout.Foldout(m_SmokeFoldout, "Smoke Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var smoke = PropertyFromName(serializedObject, "m_Smoke");
                EditorGUILayout.PropertyField(smoke);
                if (smoke.objectReferenceValue != null) {
                    var smokeLocation = PropertyFromName(serializedObject, "m_SmokeLocation");
                    EditorGUILayout.PropertyField(smokeLocation);
                    if (smokeLocation == null) {
                        EditorGUILayout.HelpBox("This field is required. The smoke location specifies where the smoke should appear from the weapon.", MessageType.Error);
                    }
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(turret, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(turret);
            }
        }
    }
}