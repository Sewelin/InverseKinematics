using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour
{
    private Vector2 _move;
    private Vector2 _look;
    
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Camera _cam;
    private Collider _collider;
#pragma warning disable 0649
    [SerializeField] private CinemachineFreeLook vCam;
    [SerializeField] private float speedMax = 1.2f;
    [SerializeField] private Vector2 stepHeight = new Vector2(0.01f, 0.2f);
#pragma warning restore 0649
    
    private static readonly int Speed = Animator.StringToHash("Speed");

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _look = value.Get<Vector2>();
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _cam = Camera.main;
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        vCam.m_XAxis.m_InputAxisValue = _look.x;
        vCam.m_YAxis.m_InputAxisValue = _look.y;
    }

    private void FixedUpdate()
    {
        _animator.SetFloat(Speed, _move.magnitude);
        
        var forwardCam = _cam.transform.forward;
        var forward = new Vector2(forwardCam.x, forwardCam.z);
        forward.Normalize();
        var rightCam = _cam.transform.right;
        var right = new Vector2(rightCam.x, rightCam.z);
        right.Normalize();
        var move = (_move.x * right + _move.y * forward) * speedMax;
        
        _rigidbody.velocity = new Vector3(move.x, _rigidbody.velocity.y, move.y);
        if (_move.magnitude > 0.1f)
            transform.rotation = Quaternion.LookRotation(new Vector3(move.x, 0, move.y));
    }
    
    // Step up over little obstacles
    private void OnCollisionEnter(Collision col)
    {
        var target = transform.position;
        var stepUp = false;
        foreach(var cp in col.contacts)
        {
            if (cp.point.y - _collider.bounds.min.y >= stepHeight.x || cp.point.y - _collider.bounds.min.y <= stepHeight.y) continue;
            if (cp.point.y < target.y) continue;
            target = cp.point;
            stepUp = true;
        }
        if (!stepUp) return;
        
        transform.position = Vector3.MoveTowards(transform.position, target + Vector3.up,
            Time.deltaTime * 3f);
        _rigidbody.velocity = transform.up;
    }
}
