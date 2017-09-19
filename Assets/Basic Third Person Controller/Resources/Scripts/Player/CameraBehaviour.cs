using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    [HideInInspector]
    public PlayerBehaviour pB;
    [HideInInspector]
    public Controller controller;

    [Header("Camera Settings")]
    public bool recoilInfluence = true;
    public float recoilInfluenceFactor = 20;
    public Vector2 cameraOffset = new Vector2(0.5f, 1.5f);
    public float maxDistance = 2;
    public float aimDifference = 0.3f;
    public float mouseSensitivity = 3;

    private float currentCamDistance, currentAimDifference, camAngleX, camAngleZ;
    private float downClamp;

    [HideInInspector]
    public bool aimIsRightSide;

    [HideInInspector]
    public float originalSide;

    private bool sideCollision, currentSideCollision, oppositeSideCollision;

    void Start () {
        controller = GetComponent<Controller>();
        currentCamDistance = maxDistance;

        originalSide = cameraOffset.x;
        if (cameraOffset.x >= 0)
        {
            aimIsRightSide = true;
        }
    }

    void Update () {

        float x = controller.camxAxis;
        float y = controller.camyAxis;

        camAngleX += x * mouseSensitivity;
        
        pB.camPivot[0].localEulerAngles = new Vector3(0, camAngleX, 0);

        camAngleZ += y * -mouseSensitivity;
        if (!pB.aim)
        {
            currentAimDifference = 0;
            downClamp = Mathf.Lerp(downClamp, 89, 5 * Time.deltaTime);
            camAngleZ = Mathf.Clamp(camAngleZ, -60, downClamp);
        }
        else
        {
            currentAimDifference = aimDifference;
            downClamp = Mathf.Lerp(downClamp, 70, 8 * Time.deltaTime);
            camAngleZ = Mathf.Clamp(camAngleZ, -60, downClamp);
        }
        pB.camPivot[1].localEulerAngles = new Vector3(camAngleZ, 0, 0);


        RaycastHit _h, _sH;
        Vector3 startPoint = pB.camPivot[0].position;

        currentSideCollision = Physics.SphereCast(startPoint, .2f, -pB.cam.forward + pB.cam.right * (cameraOffset.x * 1), out _sH, maxDistance / 2);
        oppositeSideCollision = Physics.SphereCast(startPoint, .2f, -pB.cam.forward + pB.cam.right * (cameraOffset.x * -1), out _sH, maxDistance / 2);

        if (Input.GetKeyDown(controller.SwitchAimSideKey) && controller.canSwitchAimSide && !oppositeSideCollision)
        {
            aimIsRightSide = !aimIsRightSide;
            cameraOffset.x = cameraOffset.x * -1;
            originalSide = cameraOffset.x;
        }
        if (pB.aim)
        {
            sideCollision = currentSideCollision;
        }
        else
        {
            sideCollision = false;
        }
        if (Physics.SphereCast(startPoint, 0.1f, -pB.cam.forward, out _h, maxDistance/2) ||
            Physics.SphereCast(startPoint, 0.2f, -pB.cam.forward + pB.cam.right * (cameraOffset.x / 2), out _h, maxDistance))
        {
            float dist = Vector3.Distance(pB.camPivot[0].position, _h.point) - currentAimDifference;
            dist = Mathf.Clamp(dist, .1f, maxDistance);
            currentCamDistance = dist;

            GoSmooth(0, -currentCamDistance + 0.3f, 100);
        }
        else
        {
            currentCamDistance = maxDistance - currentAimDifference;
            GoSmooth(cameraOffset.x, -currentCamDistance, 10);
        }
        pB.cameraParent.position = pB.boneRb[0].transform.position + transform.up * cameraOffset.y;

        if (recoilInfluence)
        {
            if (pB.recoil > 0.1f)
            {
                float rX = Random.Range(pB.recoil * -1, pB.recoil) / recoilInfluenceFactor;
                float rY = Random.Range(pB.recoil * -1, pB.recoil) / recoilInfluenceFactor;
                pB.cam.localRotation = Quaternion.Lerp(pB.cam.localRotation, new Quaternion(rY, rX, 0, 1), 2 * Time.deltaTime);
            }
            else
            {
                pB.cam.localRotation = Quaternion.Lerp(pB.cam.localRotation, new Quaternion(0, 0, 0, 1), 2 * Time.deltaTime);
            }
        }
    }

    void GoSmooth(float x, float z, float t)
    {
        if (sideCollision && !oppositeSideCollision)
        {
            originalSide = cameraOffset.x;
            aimIsRightSide = !aimIsRightSide;
            cameraOffset.x = cameraOffset.x * -1;
        }
        else if(!pB.aim)
        {
            cameraOffset.x = originalSide;
            if (cameraOffset.x >= 0)
            {
                aimIsRightSide = true;
            }
            else
            {
                aimIsRightSide = false;
            }
        }

        float aimX = 0;

        if (aimIsRightSide)
        {
            aimX = 0.6f;
        }
        else
        {
            aimX = -0.6f;
        }

        if (pB.aim)
        {
            if(currentSideCollision && oppositeSideCollision)
            {
                aimX = 0;
            }
            pB.cam.localPosition = Vector3.Lerp(pB.cam.localPosition, new Vector3(aimX, 0, z), t * Time.deltaTime);
        }
        else
        {
            pB.cam.localPosition = Vector3.Lerp(pB.cam.localPosition, new Vector3(x, 0, z), t * Time.deltaTime);
        }
    }
}
