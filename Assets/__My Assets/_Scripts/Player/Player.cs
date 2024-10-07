using System;
using __My_Assets._Scripts.Player;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private TMP_Text velocityText;
    
    private PlayerInput _playerInput;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
        
        playerCamera.Init();
        playerCharacter.Init();
    }
    
    private void OnDestroy()
    {
        _playerInput.Dispose();
    }

    private void Start()
    {
        // remove mouse from game
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        Vector2 rawMoveInput = _playerInput.Gameplay.Move.ReadValue<Vector2>();
        Vector2 rawLookInput = _playerInput.Gameplay.Look.ReadValue<Vector2>();

        playerCamera.UpdateRotation(rawLookInput);

        Quaternion currentLookDirection = playerCamera.transform.rotation;
        
        PlayerInputData playerInputData = new PlayerInputData()
        {
            MoveInput = rawMoveInput,
            LookDirection = currentLookDirection,
            JumpInput = _playerInput.Gameplay.Jump.WasPressedThisFrame(),
            CrouchInput = _playerInput.Gameplay.Crouch.IsPressed(),
            DashInput = _playerInput.Gameplay.Dash.WasPressedThisFrame()
        };
        
        playerCharacter.UpdateBody(deltaTime);
        playerCharacter.UpdateInput(playerInputData);
        
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        
        // I have to update the camera position here because the player need to finish updating
        // the character position before the camera or the camera will lag behind the player
        playerCamera.UpdatePosition();
        playerCamera.UpdateCameraFOV(playerCharacter.GetCurrentVelocity(), playerCharacter.GetWalkSpeed(),deltaTime);
        
        velocityText.text = "Speed: " + playerCharacter.GetCurrentVelocity().magnitude.ToString("F2");
    }
}

public struct PlayerInputData
{
    public Vector2 MoveInput;
    public Quaternion LookDirection;
    public bool JumpInput;
    public bool CrouchInput;
    public bool DashInput;
    
}

