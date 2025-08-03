using UnityEngine;

public class PlayerCameraOld : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    private Vector3 _eulerAngle;
    public void Init(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngle = target.position;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngle += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        transform.eulerAngles = _eulerAngle;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}

public struct CameraInput
{
    public Vector2 Look;
}
