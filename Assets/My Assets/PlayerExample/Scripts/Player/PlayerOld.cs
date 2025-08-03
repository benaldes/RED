
using System;
using __My_Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerOld : MonoBehaviour
{
    [FormerlySerializedAs("playerCharacter")] [SerializeField] public PlayerCharacterOld playerCharacterOld;
    [FormerlySerializedAs("playerCamera")] [SerializeField] private PlayerCameraOld playerCameraOld;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    
    private PlayerInputActions _playerInputActions;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();
        
        playerCharacterOld.Init();
        playerCameraOld.Init(playerCharacterOld.GetCameraTarget());
        cameraSpring.Init();
        cameraLean.Init();

    }

    private void OnDestroy()
    {
        _playerInputActions.Dispose();
    }

    private void Update()
    {
        var input = _playerInputActions.Gameplay;
        var deltaTime = Time.deltaTime;
        
        var cameraInput = new CameraInput {Look = input.Look.ReadValue<Vector2>()};
        playerCameraOld.UpdateRotation(cameraInput);
        
        var characterInput = new CharacterInputOld
        {
            Rotation    = playerCameraOld.transform.rotation,
            Move        = input.Move.ReadValue<Vector2>(),
            Jump        = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch      = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle :CrouchInput.None
        };
        playerCharacterOld.UpdateInput(characterInput);
        playerCharacterOld.UpdateBody(deltaTime);
        
        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCameraOld.transform.position, playerCameraOld.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacterOld.GetCameraTarget();
        var state = playerCharacterOld.GetState();
        
        playerCameraOld.UpdatePosition(playerCharacterOld.GetCameraTarget());
        cameraSpring.UpdateSpring(deltaTime,cameraTarget.up);
        cameraLean.UpdateLean(deltaTime,state.Stance is Stance.Slide,state.Acceleration,cameraTarget.up);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacterOld.SetPosition(position);
    }
}
