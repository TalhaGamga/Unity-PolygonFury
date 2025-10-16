using UnityEngine;

[System.Serializable]
public struct SensorDefinition
{
    public string Name;
    public Transform Origin;
    public float Range;
    public LayerMask Layer;
    public SensorType Type;
    public SensorBehavior Behavior;

    [Header("Debug Settings")]
    public Color DebugColor;
    public bool DrawDebug;
}