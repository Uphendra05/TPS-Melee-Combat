using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputHandler : MonoBehaviour
{
    public Vector2 move { get; private set; }

    public bool isJumpPressed { get; private set; } 
    public bool isDashPressed { get; private set; } 
    public bool isSprintPressed { get; private set; }


    private PlayerInputSystem inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputSystem();
        inputActions.Player.Enable();

        inputActions.Player.Jump.started += OnJump;
        inputActions.Player.Jump.canceled += OnJump;

        inputActions.Player.Movement.performed += OnMove;


        inputActions.Player.Dash.started += OnDash;
        inputActions.Player.Dash.canceled += OnDash;

        inputActions.Player.Sprint.started += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;

    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        isSprintPressed = context.ReadValueAsButton();
        Debug.Log("Sprint Pressed " + context.phase);
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        isDashPressed = context.ReadValueAsButton();
        Debug.Log("Dash Pressed " + context.phase);

    }

    private void OnMove(InputAction.CallbackContext context)
    {

        Debug.Log(context);
        move = context.ReadValue<Vector2>();    
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        Debug.Log("Jump Pressed " + context.phase);

    }




}
