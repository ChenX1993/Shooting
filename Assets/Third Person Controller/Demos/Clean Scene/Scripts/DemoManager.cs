using UnityEngine;
using UnityEngine.UI;
using Opsive.ThirdPersonController.UI;

namespace Opsive.ThirdPersonController.Demos.Clean
{
    /// <summary>
    /// Manages the various Third Person Controller clean scene demos.
    /// </summary>
    public class DemoManager : MonoBehaviour
    {
        public enum Genre { Shooter, Adventure, RPG, Platformer, Pseudo3D, TopDown, PointClick, Last }
        private string[] m_Title = new string[] { "Shooter", "Adventure", "RPG", "Platformer", "2.5D", "Top Down", "Point and Click"};
        private string[] m_Description = new string[] {
            "This Shooter demo shows a combat movement type with root motion.",
            "This Adventure demo shows a adventure movement type with root motion.",
            "This RPG demo shows an alternate control type more commonly found in RPG games.",
            "This Platformer demo the character does not use root motion. Root motion allows for more \"realistic\" movement which is not desired for all game types.",
            "This 2.5D demo shows the character and camera controller being used in a 2.5D game. All of the items and abilities work as they do in a third person view.",
            "This Top Down demo shows the camera with a birds eye perspective.",
            "This Point and Click demo enables the character to move based on a mouse click instead of regular keyboard controls. Right click to use the item." };
        [Tooltip("A reference to the shooter demo character")]
        [SerializeField] private GameObject m_ShooterCharacter;
        [Tooltip("A reference to the adventure demo character")]
        [SerializeField] private GameObject m_AdventureCharacter;
        [Tooltip("A reference to the RPG demo character")]
        [SerializeField] private GameObject m_RPGCharacter;
        [Tooltip("A reference to the third person demo character")]
        [SerializeField] private GameObject m_PlatformerCharacter;
        [Tooltip("A reference to the 2.5D demo character")]
        [SerializeField] private GameObject m_Pseudo3DCharacter;
        [Tooltip("A reference to the top down demo character")]
        [SerializeField] private GameObject m_TopDownCharacter;
        [Tooltip("A reference to the point click demo character")]
        [SerializeField] private GameObject m_PointClickCharacter;
        [Tooltip("A reference to Blitz")]
        [SerializeField] private GameObject m_Blitz;
        [Tooltip("A reference to the genre title text")]
        [SerializeField] private Text m_GenreTitle;
        [Tooltip("A reference to the genre description text")]
        [SerializeField] private Text m_GenreDescription;
        [Tooltip("A reference to the crosshairs GameObject")]
        [SerializeField] private GameObject m_Crosshairs;
        [Tooltip("A reference to the combat weapon wheel")]
        [SerializeField] private GameObject m_CombatWeaponWheel;
        [Tooltip("A reference to the adventure weapon wheel")]
        [SerializeField] private GameObject m_AdventureWeaponWheel;
        [Tooltip("A reference to the RPG weapon wheel")]
        [SerializeField] private GameObject m_RPGWeaponWheel;
        [Tooltip("A reference to the 2.5D scene objects")]
        [SerializeField] private GameObject m_Pseudo3DObjects;
        [Tooltip("A reference to the ImageFader")]
        [SerializeField] private ImageFader m_ImageFader;
        [Tooltip("A reference to the item pickup GameObject")]
        [SerializeField] private Transform m_ItemPickups;
        [Tooltip("A reference to the pool barrier")]
        [SerializeField] private GameObject m_PoolBarrier;
        [Tooltip("A reference to the horse UI notice")]
        [SerializeField] private GameObject m_HorseNotice;

        public Genre CurrentGenre { get { return m_CurrentGenre; } }

        // Internal variables
        private Genre m_CurrentGenre = Genre.Shooter;
        private bool[] m_HasLoaded = new bool[(int)Genre.Last];
        private Vector3 m_BlitzPosition;
        private Quaternion m_BlitzRotation;

        // Component references
        private GameObject m_Character;
        private RigidbodyCharacterController m_CharacterController;
        private Inventory m_CharacterInventory;
        private AnimatorMonitor m_CharacterAnimatorMonitor;
        private CameraController m_CameraController;
        private CameraHandler m_CameraHandler;
        private GameObject m_WeaponWheel;
        private RigidbodyCharacterController m_BlitzCharacterController;
        
