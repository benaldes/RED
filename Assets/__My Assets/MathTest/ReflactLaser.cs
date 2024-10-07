using System;
using UnityEngine;

public class ReflactLaser : MonoBehaviour
{
    public Vector3 laserDirection;
    public Vector3 reflactDirection;
    public float floatTest;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        laserDirection = transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            floatTest = Vector3.Dot(hit.normal, laserDirection);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal );
            
            var project = -Vector3.Dot(hit.normal, laserDirection) * hit.normal * 2f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, hit.point + project);
            
            var reflectDirection = laserDirection - Vector3.Dot(hit.normal, laserDirection) * hit.normal *2f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, hit.point + reflectDirection);
            
           
            
        }
    }
}
