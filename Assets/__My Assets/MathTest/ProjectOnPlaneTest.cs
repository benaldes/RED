using System;
using UnityEngine;

public class ProjectOnPlaneTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, hit.point);
            
            var right = Vector3.Cross(hit.normal,transform.forward );
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + right);

            var up = Vector3.Cross(transform.forward,right);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(hit.point, hit.point + up);

        }
    }
}