        /// <summary>
        /// Cache the component references.
        /// </summary>
        private void Awake()
        {
            m_CameraController = Camera.main.GetComponent<CameraController>();
            if (m_CameraController == null) {
                Debug.LogError("Error: Unable to find the CameraController.");
                enabled = false;
            }
            m_CameraHandler = m_CameraController.GetComponent<CameraHandler>();

            m_BlitzCharacterController = m_Blitz.GetComponent<RigidbodyCharacterController>();
            m_BlitzPosition = m_Blitz.transform.position;
            m_BlitzRotation = m_Blitz.transform.rotation;

            EventHandler.RegisterEvent<bool>(m_Blitz, "OnAllowGameplayInput", OnBlitzAllowGameplayInput);
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        private void Start()
        {
            m_GenreTitle.text = m_Title[(int)m_CurrentGenre];
            m_GenreDescription.text = m_Description[(int)m_CurrentGenre];
            m_HasLoaded[(int)m_CurrentGenre] = true;
            m_WeaponWheel = m_CombatWeaponWheel;
            m_AdventureWeaponWheel.SetActive(false);
            m_RPGWeaponWheel.SetActive(false);
            SwitchCharacters();
            m_PoolBarrier.SetActive(false);
            m_HorseNotice.SetActive(false);
        }

        /// <summary>
        /// A new genre has been selected. Switch characters.
        /// </summary>
        private void SwitchCharacters()
        {
            GameObject character = null;
            switch (m_CurrentGenre) {
                case Genre.Shooter:
                    character = m_ShooterCharacter;
                    break;
                case Genre.Adventure:
                    character = m_AdventureCharacter;
                    break;
                case Genre.RPG:
                    character = m_RPGCharacter;
                    break;
                case Genre.Platformer:
                    character = m_PlatformerCharacter;
                    break;
                case Genre.Pseudo3D:
                    character = m_Pseudo3DCharacter;
                    break;
                case Genre.TopDown:
                    character = m_TopDownCharacter;
                    break;
                case Genre.PointClick:
                    character = m_PointClickCharacter;
                    break;
            }
            character.SetActive(true);

            // Toggle the scheduler enable state by disabling and enabling it.
            var scheduler = GameObject.FindObjectOfType<Scheduler>();
            scheduler.enabled = false;
            scheduler.enabled = true;

            m_ImageFader.Fade();

            // Cache the character components.
            m_Character = character;
            m_CharacterController = character.GetComponent<RigidbodyCharacterController>();
            m_CharacterInventory = character.GetComponent<Inventory>();
            m_CharacterAnimatorMonitor = character.GetComponent<AnimatorMonitor>();
            m_CameraController.Character = character;
            m_CameraController.Anchor = character.transform;
            m_CameraController.DeathAnchor = character.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            m_CameraController.FadeTransform = character.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest);
        }

