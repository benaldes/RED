using UnityEngine;
using UnityEngine.Serialization;

namespace __My_Assets.Scripts.Player
{
    public class CameraLean : MonoBehaviour
    {
        [SerializeField] private float attackDamping = 0.5f;
        [SerializeField] private float decayDamping = 0.3f;
        [SerializeField] private float walkStrength = 0.075f;
        [SerializeField] private float slideStrength = 0.2f;
        [SerializeField] private float strengthResponse = 5;
        
        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVel1;

        private float _smoothStrength;
        public void Init()
        {
            _smoothStrength = walkStrength;
        }

        public void UpdateLean(float deltaTime,bool isSliding ,Vector3 acceleration, Vector3 up)
        {
            var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
            var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decayDamping;

            _dampedAcceleration = Vector3.SmoothDamp
            (
                current: _dampedAcceleration,
                target: planarAcceleration,
                currentVelocity: ref _dampedAccelerationVel1,
                smoothTime: damping,
                maxSpeed: float.PositiveInfinity,
                deltaTime: deltaTime
            );

            var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;

            transform.localRotation = Quaternion.identity;
            
            var targetStrength = isSliding ? slideStrength : walkStrength;
            
            _smoothStrength = Mathf.Lerp(_smoothStrength,targetStrength,1f - Mathf.Exp(-strengthResponse * deltaTime));

            transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
        }
    }
}