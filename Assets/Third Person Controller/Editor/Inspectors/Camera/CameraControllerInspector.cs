using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for CameraController.
    /// </summary>
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerInspector : InspectorBase
    {
        // CameraController
        private static bool m_CharacterFoldout = true;
        private static bool m_DeathOrbitFoldout = true;
        private CameraController m_CameraController;
        private SerializedProperty m_CameraStates;
        private ReorderableList m_ReorderableCameraStateList;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            m_CameraController = target as CameraController;
            if (m_CameraController == null || serializedObject == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            // Show all of the fields.
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();

            if (m_ReorderableCameraStateList == null) {
                m_CameraStates = PropertyFromName(serializedObject, "m_CameraStates");
                m_ReorderableCameraStateList = new ReorderableList(serializedObject, m_CameraStates, true, true, true, true);
                m_ReorderableCameraStateList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Camera States");
                };
                m_ReorderableCameraStateList.drawElementCallback = OnCameraStateListDraw;
                m_ReorderableCameraStateList.onAddCallback = OnCameraStateListAdd;
                m_ReorderableCameraStateList.onRemoveCallback = OnCameraStateListRemove;
                m_ReorderableCameraStateList.onSelectCallback = (ReorderableList list) =>
                {
                    m_CameraController.SelectedCameraState = list.index;
                };
                if (m_CameraController.SelectedCameraState != -1) {
                    m_ReorderableCameraStateList.index = m_CameraController.SelectedCameraState;
                }
            }
            m_ReorderableCameraStateList.DoLayoutList();
            if ((!Application.isPlaying || AssetDatabase.GetAssetPath(m_CameraController).Length == 0) && m_ReorderableCameraStateList.index != -1) {
                if (m_ReorderableCameraStateList.index < m_CameraStates.arraySize) {
                    var cameraStateProperty = m_CameraStates.GetArrayElementAtIndex(m_ReorderableCameraStateList.index);
                    if (cameraStateProperty.objectReferenceValue != null) {
                        var cameraState = cameraStateProperty.objectReferenceValue as CameraState;
                        var cameraStateSerializedObject = new SerializedObject(cameraState);
                        if (CameraStateInspector.DrawCameraState(cameraState, cameraStateSerializedObject, false)) {
                            Undo.RecordObject(cameraState, "Inspector");
                            cameraStateSerializedObject.ApplyModifiedProperties();
                            InspectorUtility.SetObjectDirty(cameraState);
                        }
                    }
                } else {
                    m_ReorderableCameraStateList.index = m_CameraController.SelectedCameraState = -1;
                }
            } else if (Application.isPlaying && AssetDatabase.GetAssetPath(m_CameraController).Length == 0) {
                EditorGUILayout.LabelField("Active State", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                // If the game is playing draw the active state.
                var cameraStateSerializedObject = new SerializedObject(m_CameraController.ActiveState);
                if (CameraStateInspector.DrawCameraState(m_CameraController.ActiveState, cameraStateSerializedObject, true)) {
                    Undo.RecordObject(m_CameraController.ActiveState, "Inspector");
                    cameraStateSerializedObject.ApplyModifiedProperties();
                    InspectorUtility.SetObjectDirty(m_CameraController.ActiveState);
                }
                EditorGUI.indentLevel--;
            }

            if ((m_CharacterFoldout = EditorGUILayout.Foldout(m_CharacterFoldout, "Character Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                var initCharacterOnStartProperty = PropertyFromName(serializedObject, "m_InitCharacterOnStart");
                EditorGUILayout.PropertyField(initCharacterOnStartProperty);

                var characterProperty = PropertyFromName(serializedObject, "m_Character");
                if (initCharacterOnStartProperty.boolValue) {
                    characterProperty.objectReferenceValue = EditorGUILayout.ObjectField("Character", characterProperty.objectReferenceValue, typeof(GameObject), true, GUILayout.MinWidth(80)) as GameObject;
                    if (characterProperty.objectReferenceValue == null) {
                        EditorGUILayout.HelpBox("This field is required. The character specifies the GameObject that the CameraController should interact with.", MessageType.Error);
                    } else {
                        if ((characterProperty.objectReferenceValue as GameObject).GetComponent<Opsive.ThirdPersonController.Input.PlayerInput>() == null) {
                            EditorGUILayout.HelpBox("The Camera Controller component cannot reference an AI Agent. Ensure the Camera Controller is referencing a player controlled character.", MessageType.Error);
                        }
                    }
                } else if (characterProperty.objectReferenceValue != null) {
                    characterProperty.objectReferenceValue = null;
                }

                var autoAnchorProperty = PropertyFromName(serializedObject, "m_AutoAnchor");
                EditorGUILayout.PropertyField(autoAnchorProperty);
                if (autoAnchorProperty.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_AutoAnchorBone"));
                } else {
                    var anchorProperty = PropertyFromName(serializedObject, "m_Anchor");
                    anchorProperty.objectReferenceValue = EditorGUILayout.ObjectField("Anchor", anchorProperty.objectReferenceValue, typeof(Transform), true, GUILayout.MinWidth(80)) as Transform;
                    if (anchorProperty.objectReferenceValue == null) {
                        EditorGUILayout.HelpBox("The anchor specifies the Transform that the camera should follow. If null it will use the Character's Transform.", MessageType.Info);
                    }
                }
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_FadeTransform"));
                EditorGUI.indentLevel--;
            }

            // The following properties are only applicable to the third person view.
            if ((m_DeathOrbitFoldout = EditorGUILayout.Foldout(m_DeathOrbitFoldout, "Death Orbit Options", InspectorUtility.BoldFoldout))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeathAnchor"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_UseDeathOrbit"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeathRotationSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeathOrbitMoveSpeed"));
                EditorGUILayout.PropertyField(PropertyFromName(serializedObject, "m_DeathOrbitDistance"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(m_CameraController, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(m_CameraController);
            }
        }

        /// <summary>
        /// Draws all of the added camera states.
        /// </summary>
        private void OnCameraStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var cameraState = m_CameraStates.GetArrayElementAtIndex(index).objectReferenceValue as CameraState;
            m_CameraStates.GetArrayElementAtIndex(index).objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, rect.width, EditorGUIUtility.singleLineHeight), 
                                                                                                        cameraState, typeof(CameraState), false);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(m_CameraController, "Inspector");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(m_CameraController);
            }
        }

        /// <summary>
        /// The ReordableList add button has been pressed. Add a new CameraState.
        /// </summary>
        private void OnCameraStateListAdd(ReorderableList list)
        {
            var addMenu = new GenericMenu();
            addMenu.AddItem(new GUIContent("Add Existing State"), false, AddCameraStateElement);
            addMenu.AddItem(new GUIContent("Create New State"), false, CreateCameraState);
            addMenu.ShowAsContext();
        }

        /// <summary>
        /// Adds a new camera state element to the list.
        /// </summary>
        private void AddCameraStateElement()
        {
            m_CameraStates.InsertArrayElementAtIndex(m_CameraStates.arraySize);
            m_CameraStates.serializedObject.ApplyModifiedProperties();
            InspectorUtility.SetObjectDirty(m_CameraController);
        }

        /// <summary>
        /// Create a new camera state and add it to the list.
        /// </summary>
        private void CreateCameraState()
        {
            var path = EditorUtility.SaveFilePanel("Save Camera State", "Assets", "CameraState.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var cameraState = ScriptableObject.CreateInstance<Opsive.ThirdPersonController.Wrappers.CameraState>();
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(cameraState, path);
                AssetDatabase.ImportAsset(path);

                m_CameraStates.InsertArrayElementAtIndex(m_CameraStates.arraySize);
                m_CameraStates.GetArrayElementAtIndex(m_CameraStates.arraySize - 1).objectReferenceValue = cameraState;
                m_CameraStates.serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetObjectDirty(m_CameraController);
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected CameraState.
        /// </summary>
        private void OnCameraStateListRemove(ReorderableList list)
        {
            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            m_CameraStates.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
            m_CameraStates.DeleteArrayElementAtIndex(list.index);
            list.index = list.index - 1;
            if (list.index == -1 && m_CameraStates.arraySize > 0) {
                list.index = 0;
            }
            m_CameraController.SelectedCameraState = list.index;
            InspectorUtility.SetObjectDirty(m_CameraController);
        }
    }
}