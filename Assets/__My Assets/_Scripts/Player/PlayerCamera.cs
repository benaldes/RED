using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace __My_Assets._Scripts.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Refrances")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Volume volume;
        [SerializeField] private VolumeProfile volumeProfile;
        
        [Space]
        [SerializeField] private float sensitivity = 0.8f;

        [SerializeField] private float cameraFOVChangeRate;
        [SerializeField] private float cameraFOVMaxChange;

        private Vector3 _eulerAngles;
        
        private float _initFOV;

        public void Init()
        {
            
            _initFOV = mainCamera.fieldOfView;
            _eulerAngles = transform.rotation.eulerAngles;
        }
        public void UpdateRotation(Vector2 look)
        {
            /*LensDistortion lensDistortion = volumeProfile.components[0] as LensDistortion;
            lensDistortion.intensity = new ClampedFloatParameter(0.5f, 0, 1);
            lensDistortion.active = true;
            lensDistortion.xMultiplier = new ClampedFloatParameter(0.5f, 0, 1);*/
            
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
        public void UpdateCameraFOV(Vector3 characterVelocity,float normalSpeed)
        {
            float playerSpeed = characterVelocity.magnitude;
            float a = playerSpeed/normalSpeed;
            float currentFOV = mainCamera.fieldOfView;
            float targetFOV = _initFOV + (cameraFOVMaxChange * Mathf.Clamp((a - 1), 0, 2));
            mainCamera.fieldOfView = Mathf.Lerp(currentFOV, targetFOV, 1 - Mathf.Exp(-cameraFOVMaxChange *  Time.deltaTime));
            
            if (volumeProfile.TryGet(out LensDistortion lensDistortion))
            {
                // Enable the override for intensity
                lensDistortion.intensity.overrideState = true;

                // Set the intensity value
                lensDistortion.intensity.value = 0.5f;
            }
            
        }
        
    }
}