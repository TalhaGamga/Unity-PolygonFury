using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInputHandler : MonoBehaviour
{
    public BehaviorSubject<InputSnapshot> InputSnapshotStream { get; }
        = new BehaviorSubject<InputSnapshot>(InputSnapshot.Empty);

    private readonly Dictionary<PlayerAction, InputSignal> _currentInputs = new();
    private readonly Dictionary<PlayerAction, InputSignal> _previousInputs = new();
    private InputSnapshot _lastSnapshot;

    [SerializeField] private InputReader _input;

    private void Start()
    {
        _input.Move += direction =>
        {
            HandleInput(SystemType.Movement, PlayerAction.Move, direction.magnitude > 0, direction);
        };

        _input.Jump += () =>
        {
            HandleInput(SystemType.Movement, PlayerAction.Jump);
        };

        _input.JumpCancel += () =>
        {
            HandleInput(SystemType.Movement, PlayerAction.JumpCancel);
        };

        _input.Dash += () =>
        {
            HandleInput(SystemType.Movement, PlayerAction.Dash);
        };

        _input.Attack += () =>
        {
            HandleInput(SystemType.Combat, PlayerAction.Attack);
        };

        _input.AttackCancel += () =>
        {
            HandleInput(SystemType.Combat, PlayerAction.Idle);
        };

        _input.MouseDrag += position =>
        {
            HandleInput(SystemType.Combat, PlayerAction.MouseDrag, (position.magnitude > 0), position);
        };

        _input.Reload += () =>
        {
            HandleInput(SystemType.Combat, PlayerAction.Reload);
        };

        _input.Enable();
    }

    private void HandleInput(SystemType system, PlayerAction action, bool isHeld = true, object value = default)
    {
        var behavior = InputBehaviorMap.Behavior.TryGetValue(action, out var b) ? b : InputBehavior.Eventful;

        bool wasHeld = _currentInputs.TryGetValue(action, out var prevInput) && prevInput.IsHeld;

        var input = new InputSignal
        {
            System = system,
            Action = action,
            IsHeld = isHeld,
            Value = value,
            WasPresseedThisFrame = false
        };

        if (behavior == InputBehavior.Eventful)
        {
            input.WasPresseedThisFrame = isHeld && !wasHeld;
            input.IsHeld = input.WasPresseedThisFrame;

            if (input.WasPresseedThisFrame)
            {
                UpdateInput(action, input);

                StartCoroutine(RemoveEventfulInputNextFrame(action));
            }
        }
        else
        {
            input.WasPresseedThisFrame = isHeld && !wasHeld;
            UpdateInput(action, input);
        }
    }

    private System.Collections.IEnumerator RemoveEventfulInputNextFrame(PlayerAction action)
    {
        yield return null;

        if (_currentInputs.ContainsKey(action))
            _currentInputs.Remove(action);

        var newSnapshot = new InputSnapshot
        {
            CurrentInputs = new Dictionary<PlayerAction, InputSignal>(_currentInputs),
            TimeStamp = Time.time
        };

        if (!InputSnapshotEquals(_lastSnapshot, newSnapshot))
        {
            _lastSnapshot = newSnapshot;
            InputSnapshotStream.OnNext(newSnapshot);
        }
    }


    private void UpdateInput(PlayerAction action, InputSignal newInput)
    {
        _previousInputs[action] = _currentInputs.TryGetValue(action, out var prev) ? prev : default;

        bool changed = !_currentInputs.TryGetValue(action, out var prevInput) || !InputEquals(prevInput, newInput);

        if (changed)
        {
            _currentInputs[action] = newInput;

            var newSnapshot = new InputSnapshot
            {
                CurrentInputs = new Dictionary<PlayerAction, InputSignal>(_currentInputs),
                TimeStamp = Time.time
            };

            if (!InputSnapshotEquals(_lastSnapshot, newSnapshot))
            {
                _lastSnapshot = newSnapshot;
                InputSnapshotStream.OnNext(newSnapshot);
            }
        }
    }

    private bool InputSnapshotEquals(InputSnapshot a, InputSnapshot b)
    {
        if (a.CurrentInputs == null && b.CurrentInputs == null) return true;
        if (a.CurrentInputs == null || b.CurrentInputs == null) return false;
        if (a.CurrentInputs.Count != b.CurrentInputs.Count) return false;

        foreach (var kvp in a.CurrentInputs)
        {
            if (!b.CurrentInputs.TryGetValue(kvp.Key, out var other))
                return false;
            if (!InputEquals(kvp.Value, other))
                return false;
        }
        return true;
    }

    private bool InputEquals(InputSignal a, InputSignal b)
    {
        return a.IsHeld == b.IsHeld &&
               a.Value == b.Value &&
               //a.Direction == b.Direction &&
               a.WasPresseedThisFrame == b.WasPresseedThisFrame;
    }
}