using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Opsive.ThirdPersonController.Abilities;

namespace Opsive.ThirdPersonController.Editor
{
    /// <summary>
    /// Copies the Third Person Controller components from an existing character onto a new character.
    /// </summary>
    public class ComponentCopier : EditorWindow
    {
        // Internal fields
        private GameObject m_ExistingCharacter;
        private GameObject m_NewCharacter;
        private bool m_CopyCharacter = true;
        private bool m_CopyItems = true;
        private HashSet<string> m_SeenObjects = new HashSet<string>();

        [MenuItem("Tools/Third Person Controller/Component Copier (Experimental)", false, 41)]
        public static void ShowWindow()
        {
            var componentCopier = EditorWindow.GetWindow<ComponentCopier>(true, "Component Copier");
            componentCopier.minSize = componentCopier.maxSize = new Vector2(310, 160);
        }

        /// <summary>
        /// Draws the GUI.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("This editor is currently experimental.\nHave a backup before use and please report\nany problems to support@opsive.com.");
            GUILayout.Space(10);
            m_ExistingCharacter = EditorGUILayout.ObjectField("Existing Character", m_ExistingCharacter, typeof(GameObject), true) as GameObject;
            m_NewCharacter = EditorGUILayout.ObjectField("New Character", m_NewCharacter, typeof(GameObject), true) as GameObject;
            GUILayout.Space(5);
            m_CopyCharacter = EditorGUILayout.Toggle("Copy Character", m_CopyCharacter);
            m_CopyItems = EditorGUILayout.Toggle("Copy Items", m_CopyItems);

            GUI.enabled = m_ExistingCharacter != null && m_NewCharacter != null;
            if (GUI.enabled && (m_ExistingCharacter.GetComponent<RigidbodyCharacterController>() == null || m_NewCharacter.GetComponent<RigidbodyCharacterController>() == null)) {
                EditorGUILayout.HelpBox("Both the original and new character must have first been built with the Character Builder", MessageType.Warning);
                GUI.enabled = false;
            }
            if (GUILayout.Button("Copy")) {
                DoCopy();
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Does the copy of the character and item components.
        /// </summary>
        private void DoCopy()
        {
            m_SeenObjects.Clear();

            var copyFromActive = m_ExistingCharacter.activeSelf;
            var copyToActive = m_NewCharacter.activeSelf;
            m_ExistingCharacter.SetActive(true);
            m_NewCharacter.SetActive(true);

            // Copy all of the items by deleting and recreating.
            if (m_CopyItems) {
                var copyToItems = m_NewCharacter.GetComponentsInChildren<Item>();
                var copyFromItems = m_ExistingCharacter.GetComponentsInChildren<Item>();
                for (int i = copyToItems.Length - 1; i > -1; --i) {
                    var hasItem = false;
                    // Only remove the item if the to character has the item.
                    for (int j = 0; j < copyFromItems.Length; ++j) {
                        if (copyToItems[i].ItemType == copyFromItems[j].ItemType) {
                            hasItem = true;
                            break;
                        }
                    }
                    if (hasItem) {
                        GameObject.DestroyImmediate(copyToItems[i].gameObject, true);
                    }
                }
                for (int i = 0; i < copyFromItems.Length; ++i) {
                    var newItem = CopyGameObject(GetGameObjectPath(m_ExistingCharacter), GetGameObjectPath(m_NewCharacter), copyFromItems[i].gameObject);
                    CheckFields(GetGameObjectPath(m_ExistingCharacter), GetGameObjectPath(m_NewCharacter), newItem.GetComponent<Item>());
                    m_SeenObjects.Clear();
                }
            }

            if (m_CopyCharacter) {
                var copyFromComponents = m_ExistingCharacter.GetComponents<Component>();
                for (int i = 0; i < copyFromComponents.Length; ++i) {
                    if (copyFromComponents[i].GetType().Namespace.Contains("Opsive.ThirdPersonController") || typeof(Ability).IsAssignableFrom(copyFromComponents[i].GetType()) ||
                        typeof(Rigidbody).IsAssignableFrom(copyFromComponents[i].GetType()) || typeof(Collider).IsAssignableFrom(copyFromComponents[i].GetType()) ||
                        typeof(Animator).IsAssignableFrom(copyFromComponents[i].GetType())) {
                        Component copyToComponent = null;
                        var copyToComponents = m_NewCharacter.GetComponents(copyFromComponents[i].GetType());
                        if (copyToComponents.Length == 0) {
                            copyToComponent = m_NewCharacter.AddComponent(copyFromComponents[i].GetType());
                        } else if (copyToComponents.Length == 1) {
                            copyToComponent = copyToComponents[0];
                        } else {
                            continue;
                        }

                        EditorUtility.CopySerialized(copyFromComponents[i], copyToComponent);

                        // Insure the references are pointing to the correct character.
                        CheckFields(GetGameObjectPath(m_ExistingCharacter), GetGameObjectPath(m_NewCharacter), copyToComponent);
                        m_SeenObjects.Clear();
                    }
                }
            }

            m_ExistingCharacter.SetActive(copyFromActive);
            m_NewCharacter.SetActive(copyToActive);
        }

        /// <summary>
        /// Checks all of the fields and ensures Unity objects are pointing to the object on the new character rather than the old.
        /// </summary>
        private void CheckFields(string origPath, string parentPath, object obj)
        {
            if (obj == null) {
                return;
            }
            var hash = origPath + obj.GetHashCode();
            if (m_SeenObjects.Contains(hash)) {
                return;
            }
            m_SeenObjects.Add(origPath + obj.GetHashCode());

            var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; ++i) {
                if (typeof(Component).IsAssignableFrom(fields[i].FieldType)) {
                    var fieldObj = fields[i].GetValue(obj) as Component;
                    if (fieldObj != null) {
                        fields[i].SetValue(obj, CopyComponent(origPath, parentPath, fieldObj));
                    }
                } else if (typeof(IList).IsAssignableFrom(fields[i].FieldType) && typeof(Component).IsAssignableFrom(fields[i].FieldType.GetElementType())) {
                    var list = fields[i].GetValue(obj) as IList<Component>;
                    if (list != null) {
                        for (int j = 0; j < list.Count; ++j) {
                            var newComponent = CopyComponent(origPath, parentPath, list[j]);
                            if (newComponent != null) {
                                list[j] = newComponent;
                            }
                        }
                    }
                } else if(typeof(GameObject).IsAssignableFrom(fields[i].FieldType)) {
                    var fieldObj = fields[i].GetValue(obj) as GameObject;
                    if (fieldObj != null) {
                        fields[i].SetValue(obj, CopyGameObject(origPath, parentPath, fieldObj));
                    }
                } else if (typeof(IList).IsAssignableFrom(fields[i].FieldType) && typeof(GameObject).IsAssignableFrom(fields[i].FieldType.GetElementType())) {
                    var list = fields[i].GetValue(obj) as IList<GameObject>;
                    if (list != null) {
                        for (int j = 0; j < list.Count; ++j) {
                            var newGameObject = CopyGameObject(origPath, parentPath, list[j]);
                            if (newGameObject != null) {
                                list[j] = newGameObject;
                            }
                        }
                    }
                } else if (fields[i].FieldType.IsClass && !fields[i].FieldType.Equals(typeof(Type)) && !typeof(Delegate).IsAssignableFrom(fields[i].FieldType) && 
                            !typeof(string).IsAssignableFrom(fields[i].FieldType)) {
                    if (typeof(IList).IsAssignableFrom(fields[i].FieldType)) {
                        var list = fields[i].GetValue(obj) as IList<object>;
                        if (list != null) {
                            for (int j = 0; j < list.Count; ++j) {
                                CheckFields(origPath, parentPath, list[j]);
                            }
                        }
                    } else {
                        CheckFields(origPath, parentPath, fields[i].GetValue(obj));
                    }
                }
            }
        }

