using UnityEngine;
using UnityEditor;
#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Utility class for the Third Person Controller inspectors.
    /// </summary>
    public static class InspectorUtility
    {
        private static GUIStyle m_BoldFoldout;
        public static GUIStyle BoldFoldout
        {
            get
            {
                if (m_BoldFoldout == null) {
                    m_BoldFoldout = new GUIStyle(EditorStyles.foldout);
                    m_BoldFoldout.fontStyle = FontStyle.Bold;
                }
                return m_BoldFoldout;
            }
        }

        /// <summary>
        /// Draws a float slider which has a min and max label beside it.
        /// </summary>
        /// <param name="name">The name of the slider.</param>
        /// <param name="minValue">The current minimum value.</param>
        /// <param name="maxValue">The current maximum value.</param>
        /// <param name="min">The minimum value that can be selected.</param>
        /// <param name="max">The maximum value that can be selected.</param>
        public static void DrawMinMaxLabeledFloatSlider(string name, ref float minValue, ref float maxValue, float min, float max)
        {
            EditorGUILayout.BeginHorizontal();
            minValue = EditorGUILayout.FloatField(name, minValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, min, max);
            maxValue = EditorGUILayout.FloatField(maxValue);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a float field with an infinity toggle to the right of it.
        /// </summary>
        /// <param name="property">The float property.</param>
        public static void DrawFloatInfinityField(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            var prevInfinity = (property.floatValue == float.PositiveInfinity);
            GUI.enabled = !prevInfinity;
            EditorGUILayout.PropertyField(property);
            GUI.enabled = true;
            var infinity = EditorGUILayout.ToggleLeft("Infinity", prevInfinity, GUILayout.Width(73));
            if (prevInfinity != infinity) {
                if (infinity) {
                    property.floatValue = float.PositiveInfinity;
                } else {
                    property.floatValue = 1;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws an int field with an infinity toggle to the right of it.
        /// </summary>
        /// <param name="property">The int property.</param>
        public static void DrawIntInfinityField(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            var prevInfinity = (property.intValue == int.MaxValue);
            GUI.enabled = !prevInfinity;
            EditorGUILayout.PropertyField(property);
            GUI.enabled = true;
            var infinity = EditorGUILayout.ToggleLeft("Infinity", prevInfinity, GUILayout.Width(73));
            if (prevInfinity != infinity) {
                if (infinity) {
                    property.intValue = int.MaxValue;
                } else {
                    property.intValue = 1;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Marks the scene object as dirty.
        /// </summary>
        /// <param name="obj">The object that was changed.</param>
        public static void SetObjectDirty(Object obj)
        {
            if (EditorApplication.isPlaying) {
                return;
            }
            
#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            if (!EditorUtility.IsPersistent(obj)) {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                return;
            }
#endif
            EditorUtility.SetDirty(obj);
        }
    }
}