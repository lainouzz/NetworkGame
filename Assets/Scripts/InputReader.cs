using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input reader")]
public class InputReader : ScriptableObject, GameInput.IMovePlayerActions
{
    private GameInput gameInput;
    
    public event UnityAction<Vector2> MoveEvent = delegate {  };
    public event UnityAction ShootEvent = delegate { };

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootEvent.Invoke();
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new GameInput();
            gameInput.MovePlayer.SetCallbacks(this);
            gameInput.MovePlayer.Enable();
        }
    }
}
