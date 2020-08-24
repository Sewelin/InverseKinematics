using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class Stairs : MonoBehaviour
{
    [SerializeField, Range(1, 100)] private int numberOfStairs;
    private List<Transform> toDestroy = new List<Transform>();
    
    private static readonly int OnStairs = Animator.StringToHash("OnStairs");

    private void Awake()
    {
        numberOfStairs = transform.childCount;
    }

    private void OnValidate()
    {
        MakeStairs();
    }

    private void MakeStairs()
    {
        if (transform.childCount > numberOfStairs)
        {
            for (var i = transform.childCount; i > numberOfStairs; --i)
            {
                toDestroy.Add(transform.GetChild(i-1));
            }
        }
        else if (transform.childCount < numberOfStairs)
        {
            while (transform.childCount < numberOfStairs)
            {
                var stair = Instantiate(transform.GetChild(transform.childCount - 1), transform);
                stair.localPosition += new Vector3(0f, 0.17f, 0.3f);
            }
        }
    }

    private void Update()
    {
        if (toDestroy.Count <= 0) return;
        toDestroy.ForEach(t =>
        {
            if (t != null)
                DestroyImmediate(t.gameObject);
        });
        toDestroy.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Animator>().SetBool(OnStairs, true);
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Animator>().SetBool(OnStairs, false);
    }
}
