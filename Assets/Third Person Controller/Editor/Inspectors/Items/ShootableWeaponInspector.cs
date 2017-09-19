using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for ShootableWeapon.
    /// </summary>
    [CustomEditor(typeof(ShootableWeapon))]
    public class ShootableWeaponInspector : ItemInspector
    {
        // ShootableWeapon
        [SerializeField] private static bool m_FireFoldout = true;
        [SerializeField] private static bool m_ClipFoldout = true;
        [SerializeField] private static bool m_ProjectileFoldout = true;
        [SerializeField] private static bool m_HitscanFoldout = true;
        [SerializeField] private static bool m_OverheatFoldout = true;
        [SerializeField] private static bool m_AudioFoldout = true;
        [SerializeField] private static bool m_AnimatorFoldout = true;
        [SerializeField] private static bool m_ShellFoldout = true;
        [SerializeField] private static bool m_MuzzleFlashFoldout = true;
        [SerializeField] private static bool m_SmokeFoldout = true;
        [SerializeField] private static bool m_AttachmentsFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var shootableWeapon = target as ShootableWeapon;
            if (shootableWeapon == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GameObject projectile = null;
            if ((m_FireFoldout = EditorGUILayout.Foldout(m_FireFoldout, "Fire Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DebugDrawFireRay"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CanUseInAir"));
                var firePoint = PropertyFromName(serializedObject, "m_FirePoint");
                EditorGUILayout.PropertyField(firePoint);
                if (firePoint.objectReferenceValue == null) {
                    EditorGUILayout.HelpBox("This field is required. The fire point specifies where the bullet should leave the weapon.", MessageType.Error);
                }
                var fireMode = PropertyFromName(serializedObject, "m_FireMode");
                EditorGUILayout.PropertyField(fireMode);
                if (fireMode.intValue == (int)ShootableWeapon.FireMode.Burst) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_BurstRate"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireRate"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_Spread"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireCount"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireType"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireOnUsedEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_WaitForEndUseEvent"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UseScope"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RecoilAmount"));
                var particles = PropertyFromName(serializedObject, "m_Particles");
                EditorGUILayout.PropertyField(particles);
                if (particles.objectReferenceValue != null) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_StopParticlesOnEndUse"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_ProjectileFoldout = EditorGUILayout.Foldout(m_ProjectileFoldout, "Projectile Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var projectileField = PropertyFromName(serializedObject, "m_Projectile");
                EditorGUILayout.PropertyField(projectileField);
                projectile = projectileField.objectReferenceValue as GameObject;
                EditorGUILayout.HelpBox("If a projectile GameObject is specified then this projectile will be fired from the weapon. If no projectile is specified then a hitscan will be used.", MessageType.Info);
                if (projectile != null) {
                    var alwaysVisible = PropertyFromName(serializedObject, "m_ProjectileAlwaysVisible");
                    EditorGUILayout.PropertyField(alwaysVisible);
                    if (alwaysVisible.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ProjectileRestLocation"));
                        EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ProjectileRestParent"));
                    }
                }
                EditorGUI.indentLevel--;
            }

            if (projectile == null) {
                if ((m_HitscanFoldout = EditorGUILayout.Foldout(m_HitscanFoldout, "Hitscan Options", InspectorUtility.BoldFoldout))) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawFloatInfinityField(PropertyFromName(serializedObject, "m_HitscanFireRange"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HitscanImpactLayers"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HitscanDamageEvent"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HitscanDamageAmount"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_HitscanImpactForce"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultHitscanDecal"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultHitscanDust"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultHitscanSpark"));
                    var tracer = PropertyFromName(serializedObject, "m_Tracer");
                    EditorGUILayout.PropertyField(tracer);
                    if (tracer.objectReferenceValue != null) {
                        var tracerLocation = PropertyFromName(serializedObject, "m_TracerLocation");
                        EditorGUILayout.PropertyField(tracerLocation);
                        if (tracerLocation == null) {
                            EditorGUILayout.HelpBox("This field is required. The tracer location specifies where the tracer should spawn from weapon.", MessageType.Error);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if ((m_ClipFoldout = EditorGUILayout.Foldout(m_ClipFoldout, "Clip Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawIntInfinityField(PropertyFromName(serializedObject, "m_ClipSize"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AutoReload"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RegenerateRate"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_RegenerateAmount"));
                EditorGUI.indentLevel--;
            }

            if ((m_OverheatFoldout = EditorGUILayout.Foldout(m_OverheatFoldout, "Overheat Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var overheat = PropertyFromName(serializedObject, "m_Overheat");
                EditorGUILayout.PropertyField(overheat);
                if (overheat.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_OverheatShotCount"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_CooldownDuration"));

                }
                EditorGUI.indentLevel--;
            }

            if ((m_AudioFoldout = EditorGUILayout.Foldout(m_AudioFoldout, "Audio Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireSound"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireSoundDelay"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_EmptyFireSound"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ReloadSound"), true);
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ReloadSoundDelay"), true);
                if (projectile == null) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DefaultHitscanImpactSound"));
                }
                EditorGUI.indentLevel--;
            }

            if ((m_AnimatorFoldout = EditorGUILayout.Foldout(m_AnimatorFoldout, "Animator Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_IdleAnimationStateName"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FireAnimationStateName"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ReloadAnimationStateName"));
                EditorGUI.indentLevel--;
            }

            if ((m_ShellFoldout = EditorGUILayout.Foldout(m_ShellFoldout, "Shell Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var shell = PropertyFromName(serializedObject, "m_Shell");
                EditorGUILayout.PropertyField(shell);
                if (shell.objectReferenceValue != null) {
                    var shellLocation = PropertyFromName(serializedObject, "m_ShellLocation");
                    EditorGUILayout.PropertyField(shellLocation);
                    if (shellLocation == null) {
                        EditorGUILayout.HelpBox("This field is required. The shell location specifies where the shell should leave the weapon.", MessageType.Error);
                    }
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShellForce"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShellTorque"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ShellDelay"));
                }
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

            if ((m_AttachmentsFoldout = EditorGUILayout.Foldout(m_AttachmentsFoldout, "Attachment Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var flashlight = PropertyFromName(serializedObject, "m_Flashlight");
                EditorGUILayout.PropertyField(flashlight);
                if (flashlight.objectReferenceValue != null) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ActivateFlashlightOnAim"));
                }
                var laserSight = PropertyFromName(serializedObject, "m_LaserSight");
                EditorGUILayout.PropertyField(laserSight);
                if (laserSight.objectReferenceValue != null) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_ActivateLaserSightOnAim"));
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DisableCrosshairsWhenLaserSightActive"));
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(shootableWeapon, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(shootableWeapon);
            }
        }
    }
}