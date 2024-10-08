using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace __My_Assets._Scripts.Player
{
    public class PlayerCharacter : MonoBehaviour,ICharacterController
    {
        [Header("Reference")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform root;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private ParticleSystem dashWhiteLinesParticleSystem;
        
        [Header("Ground Movement")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float groundAcceleration;
        
        [Header("Air")]
        [SerializeField] private float airSpeed;
        [SerializeField] private float airAcceleration;
        [SerializeField] private float gravity;
        
        [Header("Jumping")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float coyoteTime;
        
        [Header("Dashing")]
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashForce;
        [SerializeField] private float dashDuration;
        [SerializeField] private float dashCooldown;
        
        
        [Header("Crouching")]
        [Range(0f, 1f),SerializeField] private float crouchCapsuleHeightOffset;
        [Range(0f, 1f),SerializeField] private float standCameraHeightOffset;
        [Range(0f, 1f),SerializeField] private float crouchCameraHeightOffset;
        [SerializeField] private float cameraResponseTime;
        
        [Header("Other")]
        [SerializeField] private LayerMask collisionLayers;
        
        private CharacterState _state;
        
        // represent the direction and speed the player wants to move along in each frame
        private Vector3 _requestedMovement;
        private Quaternion _requestedRotation;
        
        // is the jump button pressed in this frame?
        private bool _requestedJump;
        // is the Crouch button pressed in this frame?
        private bool _requestedCrouch;
        // is the Dash button pressed in this frame?
        private bool _requestedDash;
        
        private float _initialStandingHeight;
        
        private float _lastDashTime;
        
        private bool _isDashing;
        
        private float _lastGroundedTime;
        
        public void Init()
        {
            _state = new CharacterState()
            {
                Stance = Stance.Stand
            };
            
            _initialStandingHeight = motor.Capsule.height;
            motor.CharacterController = this;
        }

        public void UpdateInput(PlayerInputData inputData)
        {
            _requestedMovement = new Vector3(inputData.MoveInput.x, 0f, inputData.MoveInput.y);
            // Clamp to 1 so that diagonal movement isn't faster()
            _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f); 
            // Rotate the movement vector to match the player's current look direction
            _requestedMovement = inputData.LookDirection * _requestedMovement;

            _requestedRotation = inputData.LookDirection;
            
            _requestedJump = inputData.JumpInput || _requestedJump;
            _requestedCrouch = inputData.CrouchInput;
            _requestedDash = inputData.DashInput || _requestedDash;
        }

        public void UpdateBody(float deltaTime)
        {
            if (_state.Stance == Stance.Stand)
            {
                cameraTarget.localPosition = Vector3.Lerp
                (
                    a: cameraTarget.localPosition,
                    b: new Vector3(0f, motor.Capsule.height * standCameraHeightOffset, 0f),
                    t: 1f - Mathf.Exp(-cameraResponseTime * deltaTime) 
                );
                
                root.localScale = Vector3.Lerp
                (
                    a: root.localScale,
                    b: Vector3.one,
                    t: 1f - Mathf.Exp(-cameraResponseTime * deltaTime) 
                );
            }
            else if(_state.Stance == Stance.Crouch)
            {
                cameraTarget.localPosition = Vector3.Lerp
                (
                    a: cameraTarget.localPosition,
                    b: new Vector3(0f, motor.Capsule.height * crouchCameraHeightOffset, 0f),
                    t: 1f - Mathf.Exp(-cameraResponseTime * deltaTime)
                );
                
                root.localScale = Vector3.Lerp
                (
                    a: root.localScale,
                    b: new Vector3(1f,  crouchCameraHeightOffset, 1f),
                    t: 1f - Mathf.Exp(-cameraResponseTime * deltaTime) 
                );
            }
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Get the new forward direction based on the current rotation and the requested rotation
            Vector3 forward = Vector3.ProjectOnPlane
            (
                vector: _requestedRotation * Vector3.forward,
                planeNormal: motor.CharacterUp
            );
            
            // set the player's rotation to the new rotation
            if(forward != Vector3.zero)
                currentRotation = Quaternion.LookRotation(forward,motor.CharacterUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 requestedVelocity = currentVelocity;

            // Ground movement
            if (motor.GroundingStatus.IsStableOnGround)
            {
                // Apply Ground input movement
                {
                    // project the requested movement on the ground normal 
                    Vector3 targetProjectedDirection = motor.GetDirectionTangentToSurface
                    (
                        direction: _requestedMovement,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * _requestedMovement.magnitude;
            
                    // determine the target speed based on the current stance
                    float targetSpeed = _state.Stance == Stance.Stand ? walkSpeed : crouchSpeed;
                    targetSpeed = _isDashing ? dashSpeed : targetSpeed;
                    // Get the velocity that would result if the player would move to the projected direction
                    Vector3 targetVelocity =  targetProjectedDirection * targetSpeed;
                    // Make sure the player is not moving faster than the max walk speed
                    targetVelocity = Vector3.ClampMagnitude(targetVelocity, targetSpeed);
                    // Lerp to the target velocity
                    requestedVelocity = Vector3.Lerp(requestedVelocity, targetVelocity, 1f - Mathf.Exp(-groundAcceleration * deltaTime));
                    // Set the player's velocity to the new calculated velocity
                    currentVelocity = requestedVelocity;
                }
                
                // Slide
                {
                    
                }
                
            }
            // Player in air
            else
            {
                // Apply Air input movement
                {
                    // project the requested movement on the Character Up direction 
                    Vector3 targetProjectedDirection = Vector3.ProjectOnPlane
                    (
                        vector : _requestedMovement,
                        planeNormal: motor.CharacterUp
                    ) * _requestedMovement.magnitude;
                    
                    float targetSpeed = _isDashing ? dashSpeed : airSpeed;
                    // Get the velocity that would result if the player would move to the projected direction
                    Vector3 targetVelocity = targetProjectedDirection * targetSpeed;
                    // Make sure the player is not moving faster than the max air speed
                    targetVelocity = Vector3.ClampMagnitude(targetVelocity, targetSpeed);
                    // Lerp to the target velocity
                    requestedVelocity = Vector3.Lerp
                    (
                        a: requestedVelocity, 
                        b: targetVelocity, 
                        t: 1f - Mathf.Exp(-airAcceleration * deltaTime)
                    );
                    
                    
                    requestedVelocity.y = currentVelocity.y;
                    currentVelocity = requestedVelocity;
                    
                }
                
                // Gravity
                {
                    requestedVelocity += new Vector3(0,gravity,0) * deltaTime;
                    // Set the player's velocity to the new calculated velocity
                    currentVelocity = requestedVelocity;
                }
            }
            
            // jump
            if (_requestedJump && motor.GroundingStatus.IsStableOnGround)
            {
                _requestedJump = false;
                
                // unstick the player from the ground
                motor.ForceUnground(0f);
                
                requestedVelocity = currentVelocity + new Vector3(0,jumpForce,0);
                currentVelocity = requestedVelocity;
            }

            // Dash
            {
                // Start dash
                if (_requestedDash && Time.time > _lastDashTime + dashCooldown && _requestedMovement.magnitude > 0f)
                {
                    
                    dashWhiteLinesParticleSystem.Play();
                    _isDashing = true;
                    _requestedDash = false;
                    _lastDashTime = Time.time;
                    Vector3 dashVelocity = _requestedMovement.normalized * dashForce;
                    currentVelocity += dashVelocity;
                }
                
                // Stop dash
                if (_isDashing && Time.time > _lastDashTime + dashDuration)
                {
                    _isDashing = false;
                }
                
                
            }
            
            
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _state.IsGrounded = motor.GroundingStatus.IsStableOnGround;
            
            if (_requestedCrouch && _state.Stance == Stance.Stand)
            {
                _state.Stance = Stance.Crouch;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: _initialStandingHeight - crouchCapsuleHeightOffset,
                    yOffset: (_initialStandingHeight - crouchCapsuleHeightOffset)*0.5f
                );
            }
                
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            if (!_requestedCrouch && _state.Stance == Stance.Crouch)
            {
                
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: _initialStandingHeight,
                    yOffset: _initialStandingHeight * 0.5f
                );
                Collider[] colliders = new Collider[8];

                int amountOfCollidersDetected = motor.CharacterOverlap
                (
                    motor.TransientPosition,
                    motor.TransientRotation,
                    colliders,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore

                );

                if (amountOfCollidersDetected > 0)
                {
                    motor.SetCapsuleDimensions(
                        radius: motor.Capsule.radius,
                        height: _initialStandingHeight - crouchCapsuleHeightOffset,
                        yOffset: (_initialStandingHeight -crouchCapsuleHeightOffset) * 0.5f);
                }
                else
                {
                    _state.Stance = Stance.Stand;
                }


            }
            _requestedJump = false;
            
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
        
        public Vector3 GetCurrentVelocity() => motor.Velocity;
        public float GetWalkSpeed() => walkSpeed;
    }

    public struct CharacterState
    {
        public Stance Stance;
        public bool IsGrounded;
    }
    
    public enum Stance
    {
        Stand,
        Crouch,
        Slide
    }
}