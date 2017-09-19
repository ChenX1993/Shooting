using UnityEngine;
using Opsive.ThirdPersonController.Abilities;

namespace Opsive.ThirdPersonController.Demos.Clean
{
    /// <summary>
    /// Adds a basic fog effect when the character is underwater.
    /// </summary>
    public class UnderwaterEffect : MonoBehaviour
    {
        [Tooltip("The height of the water surface")]
        [SerializeField] private float m_WaterSurfaceHeight;
        [Tooltip("The color of the water fog")]
        [SerializeField] private Color m_WaterColor = new Color(0, 0.4f, 0.7f, 0.6f);

        private Transform m_Transform;
        private Dive m_CharacterDive;
        private GlobalFog m_GlobalFog;

        private bool m_WaterEffectEnabled;
        private Color m_DefaultColor;

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_DefaultColor = RenderSettings.fogColor;
            m_GlobalFog = GetComponent<GlobalFog>();
            EventHandler.RegisterEvent<GameObject>("OnCameraAttachCharacter", AttachCharacter);
            EnableEffect(false);
        }

        /// <summary>
        /// Change the water effect based on depth.
        /// </summary>
        private void Update()
        {
            // Enable the effect if the Dive ability is active and the camera position is less then the water surface height.
            var enableEffect = false;
            if (m_CharacterDive != null && m_CharacterDive.IsActive && m_Transform.position.y < m_WaterSurfaceHeight) {
                enableEffect = true;
            }
            if (m_WaterEffectEnabled != enableEffect) {
                EnableEffect(enableEffect);
            }
        }

        /// <summary>
        /// Enables or disables the water effect.
        /// </summary>
        /// <param name="enableEffect">Should the effect be enabled?</param>
        private void EnableEffect(bool enableEffect)
        {
            m_GlobalFog.enabled = enableEffect;
            m_WaterEffectEnabled = enableEffect;
            if (enableEffect) {
                RenderSettings.fog = true;
                RenderSettings.fogColor = m_WaterColor;
                RenderSettings.fogMode = FogMode.Linear;
            } else {
                RenderSettings.fog = false;
                RenderSettings.fogColor = m_DefaultColor;
                RenderSettings.fogMode = FogMode.Exponential;
            }
        }

        /// <summary>
        /// The character has been attached to the camera.
        /// </summary>
        /// <param name="character">The character that has been attached to the camera.</param>
        private void AttachCharacter(GameObject character)
        {
            m_CharacterDive = character.GetComponent<Dive>();
        }

        /// <summary>
        /// The EventHandler was cleared. This will happen when a new scene is loaded. Unregister the registered events to prevent old events from being fired.
        /// </summary>
        private void EventHandlerClear()
        {
            EventHandler.UnregisterEvent<GameObject>("OnCameraAttachCharacter", AttachCharacter);
        }
    }
}