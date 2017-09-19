using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TransformPathMaker))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CapsuleCollider))]

[ExecuteInEditMode]
public class PlayerBehaviour : MonoBehaviour
{

    [HideInInspector]
    public Transform aimHelper, aimHelperSpine;

    public enum Direction
    {
        Forward,
        Back,
        Up,
        Down,
        Left,
        Right
    }

    [HideInInspector]
    public Animator playerAnimator;

    [HideInInspector]
    public RagdollHelper ragdollh;

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public CapsuleCollider capsuleC;

    [HideInInspector]
    public IKControl ikControll;

    [HideInInspector]
    public TransformPathMaker pathMaker;

    [HideInInspector]
    public Controller controller;

    [HideInInspector]
    public CameraBehaviour cameraBehaviour;

    [HideInInspector]
    public Transform transformToRotate;

    [HideInInspector]
    public Vector3 moveAxis;

    [HideInInspector]
    public Rigidbody[] boneRb;

    [HideInInspector]
    public Transform hipsParent;

    private PhysicMaterial pM;

    //Camera
    [HideInInspector]
    public Transform cameraParent, cam;

    [HideInInspector]
    public Transform[] camPivot = new Transform[2];


    [Header("Player Settings")]
    [Range(0, 100)]
    public float life = 100;
    [SerializeField]
    public float crouchSpeed = 1f, walkSpeed = 2.3f, runSpeed = 4.6f, aimWalkSpeed = 1.5f;
    private bool dead;
    [HideInInspector]
    public bool crouch;
    [HideInInspector]
    public bool aim;
    public float jumpForce = 7;
    public int bagLimit = 5;
    public float switchWeaponTime = .5f;
    public bool ragdollWhenFall = true;
    private float characterHeight = 1;
    [Range(0, 2)]
    public float crouchHeight = 0.75f;
    [Range(-1, 1)]
    public float bellyOffset = 0;

    [Header("Change this if holding weapons looks weird")]
    public Direction spineFacingDirection;

    [Header("Audio")]
    public AudioClip footStepAudio;

    [HideInInspector]
    public AudioSource audioSource;

    //FOOTSTEP STUFF
    [HideInInspector]
    public Transform leftFoot, rightFoot;
    private bool leftCanStep, rightCanStep;
    [HideInInspector]
    public float factor;

    //WEAPON STUFF
    public event Action OnWeaponSwitch;
    private bool equippedbefore;
    private float climbY;
    private float xAxis, yAxis;
    [HideInInspector]
    public Quaternion rotationAux, aimRotationSpineAux, aimRotationAux;

    private float lean;
    [HideInInspector]
    public float recoil;
    private float _capsuleSize;
    private float currentMovementState;
    private float runKeyPressed;
    private AnimatorStateInfo currentAnimatorState;
    [HideInInspector]
    public bool grounded, inMoveState, climbing, climbHit, switchingWeapons, halfSwitchingWeapons;
    private GameObject collidedWith;

    [HideInInspector]
    public List<WeaponBase> weapons = new List<WeaponBase>();

    [HideInInspector]
    public WeaponBase currentWeapon;

    [HideInInspector]
    public int currentWeaponID;

    [HideInInspector]
    public bool equippedWeapon;

    [HideInInspector]
    public Transform leftHandInWeapon, rightHandInWeapon;

    Quaternion startSpineRot = new Quaternion(0, 0, 0, 1);