        /// <summary>
        /// Returns the full path for the GameObject.
        /// </summary>
        private static string GetGameObjectPath(GameObject obj)
        {
            var path = obj.name;
            while (obj.transform.parent != null) {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }

        /// <summary>
        /// Copies the component.
        /// </summary>
        private Component CopyComponent(string origPath, string parentPath, Component orig)
        {
            var newGameObject = CopyGameObject(origPath, parentPath, orig.gameObject);
            var newComponent = newGameObject.GetComponent(orig.GetType());
            if (newComponent == null) {
                newComponent = newGameObject.AddComponent(orig.GetType());
                EditorUtility.CopySerialized(orig, newComponent);
                // Insure the references are pointing to the correct character.
                CheckFields(origPath, parentPath, newComponent);
            }
            return newComponent;
        }

        /// <summary>
        /// Copies the GameObject.
        /// </summary>
        private GameObject CopyGameObject(string origPath, string parentPath, GameObject orig)
        {
            if (AssetDatabase.GetAssetPath(orig).Length > 0) {
                return orig;
            }
            var path = GetGameObjectPath(orig);
            var newPath = path.Replace(origPath, parentPath);
            var newGameObject = GameObject.Find(newPath);
            // The new GameObject may not exist yet.
            if (newGameObject == null) {
                newGameObject = GameObject.Instantiate(orig.gameObject) as GameObject;
                newGameObject.name = orig.gameObject.name;
                newGameObject.layer = orig.gameObject.layer;

                path = GetGameObjectPath(orig.transform.parent.gameObject);
                var newPathParent = path.Replace(origPath, parentPath);
                var parentGameObject = GameObject.Find(newPathParent);
                newGameObject.transform.parent = parentGameObject.transform;
            }

            newGameObject.transform.localPosition = orig.transform.localPosition;
            newGameObject.transform.localRotation = orig.transform.localRotation;
            return newGameObject;
        }
    }
}