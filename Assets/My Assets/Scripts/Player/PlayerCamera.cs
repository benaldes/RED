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
        //[SerializeField] private Volume volume;
        
        [Space]
        [SerializeField] private float sensitivity;
        [SerializeField] private float cameraFOVChangeRate;
        [SerializeField] private float cameraFOVMaxChange;
        
        [SerializeField] private float lensDistortionMaxAmount;
        [SerializeField] private float maxSpeedToLensDistortion;
        [SerializeField] private float minSpeedToLensDistortion;

        private Vector3 _eulerAngles;
        
        private float _initFOV;
        
        //private LensDistortion _lensDistortion;

        public void Init()
        {
           // volume.profile.TryGet(out _lensDistortion);
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
        public void UpdateCameraFOV(Vector3 characterVelocity)
        {
            // remove vertical component of velocity
            Vector3 characterHorizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up); 
            
            float a = Mathf.InverseLerp(minSpeedToLensDistortion, maxSpeedToLensDistortion, characterHorizontalVelocity.magnitude);
            a = Mathf.Lerp(0f, lensDistortionMaxAmount, a);
            
            /*_lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = -a;
            */
        
        }

        
        
    }
}