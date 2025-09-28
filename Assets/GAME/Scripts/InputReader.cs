using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputs;

public interface IInputReader
{
    public Vector2 Direction { get; }
    void Enable();
}

[CreateAssetMenu(menuName = "ScriptableObjects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions, IInputReader
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction JumpCancel = delegate { };
    public event UnityAction<bool> Attack = delegate { };
    public event UnityAction<bool> Dash = delegate { };
    public event UnityAction Reload = delegate { };

    public Vector2 Direction => _playerInputs.Player.Move.ReadValue<Vector2>();
    public bool IsJumpKeyPressed => _playerInputs.Player.Jump.IsPressed();

    private PlayerInputs _playerInputs;

    public void Enable()
    {
        if (_playerInputs == null)
        {
            _playerInputs = new PlayerInputs();
            _playerInputs.Player.SetCallbacks(this);
        }

        _playerInputs.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started || context.canceled)
            Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                JumpCancel.Invoke();
                break;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Attack.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Attack.Invoke(false);
                break;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Dash.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Dash.Invoke(false);
                break;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        Reload?.Invoke();
    }
}