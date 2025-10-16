using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorSystem : MonoBehaviour
{
    public BehaviorSubject<SensorSnapshot> SensorSnapshotStream { get; }
    = new BehaviorSubject<SensorSnapshot>(SensorSnapshot.Empty);

    private readonly Dictionary<SensorType, SensorSignal> _currentSignals = new();
    private readonly Dictionary<SensorType, SensorSignal> _previousSignals = new();
    private SensorSnapshot _lastSnapshot;

    [SerializeField] private List<SensorDefinition> _sensorDefinitions;

    public SensorSystem(List<SensorDefinition> sensors)
    {
        _sensorDefinitions = sensors;
    }

    private void OnDrawGizmos()
    {
        if (_sensorDefinitions == null || _sensorDefinitions.Count == 0)
            return;

        foreach (var def in _sensorDefinitions)
        {
            if (def.Origin == null || !def.DrawDebug)
                continue;

            Gizmos.color = new Color(def.DebugColor.r, def.DebugColor.g, def.DebugColor.b, 0.25f);
            Gizmos.DrawSphere(def.Origin.position, def.Range);

            Gizmos.color = def.DebugColor;
            int segments = 32;
            Vector3 prevPoint = def.Origin.position + (Vector3)(Vector2.right * def.Range);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 nextPoint = def.Origin.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * def.Range;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            if (Application.isPlaying &&
                _currentSignals.TryGetValue(def.Type, out var signal) &&
                signal.IsDetected &&
                signal.DetectedObject != null)
            {
                Gizmos.color = def.DebugColor;

                Gizmos.DrawLine(def.Origin.position, signal.HitPoint);
                Gizmos.DrawSphere(signal.HitPoint, 0.1f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(signal.HitPoint, signal.HitPoint + signal.HitNormal * 0.4f);
            }

        }
    }

    public void Tick()
    {
        foreach (var def in _sensorDefinitions)
        {
            RaycastHit2D hitInfo = Physics2D.CircleCast(
                def.Origin.position,
                0.1f,
                def.Origin.right,
                def.Range,
                def.Layer
                );

            bool isDetected = hitInfo.collider != null;

            var signal = new SensorSignal
            {
                Layer = def.Layer,
                Source = def.Origin,
                DetectedObject = isDetected ? hitInfo.collider.transform : null,
                Type = def.Type,
                Distance = isDetected ? hitInfo.distance : Mathf.Infinity,
                IsDetected = isDetected,
                WasDetectedThisFrame = false,
                HitPoint = isDetected ? hitInfo.point : Vector2.zero,
                HitNormal = isDetected ? hitInfo.normal : Vector2.zero
            };

            HandleSensor(def.Type, signal, def.Behavior);
        }
    }

    private void HandleSensor(SensorType type, SensorSignal signal, SensorBehavior behavior)
    {
        bool wasDetected = _currentSignals.TryGetValue(type, out var prev) && prev.IsDetected;

        if (behavior == SensorBehavior.Eventful)
        {
            signal.WasDetectedThisFrame = signal.IsDetected && !wasDetected;
            signal.IsDetected = signal.WasDetectedThisFrame;

            if (signal.WasDetectedThisFrame)
            {
                updateSensor(type, signal);
                StartCoroutine(removeEventfulNextFrame(type));
            }
        }
        else
        {
            signal.WasDetectedThisFrame = signal.IsDetected && !wasDetected;
            updateSensor(type, signal);
        }
    }

    private IEnumerator removeEventfulNextFrame(SensorType type)
    {
        yield return null;
        if (_currentSignals.ContainsKey(type))
            _currentSignals.Remove(type);
        publishSnapshot();
    }

    private void updateSensor(SensorType type, SensorSignal newSignal)
    {
        _previousSignals[type] = _currentSignals.TryGetValue(type, out var prev) ? prev : default;
        bool changed = !_currentSignals.TryGetValue(type, out var prevSignal) || !isSensorEquals(prevSignal, newSignal);

        if (changed)
        {
            _currentSignals[type] = newSignal;
            publishSnapshot();
        }
    }

    private void publishSnapshot()
    {
        var snapshot = new SensorSnapshot
        {
            CurrentSignals = new Dictionary<SensorType, SensorSignal>(_currentSignals),
            TimeStamp = Time.time
        };

        if (!isSnapshotsEqual(_lastSnapshot, snapshot))
        {
            _lastSnapshot = snapshot;
            SensorSnapshotStream.OnNext(snapshot);
        }
    }

    private bool isSnapshotsEqual(SensorSnapshot a, SensorSnapshot b)
    {
        if (a.CurrentSignals == null && b.CurrentSignals == null) return true;
        if (a.CurrentSignals == null || b.CurrentSignals == null) return false;
        if (a.CurrentSignals.Count != b.CurrentSignals.Count) return false;

        foreach (var kvp in a.CurrentSignals)
        {
            if (!b.CurrentSignals.TryGetValue(kvp.Key, out var other))
                return false;
            if (!isSensorEquals(kvp.Value, other))
                return false;
        }

        return true;
    }

    private bool isSensorEquals(SensorSignal a, SensorSignal b)
    {
        return a.IsDetected == b.IsDetected &&
            a.DetectedObject == b.DetectedObject &&
            Mathf.Approximately(a.Distance, b.Distance) &&
            a.WasDetectedThisFrame == b.WasDetectedThisFrame;
    }
}