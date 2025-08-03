using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    
    public float distance = 100f;
    public Transform ground;
    private void OnDrawGizmos()
    {
        var downDirection =  ground.position - transform.position;
        
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward );
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, downDirection, out hit))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, hit.point);

            var hitNormal = hit.normal;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(hit.point, hit.point + hitNormal);

            var right = Vector3.Cross(hitNormal, transform.forward);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, hit.point + right);
            
            var forward = Vector3.Cross(right, hitNormal);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(hit.point, hit.point + forward);
            
            
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + downDirection * distance);
        }
    }
}