    void Start()
    {
        if (Application.isPlaying)
        {
            factor = 0.45f;
            startSpineRot = aimHelperSpine.rotation;
            Cursor.lockState = CursorLockMode.Locked;
            rotationAux = new Quaternion(0, 0, 0, 1);
            aimRotationAux = rotationAux;
            aimRotationSpineAux = rotationAux;
            life = 100;
            dead = false;
            crouch = false;
            halfSwitchingWeapons = true;

            foreach (Rigidbody r in boneRb)
            {
                BoxCollider bc = r.GetComponent<BoxCollider>();
                SphereCollider[] sc = r.GetComponents<SphereCollider>();
                if (bc != null)
                {
                    Physics.IgnoreCollision(capsuleC, bc);
                }
                if (sc != null)
                {
                    foreach (SphereCollider s in sc)
                    {
                        Physics.IgnoreCollision(capsuleC, s);
                    }
                }
            }
            pM = capsuleC.material;
        }
    }
    void FootStepAudio()
    {
        if(!grounded) { return; }
        if (climbing) { return; }
        float dist = Vector3.Distance(leftFoot.position, rightFoot.position);
        if(dist > factor)
        {
            leftCanStep = true;
        }
        if (leftCanStep && dist < factor)
        {
            leftCanStep = false;
            audioSource.PlayOneShot(footStepAudio);
        }

    }

    void Update()
    {
        if (!Application.isPlaying) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        PlayerMovement();
        GroundCheck();
        Gravity();
        FootStepAudio();
    }

