using System;
using UnityEditor;
using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private float radius = 0.5f;
    private void OnDrawGizmos()
    {
        //float distance = Vector3.Distance(transform.position, point.position);
        
        Vector2 dip = point.position - transform.position;
        
        float distanceSq = dip.x * dip.x + dip.y * dip.y;
        
        Handles.color = distanceSq > radius * radius ? Color.red : Color.green;
        
        //Gizmos.DrawSphere(transform.position, radius);
        Handles.DrawWireDisc(transform.position, Vector3.forward, radius);
    }
}
