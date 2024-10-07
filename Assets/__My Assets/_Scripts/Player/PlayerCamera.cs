using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace __My_Assets._Scripts.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Volume volume;
        
        
        [Space]
        [SerializeField] private float sensitivity;
        [SerializeField] private float cameraFOVChangeRate;
        [SerializeField] private float cameraFOVMaxChange;
        
        [SerializeField] private float lensDistortionChangeRate;
        [SerializeField] private float lensDistortionMaxChange;

        private Vector3 _eulerAngles;
        
        private float _initFOV;
        
        private LensDistortion _lensDistortion;

        public void Init()
        {
            volume.profile.TryGet(out _lensDistortion);
            _initFOV = mainCamera.fieldOfView;
            _eulerAngles = transform.rotation.eulerAngles;
        }
        public void UpdateRotation(Vector2 look)
        {
            // create the new angle the player is looking
            _eulerAngles += new Vector3(-look.y, look.x) * sensitivity;
            
            // clamp the vertical angle so the player cant look upside down
            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -90f, 90f);
            
            // apply the rotation
            transform.rotation = Quaternion.Euler(_eulerAngles) ;
        }
        
        public void UpdatePosition()
        {
            transform.position = cameraTarget.position;
        }
        
        // makes the camera field of view change based on player velocity
        public void UpdateCameraFOV(Vector3 characterVelocity,float normalSpeed,float deltaTime)
        {
            float playerSpeed = characterVelocity.magnitude;
            float a = playerSpeed/normalSpeed;
            float currentFOV = mainCamera.fieldOfView;
            float targetFOV = _initFOV + (cameraFOVMaxChange * Mathf.Clamp((a - 1), 0, 2));
            /*mainCamera.fieldOfView = Mathf.Lerp
            (
                currentFOV,
                targetFOV,
                1 - Mathf.Exp(-cameraFOVMaxChange *  deltaTime)
            );
            */
            
            float b  = Mathf.Clamp(-a,-0.3f,0f);

            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = -b;
            

        }
        
    }
}