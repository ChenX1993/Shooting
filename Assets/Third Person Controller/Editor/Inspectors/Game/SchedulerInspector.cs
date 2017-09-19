using UnityEngine;
using UnityEditor;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Shows a custom inspector for the Scheduler.
    /// </summary>
    [CustomEditor(typeof(Scheduler))]
    public class SchedulerInspector : InspectorBase
    {
        /// <summary>
        /// Draws the scheduled events list.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var scheduler = target as Scheduler;
            if (scheduler == null)
                return; // How'd this happen?

            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Events Scheduled: " + scheduler.ActiveEvents.Count);
            if (scheduler.ActiveEvents.Count > 0) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time", GUILayout.Width(30));
                EditorGUILayout.LabelField("Target", GUILayout.Width(100));
                EditorGUILayout.LabelField("Method");
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < scheduler.ActiveEvents.Count; ++i) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField((scheduler.ActiveEvents[i].EndTime - Time.time).ToString("0.##"), GUILayout.Width(30));
                    var targetName = "";
                    var methodName = "";
                    if (scheduler.ActiveEvents[i].Callback != null) {
                        if (scheduler.ActiveEvents[i].Callback.Target is Object) {
                            targetName = (scheduler.ActiveEvents[i].Callback.Target as Object).name;
                        } else {
                            targetName = scheduler.ActiveEvents[i].Callback.Target.ToString();
                        }
                        methodName = scheduler.ActiveEvents[i].Callback.Method.Name;
                    } else if (scheduler.ActiveEvents[i].CallbackArg != null) {
                        if (scheduler.ActiveEvents[i].CallbackArg.Target is Object) {
                            targetName = (scheduler.ActiveEvents[i].CallbackArg.Target as Object).name;
                        } else {
                            targetName = scheduler.ActiveEvents[i].CallbackArg.Target.ToString();
                        }
                    }
                    EditorGUILayout.LabelField(targetName, GUILayout.Width(100));
                    EditorGUILayout.LabelField(methodName);
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Keep repainting the inspector so the events/duration refreshes.
            Repaint();
        }
    }
}