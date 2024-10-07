using System;
using UnityEditor;
using UnityEngine;

public class LookAtTrigger : MonoBehaviour
{
    [SerializeField] private Transform lookAtTarget;
    [Range(0f,1f),SerializeField] private float threshold;
    [SerializeField] private float pointPosition;

    private void OnDrawGizmos()
    {
        Vector3 dip = (lookAtTarget.position - transform.position).normalized;
        
        Gizmos.DrawLine(Vector3.zero,(lookAtTarget.position - transform.position));

        float dot = Vector2.Dot(dip, transform.right);
        
        
        Gizmos.color = dot > threshold ? Color.green : Color.red;
        
        Gizmos.DrawLine(transform.position,transform.position + dip);
    }
}
