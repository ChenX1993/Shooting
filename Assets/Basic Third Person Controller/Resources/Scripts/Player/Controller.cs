using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

    public bool automaticPickUp = true;
    public bool automaticClimb = false;
    public bool canCrouch = true;
    public bool canSwitchAimSide = true;
    public AimMode aimMode = AimMode.Hold;

    //Keyboard
    public KeyCode JumpKey = KeyCode.Space;
    public KeyCode RunKey = KeyCode.LeftShift;
    public KeyCode CrouchKey = KeyCode.C;
    public KeyCode ShootKey = KeyCode.Mouse0;
    public KeyCode AimKey = KeyCode.Mouse1;
    public KeyCode SwitchAimSideKey = KeyCode.T;
    public KeyCode ReloadKey = KeyCode.R;
    public KeyCode PickUpWeaponKey = KeyCode.E;
    public KeyCode EquipWeaponKey = KeyCode.Tab;

    [HideInInspector]
    public KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };

    public enum AimMode
    {
        Hold,
        Toggle
    }
    [HideInInspector]
    public float xAxis, yAxis;

    [HideInInspector]
    public float camxAxis, camyAxis;


    // Use this for initialization
    void Start () {

        

	}

    // Update is called once per frame
    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        camxAxis = Input.GetAxisRaw("Mouse X");
        camyAxis = Input.GetAxisRaw("Mouse Y");
    }
}
