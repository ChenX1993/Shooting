using UnityEngine;
using System;
using System.Collections;

public class IKControl : MonoBehaviour
{
    [HideInInspector]
    public PlayerBehaviour pB;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Vector3 rightHandPos, leftHandPos;

    public float rightHandWeight, lookAtWeight, leftHandWeight;

    public bool useleftHand;

    [HideInInspector]
    public Transform head;

    void Start()
    {
        leftHandWeight = 0;
        rightHandWeight = 0;
        lookAtWeight = 0;
    }
    void OnAnimatorIK()
    {
        if (animator)
        {
            if (pB.inMoveState)
            {
                LerpLookAtWeight(.5f, 5);
            }
            else
            {
                LerpLookAtWeight(0, 5);
            }
            if (pB.aim)
            {
                animator.SetLookAtPosition(head.position + pB.aimHelper.forward);
            }
            else
            {
                animator.SetLookAtPosition(head.position + pB.cam.forward);
            }
            animator.SetLookAtWeight(lookAtWeight);

            if (pB.equippedWeapon)
            {
                
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, pB.rightHandInWeapon.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, pB.rightHandInWeapon.rotation);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, pB.leftHandInWeapon.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, pB.leftHandInWeapon.rotation);
                if (pB.currentWeapon.usingLeftHand)
                {
                    leftHandWeight = 1;
                }
                else
                {
                    leftHandWeight = 0;
                }
                LerpRightHandWeight(1, 5);
            }
            else
            {

                leftHandWeight = 0;
                rightHandWeight = 0;
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);
            }
        }

    }
    public void LerpHandWeight(float to, float t)
    {
        leftHandWeight = Mathf.Lerp(leftHandWeight, to, t * Time.deltaTime);
    }
    public void LerpRightHandWeight(float to, float t)
    {
        rightHandWeight = Mathf.Lerp(rightHandWeight, to, t * Time.deltaTime);
    }
    public void LerpLookAtWeight(float to, float t)
    {
        lookAtWeight = Mathf.Lerp(lookAtWeight, to, t * Time.deltaTime);
    }
}