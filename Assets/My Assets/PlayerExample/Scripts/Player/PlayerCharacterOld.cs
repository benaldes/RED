using System.Numerics;
using KinematicCharacterController;
using TMPro;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerCharacterOld : MonoBehaviour, ICharacterController
{

    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private TMP_Text groundStateText;
    [SerializeField] private TMP_Text StanceText;
    [SerializeField] private TMP_Text VelocityText;
    
    [Space]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float walkResponse;
    [SerializeField] private float crouchResponse;
    
    [Space]
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float gravity;
    [Range(0f, 1f), SerializeField] private float gravityWhileJumpIsHeldModifier;

    [Header("Slide")] 
    [SerializeField] private float slideStartSpeed;
    [SerializeField] private float slideEndSpeed;
    [SerializeField] private float slideFriction;
    [SerializeField] private float slideSteerAcceleration;
    [SerializeField] private float slideGravity;
    
    [Space]
    [SerializeField] private float airSpeed;
    [SerializeField] private float airAcceleration;
    
    [Space]
    [SerializeField] private float standHeight;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchHeightResponse;
    
    [Space]
    [Range(0f,1f),SerializeField] private float standingCameraTargetHeight;
    [Range(0f,1f),SerializeField] private float crouchingCameraTargetHeight;

    

    private CharacterStateOld _stateOld;
    private CharacterStateOld _lastStateOld;
    private CharacterStateOld _tempStateOld;
    
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _ungroundedDueToJump;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    
    private Collider[] _uncrouchOverlapResults;
    
    public Transform GetCameraTarget() => cameraTarget;
    public void Init()
    {
        _stateOld.Stance = Stance.Stand;
        _lastStateOld = _stateOld;
        motor.CharacterController = this;
        
        _uncrouchOverlapResults = new Collider[8];
    }

    public void UpdateInput(CharacterInputOld inputOld)
    {
        _requestedRotation = inputOld.Rotation;
        _requestedMovement = new Vector3(inputOld.Move.x,0f, inputOld.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement,1f);
        _requestedMovement = inputOld.Rotation * _requestedMovement;
        
        var _wasRequestedJump = _requestedJump;
        _requestedJump = _requestedJump || inputOld.Jump;
        if (_requestedJump && !_wasRequestedJump)
            _timeSinceJumpRequest = 0;
        _requestedSustainedJump = inputOld.JumpSustain;
        
        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = inputOld.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch, 
            _=> _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_stateOld.Grounded;
        else if (!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;

        groundStateText.text = "Grounded: " +_stateOld.Grounded.ToString();
        StanceText.text = "Stance: " +_stateOld.Stance.ToString();
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;
        
        var cameraTargetHeight = currentHeight * 
        (
            _stateOld.Stance is Stance.Stand 
                 ? standingCameraTargetHeight 
                 : crouchingCameraTargetHeight
        );

        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp
        (
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime) // it makes the animation framerate independent because of math i don't understand
        );
        root.localScale = Vector3.Lerp
        (
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime) // it makes the animation framerate independent because of math i don't understand
        );
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if(forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _stateOld.Acceleration = Vector3.zero;
        
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0;
            _ungroundedDueToJump = false;
            
            // snap the requested movement direction to the angle of the ground normal the character is standing on
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            // start slide
            var moving = groundedMovement.sqrMagnitude > 0f;
            var crouching = _stateOld.Stance is Stance.Crouch;
            var wasStanding = _lastStateOld.Stance is Stance.Stand;
            var wasInAir = !_lastStateOld.Grounded;
            if (moving && crouching && (wasStanding || wasInAir))
            {
                _stateOld.Stance = Stance.Slide;

                if (wasInAir)
                {
                    currentVelocity = Vector3.ProjectOnPlane
                    (
                        vector: _lastStateOld.Velocity,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    );
                }

                var effectiveSlideStartSpeed = slideStartSpeed;
                if (!_lastStateOld.Grounded && !_requestedCrouchInAir)
                {
                    effectiveSlideStartSpeed = 0;
                    _requestedCrouchInAir = false;
                }
                
                var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                currentVelocity = motor.GetDirectionTangentToSurface
                (
                    direction: currentVelocity,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * slideSpeed;
            }
            
            // Move
            if (_stateOld.Stance is Stance.Stand or Stance.Crouch)
            {
                var speed = _stateOld.Stance is Stance.Stand ? walkSpeed : crouchSpeed; 
                var response = _stateOld.Stance is Stance.Stand ? walkResponse : crouchResponse;
            
                var targetVelocity = groundedMovement * speed;

                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
                _stateOld.Acceleration = moveVelocity - currentVelocity;
                currentVelocity = moveVelocity;
            }
            // player is sliding
            else
            {
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                // slide based on ground angle
                {
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;
                    
                    currentVelocity -= force * deltaTime;
                }
                
                
                // Steer
                {
                    // target velocity is the players movement direction, at the current speed
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentSpeed;
                    var steerVelocity = currentVelocity;
                    var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;
                    // add steer force, but clamp velocity so the slide speed doesn't exceed current speed
                    steerVelocity += steerForce;
                    steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                    _stateOld.Acceleration = (steerVelocity - currentVelocity) / deltaTime;

                    currentVelocity = steerVelocity;
                }
                
                // stop player slide if velocity is too low
                if (currentVelocity.magnitude < slideEndSpeed)
                    _stateOld.Stance = Stance.Crouch;

            }
           
        }
        // player in the air
        else
        {
            _timeSinceUngrounded += deltaTime;
            // Move 
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                var planerMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.magnitude;
                
                var currentPlanerVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity, 
                    planeNormal: motor.CharacterUp
                );
                
                var movementForce = planerMovement * (airAcceleration * deltaTime);
                
                // if moving slower than the max air speed, treat movementForce as simple steering force.
                if (currentPlanerVelocity.magnitude < airSpeed)
                {
                    var targetPlanerVelocity = currentPlanerVelocity + movementForce;
                    // limit target velocity to the air speed
                    targetPlanerVelocity = Vector3.ClampMagnitude(targetPlanerVelocity, airSpeed);
                    // steer towards the target velocity
                    currentVelocity += targetPlanerVelocity - currentPlanerVelocity;
                }
                else if (Vector3.Dot(currentPlanerVelocity, movementForce) > 0f)
                {
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanerVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }
                
                // Prevent going up steep slopes
                if (motor.GroundingStatus.FoundAnyGround)
                {
                        
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal)
                        ).normalized;
                            
                        // project movement force onto the sliding plane
                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }
                
                currentVelocity += movementForce;
            }
            
           
            
            // Gravity
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if(_requestedSustainedJump && verticalSpeed > 0f)
                effectiveGravity *= gravityWhileJumpIsHeldModifier;
                
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if (_requestedJump)
        {
            bool grounded = motor.GroundingStatus.IsStableOnGround;
            bool canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;

            if (grounded || canCoyoteJump)
            {
                _requestedJump = false;
                _requestedCrouch = false;
                _requestedCrouchInAir = false;
            
                // unstick the player from the ground
                motor.ForceUnground(time: 0f);

                _ungroundedDueToJump = true;

                // Set minimum vertical speed to the jump speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                _timeSinceJumpRequest += deltaTime;

                var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                _requestedJump = canJumpLater;
            }
            
        }
        
        VelocityText.text = motor.Velocity.magnitude.ToString();
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempStateOld = _stateOld;
        
        if (_requestedCrouch && _stateOld.Stance is Stance.Stand)
        {
            _stateOld.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius : motor.Capsule.radius,
                height : crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _stateOld.Stance is Stance.Slide)
            _stateOld.Stance = Stance.Crouch;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _stateOld.Stance is not Stance.Stand)
        {
            // make the player capsule stand up
            motor.SetCapsuleDimensions
            (
                radius : motor.Capsule.radius, 
                height : standHeight, 
                yOffset: standHeight * 0.5f
            );

            // check if the player is colliding with anything above them 
            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults,mask, QueryTriggerInteraction.Ignore) > 0)
            {
                // return the player to the crouching height
                _requestedCrouch = true;
                motor.SetCapsuleDimensions
                (
                    radius : motor.Capsule.radius, 
                    height : crouchHeight, 
                    yOffset: crouchHeight * 0.5f
                );
            }
            else
            {
                _stateOld.Stance = Stance.Stand;
            }
        }
        
        _stateOld.Grounded = motor.GroundingStatus.IsStableOnGround;
        _stateOld.Velocity = motor.Velocity;
        _lastStateOld = _tempStateOld;
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

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
            motor.BaseVelocity = Vector3.zero;
    }

    public CharacterStateOld GetState() => _stateOld;
    public CharacterStateOld GetLastState() => _lastStateOld;
}

public struct CharacterInputOld
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}

public struct CharacterStateOld
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}
public enum Stance
{
    Stand,
    Crouch,
    Slide
}

public enum CrouchInput
{
    None,
    Toggle,
}
