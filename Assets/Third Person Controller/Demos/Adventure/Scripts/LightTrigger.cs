using UnityEngine;

namespace Opsive.ThirdPersonController.Demos.Adventure
{
    /// <summary>
    /// Demo script which will automatically set the light color based on when the character enters and leaves the trigger.
    /// </summary>
    public class LightTrigger : MonoBehaviour
    {
        [Tooltip("The color of the light when the character is inside the trigger")]
        [SerializeField] private Color m_EnterColor;
        [Tooltip("The color of the light when the character is outside the trigger")]
        [SerializeField] private Color m_ExitColor;
        [Tooltip("The lights to change the color of")]
        [SerializeField] private Light[] m_Lights;
        [Tooltip("The material to change")]
        [SerializeField] private Material m_LightMaterial;

        // Internal variables
        private const string MainColor = "_Color";
        private const string EmissionColor = "_EmissionColor";

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            SetColor(m_ExitColor);
        }

        /// <summary>
        /// Change the light color when the character enters the trigger.
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerManager.Player) {
                SetColor(m_EnterColor);
            }
        }

        /// <summary>
        /// Change the light color when the character enters the trigger.
        /// </summary>
        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerManager.Player) {
                SetColor(m_ExitColor);
            }
        }

        /// <summary>
        /// Sets the color of the light and light material.
        /// </summary>
        /// <param name="color">The color to set.</param>
        private void SetColor(Color color)
        {
            for (int i = 0; i < m_Lights.Length; ++i) {
                m_Lights[i].color = color;
            }
            m_LightMaterial.SetColor(MainColor, color);
            m_LightMaterial.SetColor(EmissionColor, color);
        }
    }
}