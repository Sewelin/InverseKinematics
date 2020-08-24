using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKControl : MonoBehaviour
{
    private enum IKState
    {
        Off,
        Ankle,
        ToeBase
    }
    
    private Animator _animator;

#pragma warning disable 0649
    [SerializeField] private IKState state;
    [SerializeField, Range(0f, 1f)] private float rayOffset = 0.2f;
    
    [Header("Ankle")]
    [SerializeField, Range(0f, 0.3f)] private float footToGroundAnkle = 0.132f;
    [SerializeField, Range(0f, 0.5f)] private float rotationLerpParam = 0.2f;

    [Header("ToeBase")]
    [SerializeField] private Transform leftToeBase;
    [SerializeField] private Transform rightToeBase;
    [SerializeField, Range(0f, 0.3f)] private float footToGroundToeBase = 0.218f;
#pragma warning restore 0649
    
    private static readonly int IKLeftFootWeight = Animator.StringToHash("IKLeftFootWeight");
    private static readonly int IKRightFootWeight = Animator.StringToHash("IKRightFootWeight");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    //private void Update()
    //{
    //    if (Input.GetButton("Fire1"))
    //        _animator.MatchTarget(testTarget.position, testTarget.rotation, AvatarTarget.LeftFoot, new MatchTargetWeightMask(Vector3.one, 1f), 0f, 1f);
    //}

    private void OnAnimatorIK(int layerIndex)
    {
        switch (state)
        {
            case IKState.Off:
                break;
            case IKState.Ankle:
                SetFootIkAnkle(AvatarIKGoal.LeftFoot, IKLeftFootWeight);
                SetFootIkAnkle(AvatarIKGoal.RightFoot, IKRightFootWeight);
                break;
            case IKState.ToeBase:
                SetFootIkToeBase(AvatarIKGoal.LeftFoot, leftToeBase, IKLeftFootWeight);
                SetFootIkToeBase(AvatarIKGoal.RightFoot, rightToeBase, IKRightFootWeight);
                break;
        }
    }

    /**
     * Set foot ik target using raycast from the ankle position (classic algorithm)
     */
    private void SetFootIkAnkle(AvatarIKGoal goal, int weightProperty)
    {
        // Cast a ray to detect ground
        var ray = new Ray(_animator.GetIKPosition(goal) + rayOffset * Vector3.up, Vector3.down);
        if (!Physics.Raycast(ray, out var hitInfo, rayOffset + footToGroundAnkle + 0.1f, ~(1 << gameObject.layer),
            QueryTriggerInteraction.Ignore)) return;

        var weight = _animator.GetFloat(weightProperty);
        _animator.SetIKPositionWeight(goal, weight);
        _animator.SetIKRotationWeight(goal, weight);

        // Set position
        var ikTarget = hitInfo.point;
        ikTarget.y += footToGroundAnkle; // Because what we call foot is in fact the ankle, hence a little higher
        _animator.SetIKPosition(goal, ikTarget);

        // Set rotation
        _animator.SetIKRotation(goal, GetFootIKRotation(goal, hitInfo));
    }
    
    /**
     * Set foot ik target using raycast from the toe base position (prettier, and not really more expensive)
     */
    private void SetFootIkToeBase(AvatarIKGoal goal, Transform toeBase, int weightProperty)
    {
        var toeBasePosition = toeBase.position;
        var ankleToToeBase = toeBasePosition - _animator.GetIKPosition(goal);
        
        // Cast a ray to detect ground
        var ray = new Ray(toeBasePosition + rayOffset * Vector3.up, Vector3.down);
        if (!Physics.Raycast(ray, out var hitInfo, rayOffset + footToGroundToeBase + 0.1f, ~(1 << gameObject.layer),
            QueryTriggerInteraction.Ignore)) return;

        var weight = _animator.GetFloat(weightProperty);
        _animator.SetIKPositionWeight(goal, weight);
        _animator.SetIKRotationWeight(goal, weight);
        
        // Rotation
        var rotation = GetFootIKRotation(goal, hitInfo);
        
        // Set position
        Vector3 ikTarget;
        ikTarget = hitInfo.point;
        ikTarget += footToGroundToeBase * Vector3.Normalize(hitInfo.normal); // Because the toe base isn't on the sole, but a little higher
        ikTarget -= rotation * Quaternion.Inverse(_animator.GetIKRotation(goal)) * ankleToToeBase; // Get ankle IK position (apply toe base to ankle displacement)
        _animator.SetIKPosition(goal, ikTarget);
        _animator.SetIKRotation(goal, rotation);
    }

    private Quaternion GetFootIKRotation(AvatarIKGoal goal, RaycastHit hitInfo, bool ankle = false)
    {
        // Flat stands for the xOz plane
        var rotFlat = Quaternion.Euler(0f, _animator.GetIKRotation(goal).eulerAngles.y, 0f);
        var zFlat = rotFlat * Vector3.forward;
        var y = hitInfo.normal;
        var x = Vector3.Cross(zFlat, y);
        var rot = Quaternion.LookRotation(Vector3.Cross(y, x), y);

        if (ankle) // lerp to place the middle of the foot on the ground, and not the heel (if raycast from the ankle)
            rot = Quaternion.Lerp(rot, rotFlat, rotationLerpParam);
        
        return rot;
    }
}
