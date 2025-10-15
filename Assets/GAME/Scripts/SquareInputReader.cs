using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputs;

public interface IInputReader
{
    void Enable();
}

public class SquareInputReader : InputReaderBase, IPlayerActions
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction Jump = delegate { };
    public event UnityAction JumpCancel = delegate { };
    public event UnityAction Attack = delegate { };
    public event UnityAction AttackCancel = delegate { };
    public event UnityAction Dash = delegate { };
    public event UnityAction Reload = delegate { };
    public event UnityAction<Vector2> MouseDrag = delegate { };

    public Vector2 Direction => _playerInputs.Player.Move.ReadValue<Vector2>();
    public bool IsJumpKeyPressed => _playerInputs.Player.Jump.IsPressed();

    private PlayerInputs _playerInputs;

    public override void Enable()
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
                Jump.Invoke();
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
                Attack.Invoke();
                break;
            case InputActionPhase.Canceled:
                AttackCancel.Invoke();
                break;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Dash.Invoke();
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        Reload?.Invoke();
    }

    public void OnMousePoint(InputAction.CallbackContext context)
    {
        MouseDrag?.Invoke(context.ReadValue<Vector2>());
    }
}