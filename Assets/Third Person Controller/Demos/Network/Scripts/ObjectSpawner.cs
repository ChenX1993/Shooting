using UnityEngine;
using UnityEngine.Networking;

namespace Opsive.ThirdPersonController.Demos.Networking
{
    /// <summary>
    /// Demo component which spawns a set of network objects when the server is started.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
        /// <summary>
        /// The object, position, and rotation to spawn.
        /// </summary>
        [System.Serializable]
        private class SpawnableObject
        {
            [Tooltip("The object that can spawn")]
            [SerializeField] private GameObject m_Object;
            [Tooltip("The position to spawn the object")]
            [SerializeField] private Vector3 m_Position;
            [Tooltip("The rotation to spawn the object")]
            [SerializeField] private Quaternion m_Rotation;

            // Exposed properties
            public GameObject Object { get { return m_Object; } }
            public Vector3 Position { get { return m_Position; } }
            public Quaternion Rotation { get { return m_Rotation; } }
        }

        // Internal variables
        [SerializeField] private SpawnableObject[] m_SpawnObjects;

        /// <summary>
        /// Register for any events the spawner should be aware of.
        /// </summary>
        private void Awake()
        {
            // The server is started when OnNetworkAddFirstPlayer is executed.
            EventHandler.RegisterEvent("OnNetworkAddFirstPlayer", StartServer);
        }

        /// <summary>
        /// The server has started, spawn the network objects.
        /// </summary>
        private void StartServer()
        {
            for (int i = 0; i < m_SpawnObjects.Length; ++i) {
                var obj = GameObject.Instantiate(m_SpawnObjects[i].Object, m_SpawnObjects[i].Position, m_SpawnObjects[i].Rotation) as GameObject;
                NetworkServer.Spawn(obj);
            }
        }
    }
}