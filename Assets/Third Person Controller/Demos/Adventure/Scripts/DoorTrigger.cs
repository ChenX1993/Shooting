using UnityEngine;

namespace Opsive.ThirdPersonController.Demos.Adventure
{
    /// <summary>
    /// Demo script which will automatically open and close the door when the character enters and leaves the trigger.
    /// </summary>
    public class DoorTrigger : MonoBehaviour
    {
        [Tooltip("The name of the state to play when the door is opened")]
        [SerializeField] private string m_OpenName;
        [Tooltip("The name of the state to play when the door is closed")]
        [SerializeField] private string m_CloseName;
        [Tooltip("The transition duration between states")]
        [SerializeField] private float m_TransitionDuration;

        // Internal variables
        private int m_OpenStateHash;
        private int m_CloseStateHash;

        // Component references
        private Animator m_Animator;
        private AudioSource m_AudioSource;

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_AudioSource = GetComponent<AudioSource>();

            m_OpenStateHash = Animator.StringToHash(m_OpenName);
            m_CloseStateHash = Animator.StringToHash(m_CloseName);
        }

        /// <summary>
        /// Open the door when the character enters the trigger.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerManager.Player) {
                m_Animator.CrossFade(m_OpenStateHash, m_TransitionDuration);
                m_AudioSource.Play();
            }
        }

        /// <summary>
        /// Close the door when the character exits the trigger.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerManager.Player) {
                m_Animator.CrossFade(m_CloseStateHash, m_TransitionDuration);
                m_AudioSource.Play();
            }
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