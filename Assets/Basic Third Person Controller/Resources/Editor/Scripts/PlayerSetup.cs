using UnityEngine;
using UnityEditor;
using System.Collections;


//------------- PLEASE DON'T MESS WITH THIS SCRIPT---------------//
//- :'D ---------------------------------------------//
//- Or do it, you bought this asset anyway-----------//
//- Do whatever you want, but it's not recommended---//
public class PlayerSetup : EditorWindow
{

    Transform PlayerPrefab;
    GameObject HumanoidCharacter;
    bool fixorientation;
    float sizefactor = 3;
    float scalefactor = 1;

    float legCenter;
    Vector3 armsCenter, armsLenght, legsLenght;
    RuntimeAnimatorController playerAnimator;
    Editor gameObjectEditor;

    Animator _charAnim;
    GameObject _char;
    Vector3 hipsScale;
    string message = "";

   [MenuItem("Humanoid Basics/Player Setup")]
    public static void ShowWindow()
    {
        PlayerSetup charSetup = (PlayerSetup)EditorWindow.GetWindow(typeof(PlayerSetup), true, "");
        charSetup.minSize = new Vector2(325, 225);
        charSetup.maxSize = new Vector2(325, 225);
        
    }

    void OnGUI()
    {
        Texture titleTexture = Resources.Load("Editor/Textures/PlayerSetup") as Texture;
        GUILayout.Label(titleTexture);

        GUILayout.Space(5);
        GUILayout.Label("Model Settings", EditorStyles.boldLabel);
        HumanoidCharacter = EditorGUILayout.ObjectField("Humanoid (FBX)", HumanoidCharacter, typeof(GameObject), false) as GameObject;

            scalefactor = EditorGUILayout.Slider("Scale Factor", scalefactor, 0.5f, 3);
            GUILayout.Space(5);
            GUILayout.Label("Ragdoll Settings", EditorStyles.boldLabel);
            sizefactor = EditorGUILayout.Slider("Collider Scale Factor", sizefactor, 0.5f, 5);
        RuntimeAnimatorController _pAnim = Resources.Load("Animators/PlayerAnimator") as RuntimeAnimatorController;

        if (!_pAnim)
        {
            GUILayout.Space(5);
            GUILayout.Label("(Resources/Animators)", EditorStyles.boldLabel);
            playerAnimator = EditorGUILayout.ObjectField("Player Animator Controller", playerAnimator, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
        }

        if (_pAnim != null)
        {
            playerAnimator = _pAnim;
        }
        if (PlayerPrefab == null)
        {
            try
            {
                PlayerPrefab = GameObject.FindObjectOfType<PlayerBehaviour>().transform;
                Selection.activeGameObject = PlayerPrefab.gameObject;
                SceneView.FrameLastActiveSceneView();
            }
            catch
            {

                GameObject _p = PrefabUtility.InstantiatePrefab((GameObject)Resources.Load("Prefabs/Player")) as GameObject;
                PlayerPrefab = _p.transform;
                Selection.activeGameObject = PlayerPrefab.gameObject;
                SceneView.FrameLastActiveSceneView();
                _p.name = "Player";
                message = "New Player Created!";
            }
        }

        if (HumanoidCharacter != null)
        {
            Animator _charAnim = HumanoidCharacter.GetComponent<Animator>();
            if (_charAnim == null)
            {
                message = "This asset has no Animator";
                HumanoidCharacter = null;
                return;
            }
            else
            {
                if (_charAnim.avatar == null)
                {
                    message = "This Animator has no avatar";
                    HumanoidCharacter = null;
                    return;
                }
                if (!_charAnim.avatar.isHuman)
                {
                    message = "This Asset is not humanoid";
                    HumanoidCharacter = null;
                    return;
                }
            }
        }
        GUILayout.Space(5);

        if (GUILayout.Button("Done"))
        {
            Setup();
        }
        if(message == "New Player Created!")
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
        else if (message == "All right!")
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }else if(message != "")
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }
    }
    void Setup()
    {
        if (PlayerPrefab == null)
        {
            message = "Missing Player Behaviour Transform";
            return;
        }
        if (HumanoidCharacter == null)
        {
            message = "Missing Humanoid Asset";
            return;
        }
        if (playerAnimator == null)
        {
            message = "Missing Player Animator Controller";
            return;
        }

        if (PlayerPrefab.GetComponentInChildren<Animator>() != null)
        {
            DestroyImmediate(PlayerPrefab.GetComponentInChildren<Animator>().gameObject, false);
        }

        _char = Instantiate(HumanoidCharacter, PlayerPrefab.position, PlayerPrefab.rotation) as GameObject;     
        _char.name = "Animator";
        _char.transform.localScale = _char.transform.localScale * scalefactor;
        _char.AddComponent<PlayerAnimationListener>();
        _char.AddComponent<RagdollHelper>();
        IKControl ikControll = _char.AddComponent<IKControl>();
        

        _char.transform.SetParent(PlayerPrefab);

        

        _charAnim = _char.GetComponent<Animator>();

        PlayerBehaviour pB = PlayerPrefab.GetComponent<PlayerBehaviour>();
        _char.GetComponent<RagdollHelper>().pB = pB;
        PlayerPrefab.GetComponent<CameraBehaviour>().pB = pB;

        pB.rb = pB.GetComponent<Rigidbody>();
        pB.capsuleC = pB.GetComponent<CapsuleCollider>();
        pB.playerAnimator = _charAnim;

        pB.ragdollh = pB.playerAnimator.GetComponent<RagdollHelper>();
        pB.ikControll = ikControll;
        pB.pathMaker = pB.GetComponent<TransformPathMaker>();

        pB.cameraParent = pB.transform.Find("Camera");
        pB.camPivot[0] = pB.cameraParent.GetChild(0);
        pB.camPivot[1] = pB.camPivot[0].GetChild(0);
        pB.cam = pB.camPivot[1].Find("Main Camera");

        pB.controller = pB.GetComponent<Controller>();
        pB.cameraBehaviour = pB.GetComponent<CameraBehaviour>();

        GameObject _aimHelper;
        try
        {
            _aimHelper = _charAnim.transform.Find("Aim Helper").gameObject;
            pB.aimHelper = _aimHelper.transform;
        }
        catch
        {
            _aimHelper = new GameObject("Aim Helper");

            _aimHelper.transform.parent = _charAnim.GetBoneTransform(HumanBodyBones.Head);
            _aimHelper.transform.localPosition = Vector3.zero;

            Transform _spine = _charAnim.GetBoneTransform(HumanBodyBones.Spine);
            _aimHelper.transform.parent = _spine;
            pB.aimHelper = _aimHelper.transform;
            pB.aimHelperSpine = _spine;

            _spine.TransformVector(_charAnim.transform.forward);
            ikControll.head = _charAnim.GetBoneTransform(HumanBodyBones.Head);
        }
        ikControll.pB = pB;
        ikControll.animator = _charAnim;

        pB.transformToRotate = pB.playerAnimator.transform;
        pB.pathMaker.reference = pB.transformToRotate;
        pB.audioSource = pB.GetComponent<AudioSource>();
        pB.boneRb = new Rigidbody[15];
        _charAnim.runtimeAnimatorController = playerAnimator;
        _charAnim.applyRootMotion = false;
        _charAnim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        AddCollider(HumanBodyBones.Hips, HumanBodyBones.Hips);

        AddCollider(HumanBodyBones.Chest, HumanBodyBones.Hips);

        AddCollider(HumanBodyBones.Head, HumanBodyBones.Chest);

        AddCollider(HumanBodyBones.LeftUpperArm, HumanBodyBones.Chest);
        AddCollider(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm);
        AddCollider(HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm);

        AddCollider(HumanBodyBones.RightUpperArm, HumanBodyBones.Chest);
        AddCollider(HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm);
        AddCollider(HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm);

        AddCollider(HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips);
        AddCollider(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg);
        AddCollider(HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg);

        AddCollider(HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips);
        AddCollider(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg);
        AddCollider(HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg);

        pB.hipsParent = pB.boneRb[0].transform.parent;

        Vector3 spineForward = pB.aimHelperSpine.forward;
        if(spineForward.z < 0.5f)
        {
            if(spineForward.x < -0.5f)
            {
                pB.spineFacingDirection = PlayerBehaviour.Direction.Up;
            }
        }
        else
        {
            pB.spineFacingDirection = PlayerBehaviour.Direction.Forward;
      
        }

        pB.leftFoot = _charAnim.GetBoneTransform(HumanBodyBones.LeftFoot);
        pB.rightFoot = _charAnim.GetBoneTransform(HumanBodyBones.RightFoot);

        message = "All right!";

       
    }
    void SetJointLimits(CharacterJoint joint, float low, float high, float swing, float swing1)
    {

        SoftJointLimitSpring cjTwist = joint.twistLimitSpring;
        SoftJointLimit cjLowLimit = joint.lowTwistLimit;
        SoftJointLimit cjHighLimit = joint.highTwistLimit;
        SoftJointLimit cjSwingLimit1 = joint.swing1Limit;
        SoftJointLimit cjSwingLimit2 = joint.swing2Limit;

        cjLowLimit.limit = low;
        cjHighLimit.limit = high;
        cjSwingLimit1.limit = swing;
        cjSwingLimit2.limit = swing1;

        joint.twistLimitSpring = cjTwist;
        joint.lowTwistLimit = cjLowLimit;
        joint.highTwistLimit = cjHighLimit;
        joint.swing1Limit = cjSwingLimit1;
        joint.swing2Limit = cjSwingLimit2;

    }
    void AddCollider(HumanBodyBones bone, HumanBodyBones connectTo)
    {
        GameObject b = _charAnim.GetBoneTransform(bone).gameObject;
        Rigidbody cT = _charAnim.GetBoneTransform(connectTo).gameObject.GetComponent<Rigidbody>();

        b.tag = "Bone";
        b.layer = 2;
        b.AddComponent<Rigidbody>();

        Rigidbody r = b.GetComponent<Rigidbody>();

        CharacterJoint cJ = b.AddComponent<CharacterJoint>();
        cJ.connectedBody = cT;

        SetJointLimits(cJ, 0, 0, 0, 0);

        cJ.enableCollision = false;
        cJ.enableProjection = true;
        cJ.enablePreprocessing = false;

        
        float _sizefactor;
        _sizefactor = sizefactor / 20;

        if (bone == HumanBodyBones.Head)
        {
            SphereCollider sc = b.AddComponent<SphereCollider>();
            sc.radius = 0.1f;
            sc.center = new Vector3(0, 0.09f, 0);
            cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);

        }
        else
        {
            BoxCollider c = b.AddComponent<BoxCollider>();
            c.size = c.size * _sizefactor;

            if (bone == HumanBodyBones.Hips)
            {
                DestroyImmediate(cJ);

                float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.Chest).position) / 1.2f;
                float x = Vector3.Distance(_charAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, _charAnim.GetBoneTransform(HumanBodyBones.RightUpperArm).position) / 1.8f;
                c.center = c.transform.InverseTransformDirection(PlayerPrefab.up) * (y / 2);
                c.center = c.transform.InverseTransformDirection(PlayerPrefab.up) * (y / 2);
                c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.up) * y +
                         c.transform.InverseTransformDirection(PlayerPrefab.right) * x;
            }
            if (bone == HumanBodyBones.Chest)
            {
                c.size = c.size * _sizefactor * 8;
                SetJointLimits(cJ, 0, 50, 10, 10);
                cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);

                float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.Head).position) / 1.4f;
                float x = Vector3.Distance(_charAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, _charAnim.GetBoneTransform(HumanBodyBones.RightUpperArm).position) / 1.4f;
                c.center = c.transform.InverseTransformDirection(PlayerPrefab.up) * (y / 2);
                c.center = c.transform.InverseTransformDirection(PlayerPrefab.up) * (y / 2);
                c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.up) * y +
                         c.transform.InverseTransformDirection(PlayerPrefab.right) * x;
            }
            if (bone == HumanBodyBones.RightUpperLeg || bone == HumanBodyBones.LeftUpperLeg)
            {
                SetJointLimits(cJ, -90, 0, 0, 0);
                cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);

                if(bone == HumanBodyBones.RightUpperLeg)
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position)/1.1f;
                    c.center = c.transform.InverseTransformDirection(-PlayerPrefab.up) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                             c.transform.InverseTransformDirection(PlayerPrefab.up) * y +
                             c.transform.InverseTransformDirection(PlayerPrefab.right) * _sizefactor;
                }
                else
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position)/1.1f;
                    c.center = c.transform.InverseTransformDirection(-PlayerPrefab.up) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                             c.transform.InverseTransformDirection(PlayerPrefab.up) * y +
                             c.transform.InverseTransformDirection(PlayerPrefab.right) * _sizefactor;
                }
            }
            if (bone == HumanBodyBones.RightLowerLeg || bone == HumanBodyBones.LeftLowerLeg)
            {
                SetJointLimits(cJ, 0, 120, 0, 0);
                cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);
                float y = Vector3.Distance(b.transform.position, cT.position)/1.1f;

                c.center = c.transform.InverseTransformDirection(PlayerPrefab.up) * (y);

                c.center = c.transform.InverseTransformDirection(-PlayerPrefab.up) * (y / 2);
                c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.up) * y +
                         c.transform.InverseTransformDirection(PlayerPrefab.right) * _sizefactor;

            }
            if (bone == HumanBodyBones.LeftUpperArm || bone == HumanBodyBones.RightUpperArm)
            {
                SetJointLimits(cJ, -90, 90, 90, 90);
                cJ.axis = cJ.transform.InverseTransformDirection(_char.transform.forward);
               
                if (bone == HumanBodyBones.LeftUpperArm)
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position) / 1.1f;
                    c.center = c.transform.InverseTransformDirection(-PlayerPrefab.right) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.up) * _sizefactor +
                         c.transform.InverseTransformDirection(-PlayerPrefab.right) * y;
                }
                else
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.RightLowerArm).position)/1.1f;
                    c.center = c.transform.InverseTransformDirection(PlayerPrefab.right) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.up) * _sizefactor +
                         c.transform.InverseTransformDirection(PlayerPrefab.right) * y;
                }
                

            }
            if (bone == HumanBodyBones.LeftLowerArm || bone == HumanBodyBones.RightLowerArm)
            {
                SetJointLimits(cJ, -120, 0, 10, 10);

                if (bone == HumanBodyBones.LeftLowerArm)
                {
                    cJ.axis = cJ.transform.InverseTransformDirection(_char.transform.forward);
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.LeftHand).position) / 1.1f;
                    c.center = c.transform.InverseTransformDirection(-PlayerPrefab.right) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                             c.transform.InverseTransformDirection(PlayerPrefab.up) * _sizefactor +
                             c.transform.InverseTransformDirection(-PlayerPrefab.right) * y;
                }
                else
                {
                    cJ.axis = cJ.transform.InverseTransformDirection(_char.transform.forward);
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.RightHand).position) / 1.1f;
                    c.center = c.transform.InverseTransformDirection(PlayerPrefab.right) * (y / 2);
                    c.size = c.transform.InverseTransformDirection(PlayerPrefab.forward) * _sizefactor +
                             c.transform.InverseTransformDirection(PlayerPrefab.up) * _sizefactor +
                             c.transform.InverseTransformDirection(PlayerPrefab.right) * y;
                }
            }
            if (bone == HumanBodyBones.RightHand || bone == HumanBodyBones.LeftHand)
            {
                SetJointLimits(cJ, -50, 50, 20, 20);
                cJ.axis = cJ.transform.InverseTransformDirection(_char.transform.forward);

                if (bone == HumanBodyBones.LeftHand)
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
                    c.center = c.transform.InverseTransformDirection(-PlayerPrefab.right) * (y / 2);
                }
                else
                {
                    float y = Vector3.Distance(b.transform.position, _charAnim.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
                    c.center = c.transform.InverseTransformDirection(PlayerPrefab.right) * (y / 2);
                }
            }
            if (bone == HumanBodyBones.RightFoot || bone == HumanBodyBones.LeftFoot)
            {
                SetJointLimits(cJ, -20, 20, 10, 10);
                cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);

                DestroyImmediate(c);

                
                float y = Vector3.Distance(b.transform.position, PlayerPrefab.position);
                SphereCollider sc = b.AddComponent<SphereCollider>();
                sc.center = sc.transform.InverseTransformDirection(-PlayerPrefab.up) * (y / 2.5f);
                sc.radius = _sizefactor/2.5f ;

                SphereCollider sc2 = b.AddComponent<SphereCollider>();
                sc2.center = sc2.transform.InverseTransformDirection(-PlayerPrefab.up) * (y / 2.5f) + sc2.transform.InverseTransformDirection(PlayerPrefab.forward) * (y / 1.2f);
                sc2.radius = _sizefactor / 2.8f;
            }
            if (bone == HumanBodyBones.Head)
            {
                SetJointLimits(cJ, -50, 50, 50, 50);
                cJ.axis = cJ.transform.InverseTransformDirection(-_char.transform.right);
            }
            if (c)
            {
                c.size = new Vector3(Mathf.Abs(c.size.x), Mathf.Abs(c.size.y), Mathf.Abs(c.size.z));
            }
        }

        PlayerBehaviour pb = PlayerPrefab.GetComponent<PlayerBehaviour>();
        for (int i = 0; i < pb.boneRb.Length; i++)
        {
            if (pb.boneRb[i] == null)
            {
                pb.boneRb[i] = r;
                return;
            }

        }
    }
}
