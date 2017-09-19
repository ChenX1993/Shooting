using UnityEngine;

namespace Opsive.ThirdPersonController.Demos.Adventure
{
    /// <summary>
    /// Demo script which will start and stop the audio based on the animation event.
    /// </summary>
    public class AudioEvent : MonoBehaviour
    {
        // Component references
        private AudioSource m_AudioSource;

        /// <summary>
        /// Cache the component references.
        /// </summary>
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Plays the AudioSource.
        /// </summary>
        private void PlayAudio()
        {
            m_AudioSource.Play();
        }

        /// <summary>
        /// Stops the AudioSource.
        /// </summary>
        private void StopAudio()
        {
            m_AudioSource.Stop();
        }
    }
}