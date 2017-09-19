using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for CameraStates.
    /// </summary>
    [CustomEditor(typeof(CameraState))]
    public class CameraStateInspector : InspectorBase
    {
        // CameraController
        private static bool m_LookFoldout = true;
        private static bool m_MovementFoldout = true;
        private static bool m_CharacterFadeFoldout = true;
        private static bool m_TargetLock = true;
        private static bool m_RecoilFoldout = true;
        private static bool m_RestrictionsFoldout = true;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var cameraState = target as CameraState;
            if (cameraState == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();

            if (DrawCameraState(cameraState, serializedObject, false)) {
                Undo.RecordObject(cameraState, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(cameraState);
            }
        }

        /// <summary>
        /// Draws the specified CameraState.
        /// </summary>
        /// <param name="cameraState">The camera state to draw.</param>
        /// <param name="serializedObject">The SerializedObject of the CameraState.</param>
        /// <param name="forceShow">Should the state be force shown?</param>
        /// <returns>True if there were changes to the camera state.</returns>
        public static bool DrawCameraState(CameraState cameraState, SerializedObject serializedObject, bool forceShow)
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty applyState = null;
            if (!forceShow) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Exclusive"));

                applyState = serializedObject.FindProperty("m_ApplyViewMode");
                EditorGUILayout.PropertyField(applyState);
            }
            if (forceShow || applyState.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ViewMode"));
                EditorGUI.indentLevel--;
            }

            if (cameraState.ViewMode != CameraMonitor.CameraViewMode.Pseudo3D && (m_LookFoldout = EditorGUILayout.Foldout(m_LookFoldout, "Look Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (!forceShow) {
                    applyState = serializedObject.FindProperty("m_ApplyPitchLimit");
                    EditorGUILayout.PropertyField(applyState);
                }
                if (forceShow || applyState.boolValue) {
                    EditorGUI.indentLevel++;
                    var minPitchProperty = serializedObject.FindProperty("m_MinPitchLimit");
                    var maxPitchProperty = serializedObject.FindProperty("m_MaxPitchLimit");
                    var minValue = Mathf.Round(minPitchProperty.floatValue * 100f) / 100f;
                    var maxValue = Mathf.Round(maxPitchProperty.floatValue * 100f) / 100f;
                    InspectorUtility.DrawMinMaxLabeledFloatSlider("Pitch Limit", ref minValue, ref maxValue,
                        (cameraState.ViewMode == CameraMonitor.CameraViewMode.ThirdPerson || cameraState.ViewMode == CameraMonitor.CameraViewMode.RPG) ? -90 : 0, 90);
                    minPitchProperty.floatValue = minValue;
                    maxPitchProperty.floatValue = maxValue;
                    EditorGUI.indentLevel--;
                }

                // Cover yaw limits are only applicable to the third person view.
                if (cameraState.ViewMode == CameraMonitor.CameraViewMode.ThirdPerson || cameraState.ViewMode == CameraMonitor.CameraViewMode.RPG) {
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyYawLimit");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        var minYawProperty = serializedObject.FindProperty("m_MinYawLimit");
                        var maxYawProperty = serializedObject.FindProperty("m_MaxYawLimit");
                        var minValue = Mathf.Round(minYawProperty.floatValue * 100f) / 100f;
                        var maxValue = Mathf.Round(maxYawProperty.floatValue * 100f) / 100f;
                        InspectorUtility.DrawMinMaxLabeledFloatSlider("Yaw Limit", ref minValue, ref maxValue, -180, 180);
                        minYawProperty.floatValue = minValue;
                        maxYawProperty.floatValue = maxValue;
                        EditorGUI.indentLevel--;
                    }
                }

                if (!forceShow) {
                    applyState = serializedObject.FindProperty("m_ApplyIgnoreLayerMask");
                    EditorGUILayout.PropertyField(applyState);
                }
                if (forceShow || applyState.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreLayerMask"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            if ((m_MovementFoldout = EditorGUILayout.Foldout(m_MovementFoldout, "Movement Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (!forceShow) {
                    applyState = serializedObject.FindProperty("m_ApplyMoveSmoothing");
                    EditorGUILayout.PropertyField(applyState);
                }
                if (forceShow || applyState.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MoveSmoothing"));
                    EditorGUI.indentLevel--;
                }
                if (!forceShow) {
                    applyState = serializedObject.FindProperty("m_ApplyCameraOffset");
                    EditorGUILayout.PropertyField(applyState);
                }
                if (forceShow || applyState.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CameraOffset"), true);
                    EditorGUI.indentLevel--;
                }
                // The following properties are only applicable to the third person view.
                if (cameraState.ViewMode == CameraMonitor.CameraViewMode.ThirdPerson || cameraState.ViewMode == CameraMonitor.CameraViewMode.RPG) {
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplySmartPivot");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SmartPivot"), true);
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyFieldOfView");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FieldOfView"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FieldOfViewSpeed"));
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyTurn");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TurnSmoothing"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TurnSpeed"));
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyStepZoom");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if(forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StepZoomSensitivity"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinStepZoom"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxStepZoom"));
                        EditorGUI.indentLevel--;
                    }
                } else {
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyRotationSpeed");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RotationSpeed"));
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyView");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ViewDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ViewStep"));
                        if (cameraState.ViewMode == CameraMonitor.CameraViewMode.Pseudo3D) {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LookDirection"));
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                if (cameraState.ViewMode != CameraMonitor.CameraViewMode.Pseudo3D) {
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyCollisionRadius");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CollisionRadius"));
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }

            if ((cameraState.ViewMode == CameraMonitor.CameraViewMode.ThirdPerson || cameraState.ViewMode == CameraMonitor.CameraViewMode.RPG) &&
                    (m_CharacterFadeFoldout = EditorGUILayout.Foldout(m_CharacterFadeFoldout, "Character Fade Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                if (!forceShow) {
                    applyState = serializedObject.FindProperty("m_ApplyFadeCharacter");
                    EditorGUILayout.PropertyField(applyState);
                }
                if (forceShow || applyState.boolValue) {
                    EditorGUI.indentLevel++;
                    var fade = serializedObject.FindProperty("m_FadeCharacter");
                    EditorGUILayout.PropertyField(fade);
                    if (fade.boolValue) {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartFadeDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EndFadeDistance"));
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            // The following properties are only applicable to the third person view.
            if (cameraState.ViewMode == CameraMonitor.CameraViewMode.ThirdPerson || cameraState.ViewMode == CameraMonitor.CameraViewMode.RPG) {
                if ((m_TargetLock = EditorGUILayout.Foldout(m_TargetLock, "Target Lock Options", InspectorUtility.BoldFoldout))) {
                    EditorGUI.indentLevel++;
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyTargetLock");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        var useTargetLock = serializedObject.FindProperty("m_UseTargetLock");
                        EditorGUILayout.PropertyField(useTargetLock);
                        if (useTargetLock.boolValue) {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetLockSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BreakForce"));
                            var useHumanoidTargetLock = serializedObject.FindProperty("m_UseHumanoidTargetLock");
                            EditorGUILayout.PropertyField(useHumanoidTargetLock);
                            if (useHumanoidTargetLock.boolValue) {
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HumanoidTargetLockBone"));
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if ((m_RecoilFoldout = EditorGUILayout.Foldout(m_RecoilFoldout, "Recoil Options", InspectorUtility.BoldFoldout))) {
                    EditorGUI.indentLevel++;
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyRecoil");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RecoilSpring"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RecoilDampening"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if ((m_RestrictionsFoldout = EditorGUILayout.Foldout(m_RestrictionsFoldout, "Restrictions Options", InspectorUtility.BoldFoldout))) {
                    EditorGUI.indentLevel++;
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyObstructionCheck");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ObstructionCheck"));
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyStaticHeight");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StaticHeight"));
                        EditorGUI.indentLevel--;
                    }
                    if (!forceShow) {
                        applyState = serializedObject.FindProperty("m_ApplyVerticalOffset");
                        EditorGUILayout.PropertyField(applyState);
                    }
                    if (forceShow || applyState.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_VerticalOffset"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            }
            return EditorGUI.EndChangeCheck();
        }
    }
}