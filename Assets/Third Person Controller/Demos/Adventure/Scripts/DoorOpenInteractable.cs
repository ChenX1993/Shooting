using UnityEngine;

namespace Opsive.ThirdPersonController.Demos.Adventure
{
    /// <summary>
    /// Demo script which opens the door and changes the light color when the character interacts with the interactable.
    /// </summary>
    public class DoorOpenInteractable : AnimatedInteractable
    {
        [Tooltip("The animator of the door")]
        [SerializeField] private Animator m_DoorAnimator;
        [Tooltip("The door close color")]
        [SerializeField] private Color m_CloseLightColor;
        [Tooltip("The door close color")]
        [SerializeField] private Color m_OpenLightColor;
        [Tooltip("The light to change the color of")]
        [SerializeField] private Light m_Light;
        [Tooltip("The material to change the color of")]
        [SerializeField] private Material m_LightMaterial;

        // Internal variables
        private const string MainColor = "_Color";
        private const string EmissionColor = "_EmissionColor";
        private bool m_HasInteracted;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_LightMaterial.SetColor(MainColor, m_CloseLightColor);
            m_LightMaterial.SetColor(EmissionColor, m_CloseLightColor);
        }

        /// <summary>
        /// Can the object be interacted with?
        /// </summary>
        /// <returns>True if the object can be interacted with.</returns>
        public override bool IsInteractionReady()
        {
            return !m_HasInteracted && base.IsInteractionReady();
        }

        /// <summary>
        /// Play the interaction animation.
        /// </summary>
        public override void Interact()
        {
            base.Interact();

            // The interactable can only be interacted with once.
            m_HasInteracted = true;

            // Open the door and change the light color.
            m_DoorAnimator.SetBool("Open", true);
            m_Light.color = m_OpenLightColor;
            m_LightMaterial.SetColor(MainColor, m_OpenLightColor);
            m_LightMaterial.SetColor(EmissionColor, m_OpenLightColor);
        }
    }
}