using System;
using KinematicCharacterController;
using TMPro;
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
        [SerializeField] private Transform attackPoint;
        
        [Header("Attack")]
        [SerializeField] private float attackRange;
        [SerializeField] private LayerMask DamagebleLayer;
        
        
        [Header("Ground Movement")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float groundAcceleration;
        [SerializeField] private float groundDeceleration;
        
        [Header("Air")]
        [SerializeField] private float airSpeed;
        [SerializeField] private float airAcceleration;
        [SerializeField] private float airDeceleration;
        [SerializeField] private float gravity;
        
        [Header("Jumping")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float coyoteTime;
        
        [Header("Crouching")]
        [Range(0f, 1f),SerializeField] private float crouchCapsuleHeightOffset;
        [Range(0f, 1f),SerializeField] private float standCameraHeightOffset;
        [Range(0f, 1f),SerializeField] private float crouchCameraHeightOffset;
        [SerializeField] private float cameraResponseTime;
        
        [Header("Sliding")]
        [SerializeField] private float slideGravity;
        [SerializeField] private float neededSpeedToSlide;
        [SerializeField] private float SpeedToStopSlide;
        [SerializeField] private float startSlideSpeedBoost;
        [SerializeField] private float groundFrictionWhenSliding;
        [SerializeField] private float crouchInAirToSlideCoyoteTime;
        
        [Header("Dashing")]
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashForce;
        [SerializeField] private float dashDuration;
        [SerializeField] private float dashCooldown;
        [SerializeField] private float dashGravityScale;
        
        [Header("debug text")]
        [SerializeField] private TMP_Text velocityText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text GroundedText;
        
        [Header("Other")]
        [SerializeField] private LayerMask collisionLayers;
        
        
        private CharacterState _state;
        private CharacterState _lastState;
        
        // represent the direction and speed the player wants to move along in each frame
        private Vector3 _requestedMovement;
        private Quaternion _requestedRotation;
        
        // is the jump button pressed in this frame?
        private bool _requestedJump;
        
        // holds the last time the jump button was pressed
        private float _lastRequestedJumpTime;
        
        // is the Crouch button pressed in this frame?
        private bool _requestedCrouch;
        
        private float _lastRequestedCrouchTimeInAir;
        
        // is the Dash button pressed in this frame?
        private bool _requestedDash;
        
        // holds the last time the player dashed
        private float _lastDashTime;
        
        // is the player currently dashing?
        private bool _isDashing;
        
        // is the attack button pressed in this frame?
        private bool _requestedAttack;
        
        // holds the last time the character was grounded
        private float _lastGroundedTime;
        
        private float _initialStandingHeight;
        
        private Quaternion _cameraRotation;
        public void Init()
        {
            _state = new CharacterState()
            {
                Stance = Stance.Stand
            };
            
            _lastState = _state;
            
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

            _cameraRotation = inputData.LookDirection;
            
            _requestedRotation = inputData.LookDirection;
            
            _requestedAttack = inputData.AttackInput || _requestedAttack;
            
            _requestedJump = inputData.JumpInput || _requestedJump;
            _requestedCrouch = inputData.CrouchInput;
            _requestedDash = inputData.DashInput || _requestedDash;
            
            // if the jump button was pressed in the last frame,set the _lastRequestedJumpTime to the current time
            _lastRequestedJumpTime = inputData.JumpInput ? Time.time : _lastRequestedJumpTime;
            
            // if the crouch button was pressed and the player is in the air set the _lastRequestedCrouchTimeInAir to the current time
            _lastRequestedCrouchTimeInAir = inputData.CrouchInput && !_state.IsGrounded ? Time.time : _lastRequestedCrouchTimeInAir;
            
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
            else if(_state.Stance is Stance.Crouch or Stance.Slide)
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
                
                // project the requested movement on the ground normal 
                Vector3 targetProjectedDirection = motor.GetDirectionTangentToSurface
                (
                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * _requestedMovement.magnitude;
                
                // start Slide
                if (_state.Stance == Stance.Crouch && 
                    currentVelocity.magnitude > neededSpeedToSlide)
                {
                    _lastState = _state;
                    _state.Stance = Stance.Slide;

                    var speedBoost = targetProjectedDirection * startSlideSpeedBoost;
                    requestedVelocity += speedBoost;
                    currentVelocity = requestedVelocity;
                }
        
                // Move
                else if(_state.Stance == Stance.Stand || _state.Stance == Stance.Crouch)
                {
                    // determine the target speed based on the current stance
                    float normalTargetSpeed = _state.Stance == Stance.Stand ? walkSpeed : crouchSpeed;
                    // determine the target speed based if the player is dashing
                    float targetSpeed = _isDashing ? dashSpeed : normalTargetSpeed;
                    // Get the velocity that would result if the player would move to the projected direction
                    Vector3 targetVelocity =  targetProjectedDirection * targetSpeed;
                    // Make sure the player is not moving faster than the max walk speed
                    targetVelocity = Vector3.ClampMagnitude(targetVelocity, targetSpeed);
                    // change the acceleration based on if the current speed is lower than the target speed
                    float targetAcceleration = targetVelocity.magnitude > currentVelocity.magnitude ? groundAcceleration : groundDeceleration;
                    // if the speed is lower than the normal target speed, increase the acceleration
                    targetAcceleration = currentVelocity.magnitude < normalTargetSpeed ? targetAcceleration * 3f : targetAcceleration;
                    // Lerp to the target velocity
                    requestedVelocity = Vector3.Lerp
                    (
                        requestedVelocity,
                        targetVelocity,
                        1f - Mathf.Exp(-targetAcceleration * deltaTime)
                    );
                    
                    // Set the player's velocity to the new calculated velocity
                    currentVelocity = requestedVelocity;
                }
                // Sliding
                else
                {
                    currentVelocity -= currentVelocity * (groundFrictionWhenSliding * deltaTime);
                    var Force = Vector3.ProjectOnPlane
                    (
                        -motor.CharacterUp,
                        motor.GroundingStatus.GroundNormal
                    ) * slideGravity;
                    
                    currentVelocity -= Force * deltaTime;

                    if (currentVelocity.magnitude < SpeedToStopSlide || _requestedCrouch == false)
                    {
                        _lastState.Stance = _state.Stance;
                        _state.Stance = Stance.Crouch;
                    }

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
                    float targetAcceleration = targetSpeed > airSpeed ? airAcceleration : airDeceleration;
                    // Lerp to the target velocity
                    requestedVelocity = Vector3.Lerp
                    (
                        a: requestedVelocity, 
                        b: targetVelocity, 
                        t: 1f - Mathf.Exp(-targetAcceleration * deltaTime)
                    );
                    
                    
                    requestedVelocity.y = currentVelocity.y;
                    currentVelocity = requestedVelocity;
                    
                }
                
                // Gravity
                {
                    float effectiveGravity = _isDashing ? dashGravityScale * gravity : gravity;
                    requestedVelocity += new Vector3(0,effectiveGravity,0) * deltaTime;
                    // Set the player's velocity to the new calculated velocity
                    currentVelocity = requestedVelocity;
                }
            }
            
            // jump
            if (_requestedJump && (motor.GroundingStatus.IsStableOnGround || Time.time < _lastGroundedTime + coyoteTime))
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
                    // play the white dash particles
                    dashWhiteLinesParticleSystem.Play();
                    
                    _isDashing = true;
                    _requestedDash = false;
                    
                    _lastDashTime = Time.time;
                    Vector3 dashVelocity = _requestedMovement.normalized * dashForce;
                    dashVelocity = Vector3.ProjectOnPlane
                    (
                        vector: dashVelocity,
                        planeNormal: motor.CharacterUp
                    ).normalized * dashVelocity.magnitude;
                        
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
            
            if(_state.IsGrounded)
                _lastGroundedTime = Time.time;
            
            
            // enter the Crouch state
            if (_requestedCrouch && _state.Stance == Stance.Stand)
            {
                _lastState.Stance = _state.Stance;
                _state.Stance = Stance.Crouch;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: _initialStandingHeight - crouchCapsuleHeightOffset,
                    yOffset: (_initialStandingHeight - crouchCapsuleHeightOffset)*0.5f
                );
            }
            // i call it here becase i want the character to update its attack state before its calculated its Velocity
            UpdateCombat();

        }
        RaycastHit[] hits = new RaycastHit[8];
        
        private void UpdateCombat()
        {
            if (_requestedAttack)
            {
                _requestedAttack = false;
                Physics.Raycast(cameraTarget.position,_cameraRotation * Vector3.forward,out hits[0],attackRange,collisionLayers);
                
                Color color = hits[0].collider == null ? Color.red : Color.green;
                Debug.DrawLine(cameraTarget.position,cameraTarget.position + (_cameraRotation * Vector3.forward) * attackRange,color,100);
                
            }
        }

        

        public void PostGroundingUpdate(float deltaTime)
        {
            if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
                _state.Stance = Stance.Crouch;
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            if (!_requestedCrouch && _state.Stance is Stance.Crouch or Stance.Slide)
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
                    _lastState = _state;
                    _state.Stance = Stance.Stand;
                }


            }
            if(Time.time > _lastRequestedJumpTime + coyoteTime)
                _requestedJump = false;
            
            velocityText.text = "Speed: " + GetCurrentVelocity().magnitude.ToString("F2");
            
            stateText.color = _state.Stance switch
            {
                Stance.Stand => Color.cyan,
                Stance.Crouch => Color.yellow,
                Stance.Slide => Color.magenta,
                _ => Color.white
            };
            stateText.text = _state.Stance.ToString(); 
            
            GroundedText.color = _state.IsGrounded ? Color.green : Color.red;
            GroundedText.text = _state.IsGrounded ? "Grounded" : "Not Grounded";
            
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