        /// <summary>
        /// Switch characters when the enter key is pressed.  
        /// </summary>
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Return)) {
                SwitchGenres(true);
            }
        }

        /// <summary>
        /// Switch to the previous or next genre.
        /// </summary>
        /// <param name="next">Switch to the next genre?</param>
        public void SwitchGenres(bool next)
        {
            m_CharacterController.TryStopAllAbilities(true);
            m_Character.SetActive(false);
            m_CurrentGenre = (Genre)(((int)m_CurrentGenre + (next ? 1 : -1)) % (int)Genre.Last);
            if ((int)m_CurrentGenre < 0) m_CurrentGenre = Genre.PointClick;
            m_GenreTitle.text = m_Title[(int)m_CurrentGenre];
            m_GenreDescription.text = m_Description[(int)m_CurrentGenre];
            m_WeaponWheel.SetActive(false);
            SwitchCharacters();
            GenreSwitched();
            m_HasLoaded[(int)m_CurrentGenre] = true;
        }

        /// <summary>
        /// The genre has been switched. Update the various object references.
        /// </summary>
        private void GenreSwitched()
        {
            switch (m_CurrentGenre) {
                case Genre.Shooter:
                    m_CameraHandler.ZoomStateName = "Zoom";
                    m_CameraController.ChangeState("TopDown", false);
                    m_CameraController.ChangeState("ThirdPerson", true);
                    m_Crosshairs.SetActive(true);
                    m_WeaponWheel = m_CombatWeaponWheel;
                    break;
                case Genre.Adventure:
                    m_WeaponWheel = m_AdventureWeaponWheel;
                    break;
                case Genre.RPG:
                    m_CameraController.ChangeState("ThirdPerson", false);
                    m_CameraController.ChangeState("RPG", true);
                    m_WeaponWheel = m_RPGWeaponWheel;
                    break;
                case Genre.Platformer:
                    m_CameraHandler.ZoomStateName = "Zoom";
                    m_CameraController.ChangeState("RPG", false);
                    m_CameraController.ChangeState("ThirdPerson", true);
                    m_Crosshairs.SetActive(true);
                    m_Pseudo3DObjects.SetActive(false);
                    m_WeaponWheel = m_CombatWeaponWheel;
                    break;
                case Genre.Pseudo3D:
                    m_CameraHandler.ZoomStateName = string.Empty;
                    m_CameraController.ChangeState("ThirdPerson", false);
                    m_CameraController.ChangeState("Pseudo3D", true);
                    m_Crosshairs.SetActive(false);
                    m_Pseudo3DObjects.SetActive(true);
                    m_WeaponWheel = m_CombatWeaponWheel;
                    break;
                case Genre.TopDown:
                    m_CharacterController.Movement = RigidbodyCharacterController.MovementType.TopDown;
                    m_CameraController.ChangeState("Pseudo3D", false);
                    m_CameraController.ChangeState("TopDown", true);
                    m_Crosshairs.SetActive(false);
                    m_Pseudo3DObjects.SetActive(false);
                    break;
                case Genre.PointClick:
                    m_CharacterController.Movement = RigidbodyCharacterController.MovementType.PointClick;
                    m_Crosshairs.SetActive(false);
                    break;
            }
            m_WeaponWheel.SetActive(true);
            for (int i = 0; i < m_ItemPickups.childCount; ++i) {
                m_ItemPickups.GetChild(i).gameObject.SetActive(true);
            }

            // Start fresh.
            if (m_HasLoaded[(int)m_CurrentGenre]) {
                m_CharacterInventory.RemoveAllItems(false);
                m_CharacterInventory.LoadDefaultLoadout();
            }
            m_CharacterAnimatorMonitor.PlayDefaultStates();
            Respawn();

            // Update Blitz.
            var enableBlitz = m_CurrentGenre == Genre.Shooter || m_CurrentGenre == Genre.Adventure || m_CurrentGenre == Genre.Platformer || m_CurrentGenre == Genre.RPG;
            if (enableBlitz) {
                m_BlitzCharacterController.TryStopAllAbilities();
                m_BlitzCharacterController.SetPosition(m_BlitzPosition);
                m_BlitzCharacterController.SetRotation(m_BlitzRotation);
            }
            m_Blitz.SetActive(enableBlitz);
        }

        /// <summary>
        /// Use a demo SpawnSelection component to respawn.
        /// </summary>
        private void Respawn()
        {
            var spawnLocation = CleanSpawnSelection.GetSpawnLocation();
            m_CharacterController.SetPosition(spawnLocation.position);
            m_CharacterController.SetRotation(spawnLocation.rotation);
            if (m_CurrentGenre == Genre.Shooter || m_CurrentGenre == Genre.Adventure || m_CurrentGenre == Genre.Platformer || m_CurrentGenre == Genre.RPG) {
                m_CameraController.ImmediatePosition();
            } else if (m_CurrentGenre == Genre.TopDown || m_CurrentGenre == Genre.PointClick) {
                m_CameraController.ImmediatePosition(Quaternion.Euler(53.2f, 0, 0));
            } else {
                m_CameraController.ImmediatePosition(Quaternion.Euler(26.56505f, 0, 0));
            }
        }

        /// <summary>
        /// The barrier should be active whenever Blitz is in control.
        /// </summary>
        private void OnBlitzAllowGameplayInput(bool allow)
        {
            m_PoolBarrier.SetActive(allow);
            m_HorseNotice.SetActive(allow);
        }
    }
}