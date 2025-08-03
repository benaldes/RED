using UnityEngine;

namespace __My_Assets.Scripts.Player
{
    public class CameraSpring : MonoBehaviour
    {
        [SerializeField] private float halfLife = 0.075f;
        [SerializeField] private float frequency = 18f;
        [SerializeField] private float angularDisplacement = 2f;
        [SerializeField] private float linerDisplacement = 0.05f;
        
        private Vector3 _springPosition;
        private Vector3 _springVelocity;
        public void Init()
        {
            _springPosition = transform.position;
            _springVelocity = Vector3.zero;
        }

        public void UpdateSpring(float deltaTime, Vector3 up)
        {
            transform.position = Vector3.zero;
            
            Spring(ref _springPosition, ref _springVelocity,transform.position,halfLife,frequency,deltaTime);
            var localSpringPosition = _springPosition - transform.position;
            var springHeight = Vector3.Dot(localSpringPosition, up);
            
            transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
            transform.localPosition = localSpringPosition * linerDisplacement;
        }
        private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
        {
            var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
            float f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
            float oo = frequency * frequency;
            float hoo = timeStep * oo;
            float hhoo = timeStep * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector2 detX = f * current + timeStep * velocity + hhoo * target;
            Vector2 detV = velocity + hoo * (target - current);
            current = detX * detInv;
            velocity = detV * detInv;
        }
    }
}