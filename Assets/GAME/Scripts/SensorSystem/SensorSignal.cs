using UnityEngine;

public struct SensorSignal
{
    public LayerMask Layer;
    public Transform Source;
    public Transform DetectedObject;
    public SensorType Type;
    public float Distance;
    public bool IsDetected;
    public bool WasDetectedThisFrame;
    public Vector2 HitPoint;
    public Vector2 HitNormal;
}