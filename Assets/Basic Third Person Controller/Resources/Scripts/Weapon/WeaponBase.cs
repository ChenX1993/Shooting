using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[ExecuteInEditMode]
public class WeaponBase : MonoBehaviour {

    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsGroup : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field)]
    public class AdvancedSetting : Attribute
    { }

    private GameObject mesh;
    private Rigidbody rb;
    private BoxCollider bc;
    private Transform barrel;

    [HideInInspector]
    public bool usingLeftHand;

    [HideInInspector]
    public bool canShoot;

    [HideInInspector]
    public PlayerBehaviour pB;

    [HideInInspector]
    public Transform leftHand, rightHand;
    [HideInInspector]
    public Mesh leftHandMesh, rightHandMesh;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public bool shootProgress;

    [HideInInspector]
    public bool isColliding;

    [HideInInspector]
    public bool reloadProgress;
    [Header("Settings")]   
    public string weapon;
    [Range(1, 16)]
    public int raysPerShot = 1;
    public ShootingMode shootingMode = ShootingMode.Raycast;
    public GameObject projectile;
    public int maxInClipBullets;
    public int reloadBullets;
    public float fireRate;
    public float recoil;
    public float reloadTime;
    public int currentAmmo;
    [Header("Positioning")]
    public bool previewHands = false;
    public UseLeftHand useLeftHand = UseLeftHand.Yes;
    public bool updatePreset = false;
    public PositioningPresets PositioningPreset;

    private Vector3 inventoryPos = new Vector3(0, -.5f, .2f);
    private Quaternion inventoryRot = new Quaternion(.5f, -.35f, 0, 1);

    [Serializable]
    public struct CurrentPositioning
    {
        public Vector3 defaultPosition;
        public Quaternion defaultRotation;

        public Vector3 aimingPosition;
        public Quaternion aimingRotation;


        public static CurrentPositioning defaultSettings
        {
            get
            {
                return new CurrentPositioning
                {
                   
                    defaultPosition = Vector3.zero,
                    defaultRotation = Quaternion.identity,

                    aimingPosition = Vector3.zero,
                    aimingRotation = Quaternion.identity,
                };
            }
        }
    }

    [SettingsGroup]
    public CurrentPositioning currentPositioning = CurrentPositioning.defaultSettings;

    [HideInInspector]
    public Vector3[] startPos = new Vector3[2];
    [HideInInspector]
    public Quaternion[] startRot = new Quaternion[2];

    public enum PositioningPresets
    {
        PistolPreset,
        RiflePreset,
        RPGPreset,
        Custom
    }
    //Pistol Preset
    private Vector3 defaultPositionPistol = new Vector3(0, -.7f, .2f);
    private Quaternion defaultRotationPistol = new Quaternion(.45f, -.47f, .25f, 1);

    private Vector3 aimingPositionPistol = new Vector3(.2f, -.15f, .55f);
    private Quaternion aimingRotationPistol = new Quaternion(0, 0, 0, 1);

    //Rifle Preset
    private Vector3 defaultPositionRifle = new Vector3(0, -.45f, .2f);
    private Quaternion defaultRotationRifle = new Quaternion(.35f, -.7f, .25f, 1);

    private Vector3 aimingPositionRifle = new Vector3(.2f, -.15f, .35f);
    private Quaternion aimingRotationRifle = new Quaternion(0, 0, 0, 1);

    //RPG Preset
    private Vector3 defaultPositionRPG = new Vector3(.1f, -.05f, .25f);
    private Quaternion defaultRotationRPG = new Quaternion(0, 0, 0, 1);

    private Vector3 aimingPositionRPG = new Vector3(.1f, -.05f, .50f);
    private Quaternion aimingRotationRPG = new Quaternion(0, 0, 0, 1);

    
    public enum ShootingMode
    {
        Raycast,
        Projectile
    }
    public enum UseLeftHand
    {
        No,
        Yes
    }
    [HideInInspector]
    public float currentRecoil;
    private AudioSource audioS;
    [Header("Audio")]  
    public AudioClip pickUpAudio;
    public AudioClip shotAudio;
    public AudioClip noAmmoAudio;
    public AudioClip reloadAudio;
    public AudioClip aimAudio;
    public AudioClip switchAudio;
    [Header("Extras")]
    public Sprite icon;
    public Sprite centerCross;

    void Start()
    {
        
        if (!Application.isPlaying) { return; }

        if (useLeftHand == UseLeftHand.Yes)
        {
            usingLeftHand = true;
        }
        animator = transform.GetChild(0).GetComponent<Animator>();
        leftHand = animator.transform.Find("LeftHand");
        rightHand = animator.transform.Find("RightHand");

        currentAmmo = maxInClipBullets;
        mesh = animator.transform.Find("Mesh").gameObject;
        barrel = animator.transform.Find("Barrel");
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        audioS = GetComponent<AudioSource>();

        startPos[0] = leftHand.localPosition;
        startPos[1] = rightHand.localPosition;

        startRot[0] = leftHand.localRotation;
        startRot[1] = rightHand.localRotation;



    }
    void SetDefaultPositioning(Vector3 defaultPos, Quaternion defaultRot, Vector3 aimPos, Quaternion aimRot)
    {

        currentPositioning.defaultPosition = defaultPos;
        currentPositioning.defaultRotation = defaultRot;

        currentPositioning.aimingPosition = aimPos;
        currentPositioning.aimingRotation = aimRot;
    }
    void OnDrawGizmos()
    {
        if (!previewHands) { return; }
        if (!animator)
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
        }
        else
        {
            if (!leftHand)
            {
                leftHand = animator.transform.Find("LeftHand");
            }
            if (!rightHand)
            {
                rightHand = animator.transform.Find("RightHand");
            }
        }

        if (leftHand && rightHand)
        {
            if (!leftHandMesh)
            {
                GameObject _lH = Resources.Load("Editor/Mesh/LeftHand") as GameObject;
                leftHandMesh = _lH.GetComponent<MeshFilter>().sharedMesh;
            }
            if (!rightHandMesh)
            {
                GameObject _rh = Resources.Load("Editor/Mesh/RightHand") as GameObject;
                rightHandMesh = _rh.GetComponent<MeshFilter>().sharedMesh;
            }
            if (leftHandMesh && rightHandMesh)
            {
                Gizmos.DrawMesh(leftHandMesh, leftHand.position, leftHand.rotation);
                Gizmos.DrawMesh(rightHandMesh, rightHand.position, rightHand.rotation);
            }
        }
        
    }
    void Update()
    {
        if (updatePreset)
        {
            if (PositioningPreset == PositioningPresets.PistolPreset)
            {
                SetDefaultPositioning(defaultPositionPistol, defaultRotationPistol, aimingPositionPistol, aimingRotationPistol);
            }
            else if (PositioningPreset == PositioningPresets.RiflePreset)
            {
                SetDefaultPositioning(defaultPositionRifle, defaultRotationRifle, aimingPositionRifle, aimingRotationRifle);
            }
            else if (PositioningPreset == PositioningPresets.RPGPreset)
            {
                SetDefaultPositioning(defaultPositionRPG, defaultRotationRPG, aimingPositionRPG, aimingRotationRPG);
            }
            updatePreset = false;
            print("Preset Updated");
        }
    }
    void FixedUpdate()
    {
       
        if (!Application.isPlaying) { return; }

        if (raysPerShot == 1)
        {
            currentRecoil = Mathf.Lerp(currentRecoil, 0, .02f);
        }
        else
        {
            currentRecoil = Mathf.Lerp(currentRecoil, .025f, .03f);
        }

        if (pB)
        {
            if (pB.currentWeapon == this)
            {
                if (useLeftHand == WeaponBase.UseLeftHand.No)
                {
                    usingLeftHand = (pB.aim) ? true : false;
                }
                isColliding = Physics.Linecast(transform.position - transform.forward * bc.size.z * 2, transform.position + transform.forward * bc.size.z);
            }
        }
    }
    public void Shoot()
    {
        if (isColliding) { return; }
        if (currentAmmo > 0 && !reloadProgress && !shootProgress)
        {
            StartCoroutine(ShootProgress());
            return;
        }
        if (currentAmmo == 0 && !reloadProgress && !shootProgress)
        {
            if (!audioS.isPlaying)
            {
                audioS.PlayOneShot(noAmmoAudio);
            }
            Reload();
        }
    }
    IEnumerator ShootProgress()
    {

        shootProgress = true;
        if (!pB.aim)
        {
            yield return new WaitForSeconds(.25f);
        }
        if (!pB.aim)
        {
            shootProgress = false;
            StopCoroutine(ShootProgress());
        }
        else
        {
            animator.Rebind();
            animator.Play("Shoot");

            pB.recoil = UnityEngine.Random.Range(recoil, recoil * 2);

            audioS.PlayOneShot(shotAudio);
            currentAmmo--;

            if (shootingMode == ShootingMode.Projectile)
            {
                ProjectileShoot();
            }
            else
            {
                RaycastShoot();
            }
            if (currentRecoil < recoil) {
                currentRecoil += 0.02f; 
            }
            yield return new WaitForSeconds(fireRate);

            shootProgress = false;

            leftHand.localPosition = startPos[0];
            leftHand.localRotation = startRot[0];

            rightHand.localPosition = startPos[1];
            rightHand.localRotation = startRot[1];
        }
    }
    void RaycastShoot()
    {
        for (int i = 0; i < raysPerShot; i++)
        {
            Vector3 _recoil = UnityEngine.Random.insideUnitSphere * currentRecoil;
            
            if(i > 0)
            {
                _recoil *= i;
            }

            RaycastHit hit, centerHit;

            bool centerHitted = Physics.Raycast(pB.cam.position, pB.cam.forward + _recoil, out centerHit);

            if (centerHitted)
            {
                Physics.Linecast(barrel.position, centerHit.point + pB.cam.forward, out hit);

                if (hit.transform == null) { return; }

                HandleHit(hit);
                ShotVisuals(hit);
            }
        }
    }
    void HandleHit(RaycastHit h)
    {
        //your code
    }
    void ShotVisuals(RaycastHit h)
    {
        string tag = h.transform.tag;

        if (tag != "" && tag != "Weapon" && tag != "Player")
        {
            int i = HUDBehaviour.instance.nextBulletHole;
            GameObject _hole = HUDBehaviour.instance.bulletHoles[i];

            _hole.transform.position = h.point;
            _hole.transform.rotation = Quaternion.identity;
            _hole.transform.rotation = Quaternion.FromToRotation(-_hole.transform.forward, h.normal);
            _hole.SetActive(true);

            HUDBehaviour.instance.nextBulletHole++;
            if (HUDBehaviour.instance.nextBulletHole > HUDBehaviour.instance.bulletHoles.Count - 1)
            {
                HUDBehaviour.instance.nextBulletHole = 0;
            }

        }
    }
    void ProjectileShoot()
    {
        RaycastHit hit;
        Physics.Raycast(pB.cam.position, pB.cam.forward, out hit);

        GameObject _projectile = Instantiate(projectile, barrel.position, barrel.rotation) as GameObject;

        if (hit.transform == null) { return; }

        if (pB.aim)
        {
            _projectile.transform.LookAt(hit.point);
        }
    }
    public void Reload()
    {
        if (currentAmmo < maxInClipBullets && reloadBullets > 0 && !reloadProgress)
        {
            StartCoroutine(ReloadProgress());
        }
    }
    IEnumerator ReloadProgress()
    {
        int toRefill = maxInClipBullets - currentAmmo;

        shootProgress = false;
        reloadProgress = true;
        animator.Play("Reload");
        audioS.PlayOneShot(reloadAudio);

        yield return new WaitForSeconds(reloadTime);

        if (toRefill <= reloadBullets)
        {
            reloadBullets -= toRefill;
            currentAmmo += toRefill;
        }
        else{
            currentAmmo += reloadBullets;
            reloadBullets = 0;
        }

        reloadProgress = false;
        leftHand.localPosition = startPos[0];
        leftHand.localRotation = startRot[0];

        rightHand.localPosition = startPos[1];
        rightHand.localRotation = startRot[1];
    }
    public void AimAudio()
    {
        audioS.PlayOneShot(aimAudio);
    }
    public void PutInInventory()
    {
        if (audioS)
        {
            audioS.PlayOneShot(pickUpAudio);
        }
        Destroy(rb);
        bc.enabled = false;
        transform.localPosition = inventoryPos;
        transform.localRotation = inventoryRot;
    }
    public void RemoveFromInventory()
    {
        gameObject.AddComponent<Rigidbody>();
        bc.enabled = true;

    }
    public void ToggleRenderer(bool value)
    {
        mesh.SetActive(value);
        if (value)
        {
            audioS.PlayOneShot(switchAudio);
        }
        if(value == false)
        {
            transform.localPosition = inventoryPos;
            transform.localRotation = inventoryRot;
        }
    }
    public void MoveTo(Transform reference)
    {
        Vector3 _offset = currentPositioning.defaultPosition;
        Quaternion _toRot = Quaternion.identity;

        if (pB.aim || reloadProgress)
        {
            if (reloadProgress)
            {
                usingLeftHand = true;
            }
            if (pB.halfSwitchingWeapons)
            {
                _offset = currentPositioning.aimingPosition;
                _toRot = currentPositioning.aimingRotation;
            }
        }
        else
        {
            if (!pB.crouch)
            {
                if (pB.grounded)
                {
                    _offset = currentPositioning.defaultPosition;
                }
                else
                {
                    _offset = currentPositioning.defaultPosition;
                    _offset.y += 0.3f;
                    _offset.z += 0.1f;
                }
            }
            else
            {
                _offset.z -= 0.1f;
            }
            _toRot = currentPositioning.defaultRotation;

        }
        if (pB.switchingWeapons && !pB.halfSwitchingWeapons)
        {
            _offset = inventoryPos;
            _toRot = inventoryRot;
        }

        _offset.z += pB.bellyOffset;
        transform.localPosition = Vector3.Slerp(transform.localPosition, _offset, 6 * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _toRot, 8 * Time.deltaTime);

    }
}

