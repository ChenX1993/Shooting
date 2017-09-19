using UnityEngine;

namespace Opsive.ThirdPersonController.Demos.Clean
{
    /// <summary>
    /// A demo SpawnSelection which will spawn based on the current genre.
    /// </summary>
    public class CleanSpawnSelection : SpawnSelection
    {
        [Tooltip("The spawn location for the 2.5D genre")]
        [SerializeField] private Transform[] m_Pseudo3DSpawnLocations;
        [Tooltip("A reference to the DemoManager")]
        [SerializeField] private DemoManager m_DemoManager;

        /// <summary>
        /// Internal method for returning a random spawn location from the spawn location list.
        /// </summary>
        /// <returns>The Transform of a random spawn location.</returns>
        protected override Transform GetSpawnLocationInternal()
        {
            if (m_DemoManager.CurrentGenre == DemoManager.Genre.Pseudo3D) {
                return m_Pseudo3DSpawnLocations[Random.Range(0, m_Pseudo3DSpawnLocations.Length - 1)];
            }
            // The current genre is not 2.5D so spawn with the base class.
            return base.GetSpawnLocationInternal();
        }
    }
}