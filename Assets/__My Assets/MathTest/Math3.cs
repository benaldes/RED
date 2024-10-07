using System;
using UnityEngine;

public class Math3 : MonoBehaviour
{
    [SerializeField] private Transform inPoint;
    [SerializeField] private Vector2 point;
    [SerializeField] private Vector2 vector1;
    [SerializeField] private Vector2 vector2;
    [SerializeField] private float result;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position,transform.up);
        Gizmos.DrawLine(inPoint.position,inPoint.position +  inPoint.up);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,transform.right);
        Gizmos.DrawLine(inPoint.position,inPoint.position + inPoint.right);

        var rootPoint = inPoint.position;
        var rootRightDiraction = inPoint.right;
        
        var newPointPosition = rootPoint + (point.x * rootRightDiraction) + (point.y * inPoint.up);
        Gizmos.DrawWireSphere(newPointPosition,0.1f);

        result = Vector2.Dot(transform.up, inPoint.up);
        vector2 = transform.up;
    }
}
    