    //Body Aim
    void LateUpdate()
    {
        if (!Application.isPlaying) { return; }

        recoil = Mathf.Lerp(recoil, 0, 10 * Time.deltaTime);
        if (dead) { return; }

       
        Vector3 spineOffset = cam.forward - cam.up / 5;

        Vector3 armsOffset = cam.forward;
        
        if (aim)
        {
            if (!cameraBehaviour.aimIsRightSide)
            {
                armsOffset -= cam.right / 3 ;
            }
            spineOffset.y = Mathf.Clamp(spineOffset.y, -0.4f, 0.35f);

            if (SomethingInFront())
            {
                spineOffset.y = Mathf.Clamp(spineOffset.y, 0, 0f);
                armsOffset.y = Mathf.Clamp(armsOffset.y, 0, 1);
            }
            if (SomethingInFrontAim(2) && currentWeapon && !currentWeapon.reloadProgress)
            {
                if (cameraBehaviour.aimIsRightSide)
                {
                    lean = -.2f;
                }
                else
                {
                    lean = .2f;
                }
            }
            else
            {
                lean = 0;
            }
            if (crouch)
            {
                armsOffset.y = Mathf.Clamp(armsOffset.y, -.7f, 0.4f);
                if (!cameraBehaviour.aimIsRightSide)
                {
                    spineOffset -= cam.right * .3f;
                }
                else
                {
                    spineOffset += cam.right * .3f;
                }
                if (!SomethingInFront())
                {
                    spineOffset.y = Mathf.Clamp(spineOffset.y, -.5f, -.5f);
                }
            }
            
            aimRotationAux = Quaternion.Lerp(aimRotationAux, Quaternion.LookRotation((transformToRotate.position + armsOffset + cam.up * recoil / 10) - transformToRotate.position), 10 * Time.deltaTime);
            aimRotationSpineAux = Quaternion.Lerp(aimRotationSpineAux, Quaternion.LookRotation((transformToRotate.position + spineOffset + cam.up * recoil / 5) - transformToRotate.position) * new Quaternion(0, 0, lean, 1) * startSpineRot, 10 * Time.deltaTime);
        }
        else
        {
            lean = 0;
            
            aimRotationSpineAux = Quaternion.Lerp(aimRotationSpineAux, aimHelperSpine.rotation, 20 * Time.deltaTime);

            Vector3 _off = Vector3.zero;

            if (spineFacingDirection == Direction.Forward)
            {
                _off = aimHelperSpine.forward;

            }else if(spineFacingDirection == Direction.Back)
            {
                _off = -aimHelperSpine.forward;
            }
            else if (spineFacingDirection == Direction.Up)
            {
                _off = aimHelperSpine.up;
            }
            else if (spineFacingDirection == Direction.Down)
            {
                _off = -aimHelperSpine.up;
            }
            else if (spineFacingDirection == Direction.Left)
            {
                _off = -aimHelperSpine.right;
            }
            else if (spineFacingDirection == Direction.Right)
            {
                _off = aimHelperSpine.right;
            }
            _off.y = Mathf.Clamp(_off.y, 0, 5);
            if (crouch)
            {
                _off -= transformToRotate.right * 0.3f;
            }
            aimRotationAux = Quaternion.Lerp(aimRotationAux, Quaternion.LookRotation((aimHelper.position + _off) - aimHelper.position), 10 * Time.deltaTime);

        }
        if (!playerAnimator.enabled) { return; }

        aimHelperSpine.rotation = aimRotationSpineAux;
    }
    void PlayerMovement()
    {

        if (dead) { return; }

        AnimatorMovementState();
        Climb();
        RagdollWhenFall();
        StandUp();

        xAxis = controller.xAxis;
        yAxis = controller.yAxis;

        if (ragdollh.state == RagdollHelper.RagdollState.blendToAnim)
        {
            transformToRotate.localPosition = Vector3.Lerp(transformToRotate.localPosition, Vector3.zero, 20 * Time.deltaTime);
        }

        if (equippedWeapon && !climbing && !switchingWeapons)
        {
            if (Input.GetKey(controller.ShootKey))
            {
                currentWeapon.Shoot();
            }
        }
        if (inMoveState && !climbing)
        {
            Vector3 orientedX = xAxis * cam.right;
            Vector3 orientedY = yAxis * cam.forward;

            orientedX.y = 0;
            orientedY.y = 0;

            moveAxis = orientedY + orientedX;
            Vector3 lookForward = cam.forward;

            if (aim)
            {
                if (!cameraBehaviour.aimIsRightSide && crouch)
                {
                     lookForward -= cam.right / 2;
                }
                lookForward.y = 0;
                rotationAux = Quaternion.LookRotation((transformToRotate.position + lookForward) - transformToRotate.position);
            }
            transformToRotate.rotation = Quaternion.Lerp(playerAnimator.transform.rotation, rotationAux, 10 * Time.deltaTime);

            if (moveAxis != Vector3.zero)
            {
                if (!aim)
                {
                    rotationAux = Quaternion.LookRotation((transformToRotate.position + moveAxis) - transformToRotate.position);
                }
            }

            //SPEED CHANGE
            float _speed = 0;

            if (currentMovementState < 0.5f)
            {
                _speed = crouchSpeed;

            }
            else if (currentMovementState < 1.5f)
            {
                if (runKeyPressed > 1.5f && !crouch && !aim)
                {
                    _speed = runSpeed;

                }
                else
                {
                    _speed = walkSpeed;
                }

            }
            else if (currentMovementState < 2.5f)
            {
                if (runKeyPressed > 1.5f)
                {
                    _speed = aimWalkSpeed;
                }
                else
                {
                    _speed = aimWalkSpeed;
                }
            }

            if (!SomethingInFront())
            {
                if (Input.GetKey(controller.RunKey) && moveAxis != Vector3.zero) { LerpSpeed(2); } else { LerpSpeed(1); }


                if (grounded)
                {
                    Vector3 moveSpeed;
                    moveSpeed = moveAxis.normalized * _speed;
                    rb.velocity = new Vector3(moveSpeed.x, rb.velocity.y, moveSpeed.z);
                }

            }
            else
            {
                if (aim)
                {
                    if (grounded)
                    {
                        Vector3 moveSpeed;
                        moveSpeed = moveAxis.normalized * _speed;
                        rb.velocity = new Vector3(moveSpeed.x, rb.velocity.y, moveSpeed.z);
                    }

                }
            }
            Jump();
            Crouch();
            SwitchWeapon();
            PickUpWeapon();

            if (!switchingWeapons)
            {
                if ((Input.GetKeyDown(controller.EquipWeaponKey)) && weapons.Count > 0)
                {
                    EquipWeaponToggle();
                }
                if (equippedWeapon)
                {
                    if (Input.GetKeyDown(controller.ReloadKey))
                    {
                        currentWeapon.Reload();
                    }
                }
            }
            Aim();

        }
    }

