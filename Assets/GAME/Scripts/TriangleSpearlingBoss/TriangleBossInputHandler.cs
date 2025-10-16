using R3;
using System.Collections.Generic;
using UnityEngine;

public class TriangleBossInputHandler : MonoBehaviour, IInputHandler
{
    public BehaviorSubject<InputSnapshot> InputSnapshotStream { get; }
    = new BehaviorSubject<InputSnapshot>(InputSnapshot.Empty);

    [SerializeField] private SensorSystem _sensorSystem;

    private readonly Dictionary<CharacterAction, InputSignal> _currentInputs = new();
    private readonly Dictionary<CharacterAction, InputSignal> _previousInputs = new();
    private InputSnapshot _lastSnapshot;

    private CompositeDisposable _disposables = new();

    private void Awake()
    {
        _sensorSystem.SensorSnapshotStream.Subscribe(dispatchSensorSignals).AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    private void handleInput(SystemType system, CharacterAction action, bool isHeld = true, object value = default)
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

    private System.Collections.IEnumerator RemoveEventfulInputNextFrame(CharacterAction action)
    {
        yield return null;

        if (_currentInputs.ContainsKey(action))
            _currentInputs.Remove(action);

        var newSnapshot = new InputSnapshot
        {
            CurrentInputs = new Dictionary<CharacterAction, InputSignal>(_currentInputs),
            TimeStamp = Time.time
        };

        if (!InputSnapshotEquals(_lastSnapshot, newSnapshot))
        {
            _lastSnapshot = newSnapshot;
            InputSnapshotStream.OnNext(newSnapshot);
        }
    }


    private void UpdateInput(CharacterAction action, InputSignal newInput)
    {
        _previousInputs[action] = _currentInputs.TryGetValue(action, out var prev) ? prev : default;

        bool changed = !_currentInputs.TryGetValue(action, out var prevInput) || !InputEquals(prevInput, newInput);

        if (changed)
        {
            _currentInputs[action] = newInput;

            var newSnapshot = new InputSnapshot
            {
                CurrentInputs = new Dictionary<CharacterAction, InputSignal>(_currentInputs),
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

    private void dispatchSensorSignals(SensorSnapshot sensorSnapshot)
    {
        foreach (var kvp in sensorSnapshot.CurrentSignals)
        {
            var signalType = kvp.Key;
            var signal = kvp.Value;

            switch (signalType)
            {
                case SensorType.Square:
                    handleSquareSensor(signal);
                    break;

                case SensorType.Platform:
                    handlePlatformSensor(signal);
                    break;

                default:
                    break;
            }
        }
    }


    private void handlePlatformSensor(SensorSignal sensorSignal)
    {
        // handle movement sensor
    }

    private void handleSquareSensor(SensorSignal sensorSignal)
    {
        // handle attack sensor
    }
}