    void EquipWeaponToggle()
    {
        equippedWeapon = !equippedWeapon;

        if (weapons.Count > 0)
        {
            if (equippedWeapon)
            {
                currentWeapon = GetCurrentWeapon();
                leftHandInWeapon = currentWeapon.leftHand;
                rightHandInWeapon = currentWeapon.rightHand;
            }
            currentWeapon.ToggleRenderer(equippedWeapon);
        }
        if (OnWeaponSwitch != null)
        {
            OnWeaponSwitch();
        }
    }

    void SwitchWeapon()
    {
        int numberPressed = 0;
        bool pressed = false;

        for (int i = 0; i < controller.keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(controller.keyCodes[i]))
            {
                numberPressed = i;
                pressed = true;
            }

        }
        if (pressed)
        {
            if (numberPressed < weapons.Count && !GetCurrentWeapon().reloadProgress) {

                if (numberPressed != currentWeaponID)
                {
                    if (!switchingWeapons)
                    {
                        StartCoroutine(WeaponSwitchProgress(numberPressed));
                    }
                }

                if (!equippedWeapon)
                {
                    EquipWeaponToggle();
                }
            }
        }
    }

    IEnumerator WeaponSwitchProgress(int numberP)
    {
        switchingWeapons = true;
        halfSwitchingWeapons = false;
        yield return new WaitForSeconds(switchWeaponTime/2);
        halfSwitchingWeapons = true;
        if (currentWeapon)
        {
            currentWeapon.ToggleRenderer(false);
        }
        currentWeaponID = numberP;
        currentWeapon = GetCurrentWeapon();
        OnWeaponSwitch();
        if (equippedWeapon)
        {
            currentWeapon.ToggleRenderer(true);

            leftHandInWeapon = currentWeapon.leftHand;
            rightHandInWeapon = currentWeapon.rightHand;
        }
        yield return new WaitForSeconds(switchWeaponTime);
        switchingWeapons = false;
    }

    void PickUpWeapon()
    {
        if (Input.GetKeyDown(controller.PickUpWeaponKey) && weapons.Count < bagLimit || controller.automaticPickUp && weapons.Count < bagLimit)
        {
            if (collidedWith != null)
            {
                if (collidedWith.tag == "Weapon")
                {
                    WeaponBase _c = collidedWith.GetComponent<WeaponBase>();
                    bool alreadyHave = false;
                    int id = 0;
                    for(int i = 0; i < weapons.Count; i++)
                    {
                        if(weapons[i].weapon == _c.weapon)
                        {
                            alreadyHave = true;
                            id = i;
                        }
                    }
                    if (alreadyHave)
                    {
                        weapons[id].reloadBullets += _c.reloadBullets + _c.currentAmmo;
                        Destroy(_c.gameObject); 
                    }
                    else
                    {
                        _c.transform.parent = aimHelper;
                        _c.pB = this;
                        _c.PutInInventory();
                        _c.ToggleRenderer(false);
                        weapons.Add(_c);
                    }
                    collidedWith = null;
                }
            }
        }
    }

    void OnCollisionStay(Collision col)
    {
        collidedWith = col.gameObject;
    }

    void OnCollisionExit()
    {
        collidedWith = null;
    }

    void AnimatorMovementState()
    {
        currentAnimatorState = playerAnimator.GetCurrentAnimatorStateInfo(0);
        inMoveState = currentAnimatorState.IsName("Grounded");
        playerAnimator.SetBool("Grounded", grounded);
        playerAnimator.SetFloat("Speed", runKeyPressed);
        playerAnimator.SetBool("HoldingWeapon", equippedWeapon);
        currentMovementState = playerAnimator.GetFloat("State");

        if (crouch)
        {
            playerAnimator.SetFloat("State", Mathf.Lerp(currentMovementState, 0, 5 * Time.deltaTime));
        }
        else if (aim)
        {
            playerAnimator.SetFloat("State", Mathf.Lerp(currentMovementState, 2, 5 * Time.deltaTime));
        }
        else
        {
            playerAnimator.SetFloat("State", Mathf.Lerp(currentMovementState, 1, 5 * Time.deltaTime));
        }
        if (!SomethingInFront())
        {
            float _m = Mathf.Clamp01(Mathf.Abs(xAxis) + Mathf.Abs(yAxis));
            playerAnimator.SetFloat("Move", Mathf.Lerp(playerAnimator.GetFloat("Move"), _m * runKeyPressed, 10 * Time.deltaTime));
            playerAnimator.SetFloat("AxisX", Mathf.Lerp(playerAnimator.GetFloat("AxisX"), xAxis, 10 * Time.deltaTime));
            playerAnimator.SetFloat("AxisY", Mathf.Lerp(playerAnimator.GetFloat("AxisY"), yAxis, 10 * Time.deltaTime));
        }
        else
        {
            if (aim)
            {
                float _yAxis = yAxis;
                _yAxis = Mathf.Clamp(_yAxis, -1, 0);

                float _m = Mathf.Clamp01(Mathf.Abs(xAxis) + Mathf.Abs(_yAxis));

                playerAnimator.SetFloat("Move", Mathf.Lerp(playerAnimator.GetFloat("Move"), _m * runKeyPressed, 10 * Time.deltaTime));
                playerAnimator.SetFloat("AxisX", Mathf.Lerp(playerAnimator.GetFloat("AxisX"), xAxis, 10 * Time.deltaTime));
                playerAnimator.SetFloat("AxisY", Mathf.Lerp(playerAnimator.GetFloat("AxisY"), _yAxis, 10 * Time.deltaTime));
            }
            else
            {
                playerAnimator.SetFloat("Move", Mathf.Lerp(playerAnimator.GetFloat("Move"), 0, 5 * Time.deltaTime));
                playerAnimator.SetFloat("AxisX", Mathf.Lerp(playerAnimator.GetFloat("AxisX"), 0, 10 * Time.deltaTime));
                playerAnimator.SetFloat("AxisY", Mathf.Lerp(playerAnimator.GetFloat("AxisY"), 0, 10 * Time.deltaTime));
            }
        }
    }

    public bool SomethingInFront()
    {
        Vector3 posToDetect = transformToRotate.position + transformToRotate.up * .5f;
        return Physics.Raycast(posToDetect, transformToRotate.forward, 0.5f);
    }

    public bool SomethingInFrontAim(float distance)
    {
        Vector3 camF = cam.forward;
        camF.y = 0;
        Vector3 offset = -cam.right * 0.15f;
        if (!cameraBehaviour.aimIsRightSide)
        {
            offset = cam.right * 0.15f;
        }
        Vector3 posToDetect = transformToRotate.position + transformToRotate.up * .5f;
        return Physics.Raycast(posToDetect, camF + offset, distance) && !Physics.Raycast(posToDetect + (offset * -5), camF + (offset * -5), distance);
    }

    public void Aim()
    {
        if (!climbing && grounded && !ragdollh.ragdolled)
        {

            if (equippedWeapon)
            {
                WeaponBase _currentW = currentWeapon;

                
                if (controller.aimMode == Controller.AimMode.Hold)
                {
                    aim = Input.GetKey(controller.AimKey) || Input.GetKey(controller.ShootKey);
                }
                else
                {
                    if (!_currentW.reloadProgress && !_currentW.shootProgress)
                    {
                        if (Input.GetKeyDown(controller.AimKey))
                        {
                            aim = !aim;
                        }
                    }
                    else
                    {
                        aim = true;
                    }
                }
                if(Input.GetKeyDown(controller.AimKey))
                {
                    _currentW.AimAudio();
                }
                if (Input.GetKeyUp(controller.AimKey))
                {
                    _currentW.AimAudio();
                }

            }
            else
            {
                if (controller.aimMode == Controller.AimMode.Hold)
                {
                    aim = Input.GetKey(controller.AimKey);
                }
                else
                {
                    if (Input.GetKeyDown(controller.AimKey))
                    {
                        aim = !aim;
                    }
                }
            }
        }

        if (equippedWeapon)
        {
            if (!aim)
            {
                currentWeapon.MoveTo(transformToRotate);
            }
            else
            {
                currentWeapon.MoveTo(aimHelper);
                
            }
            aimHelper.rotation = aimRotationAux;
        }
    }

    public void Crouch()
    {
        if (controller.canCrouch)
        {
            bool somethingAbove = Physics.Raycast(transform.position + transform.up * .5f, transform.up, 1.4f);
            if (Input.GetKeyDown(controller.CrouchKey))
            {
                if (crouch && !somethingAbove)
                {
                    crouch = false;
                }
                else
                {
                    crouch = true;
                }
            }
            if (Input.GetKeyDown(controller.JumpKey))
            {
                if (crouch && !somethingAbove)
                {
                    crouch = false;
                }
            }
            if (somethingAbove)
            {
                crouch = true;
            }
            if (crouch)
            {
                _capsuleSize = Mathf.Lerp(_capsuleSize, crouchHeight, 5 * Time.deltaTime);
            }
            else
            {
                _capsuleSize = Mathf.Lerp(_capsuleSize, characterHeight, 5 * Time.deltaTime);
            }
            capsuleC.center = new Vector3(0, .9f * _capsuleSize, 0);
            capsuleC.height = 1.8f * _capsuleSize;
        }
        
    }

    public void Jump()
    {
        bool canJumpBasedOnWeapon = true;

        if (currentWeapon != null){

            if (currentWeapon.reloadProgress)
            {
                canJumpBasedOnWeapon = false;
            }
        }

        if (grounded && inMoveState && !climbing && !crouch && !climbHit && !aim && !ragdollh.ragdolled && canJumpBasedOnWeapon)
        {
           
            if (Input.GetKeyDown(controller.JumpKey))
            {
                playerAnimator.SetTrigger("Jump Forward");
                if (moveAxis != Vector3.zero && !SomethingInFront())
                {
                    rb.velocity = transformToRotate.up * jumpForce + transformToRotate.forward * 4;
                }
                else
                {
                    rb.velocity = transformToRotate.up * jumpForce / 1.1f;
                }
            }
        }
    }

    public void Climb()
    {
        bool canClimbBasedOnWeapon = true;

        if (currentWeapon != null)
        {

            if (currentWeapon.reloadProgress)
            {
                canClimbBasedOnWeapon = false;
            }
        }


        RaycastHit hit;

        Vector3 climbRayPos = transform.position + transformToRotate.forward * 0.45f + transformToRotate.up * 2.1f * characterHeight;

        if (Physics.Raycast(climbRayPos, -transform.up, out hit, 1.8f) && !ragdollh.ragdolled && canClimbBasedOnWeapon)
        {
            climbHit = true;

            climbY = hit.point.y;
            float dist = climbY - transform.position.y;

            if (hit.collider.tag == "Climbable")
            {

                if ((controller.automaticClimb || Input.GetKeyDown(controller.JumpKey)))
                {
                    
                    if (pathMaker.play == false)
                    {
                        equippedbefore = equippedWeapon;
                        if (equippedWeapon)
                        {
                            EquipWeaponToggle();
                        }
                        if (dist > 1f)
                        {
                            climbing = true;
                            aim = false;
                            playerAnimator.SetTrigger("Climb");

                            pathMaker.pointsTime[0] = Vector3.Distance(transform.position, pathMaker.points[0]);
                            pathMaker.points[0].y = climbY - 1.5f;

                            pathMaker.pointsTime[1] = 1;
                            pathMaker.points[1].y = climbY + 0.8f;
                            pathMaker.points[1].z = 1f;

                            pathMaker.pointsTime[2] = 1;
                            pathMaker.points[2].y = climbY + 1.3f;
                            pathMaker.points[2].z = 1f;
                            pathMaker.Play();
                            return;
                        }
                    }
                    
                }
            }
            if (climbing)
            {
                ikControll.LerpHandWeight(1f, 3f);
                ikControll.leftHandPos = hit.point + transformToRotate.right * -0.3f + transformToRotate.forward * -0.3f;
                ikControll.rightHandPos = hit.point + transformToRotate.right * 0.3f + transformToRotate.forward * -0.3f;
            }
        }
        else
        {
            climbHit = false;
            climbing = false;
            ikControll.LerpHandWeight(0f, 5);
            if (equippedbefore)
            {
                equippedbefore = false;
                EquipWeaponToggle();
            }
        }
    }

    public void StandUp()
    {
        if (boneRb[0].transform.parent == null && ragdollh.ragdolled)
        {
            transform.position = boneRb[0].position;
        }
        if (ragdollh.ragdolled)
        {
            RaycastHit h;
            if (Input.GetKeyDown(controller.JumpKey) && Physics.SphereCast(transform.position + transform.up * 1, 0.2f, -transform.up, out h, 3f))
            {
                ToggleRagdoll();
            }
        }
    }

    public void ToggleRagdoll()
    {
        if(boneRb[0].velocity.magnitude > 1) { return; }
        bool ragdoll = !ragdollh.ragdolled;
        foreach (Rigidbody r in boneRb)
        {

            if (ragdoll == false)
            {
                capsuleC.enabled = true;
                ragdollh.ragdolled = false;
                r.isKinematic = true;
                r.velocity = Vector3.zero;

                boneRb[0].transform.parent = hipsParent;

                cameraParent.parent = transform;
            }
            else
            {
                if (equippedWeapon)
                {
                    EquipWeaponToggle();
                }
                crouch = false;
                ragdollh.ragdolled = true;
                aim = false;
                pathMaker.Reset();
                rb.useGravity = false;

                r.isKinematic = false;
                r.velocity = rb.velocity * 1.5f;
                playerAnimator.SetFloat("Move", 0);
                playerAnimator.enabled = false;

                rb.velocity = Vector3.zero;
                rb.isKinematic = true;

                capsuleC.enabled = false;

                boneRb[0].transform.parent = null;
                cameraParent.parent = null;

            }
        }
    }

    public void RagdollWhenFall()
    {
        if (!ragdollh.ragdolled && ragdollWhenFall)
        {
            if (rb.velocity.y < -15)
            {
                ToggleRagdoll();
            }
        }
    }

    public void Damage(float amount)
    {
        if (!dead)
        {
            life -= amount;
            if (life <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        dead = true;
        life = 0;
        if (!ragdollh.ragdolled)
        {
            ToggleRagdoll();
        }
    }

    public void Revive()
    {
        if (dead)
        {
            Start();
            ToggleRagdoll();
        }
    }

    void GroundCheck()
    {
        RaycastHit hit;
        if(Physics.SphereCast(transform.position + transform.up * 2, .15f, -transform.up, out hit, 2.5f))
        {
            grounded = true;
            if (moveAxis == Vector3.zero || ragdollh.state == RagdollHelper.RagdollState.blendToAnim)
            {
                pM.staticFriction = 3;
                pM.dynamicFriction = 3;
            }
            else
            {
                pM.staticFriction = 0;
                pM.dynamicFriction = 0;
            }
        }
        else
        {
            grounded = false;
            pM.staticFriction = 0;
            pM.dynamicFriction = 0;
        }
    }

    void LerpSpeed(float final)
    {
        runKeyPressed = Mathf.Lerp(runKeyPressed, final, 10 * Time.deltaTime);
    }

    void Gravity()
    {
        if (ragdollh.state == RagdollHelper.RagdollState.animated)
        {
            Vector3 velocity = rb.velocity;
            velocity.y -= 10F * Time.deltaTime;
            rb.velocity = velocity;
        }
    }

    public WeaponBase GetCurrentWeapon()
    {
        if(weapons.Count > 0)
        {
            return weapons[currentWeaponID];
        }
        else
        {
            return null;
        }
    }
}