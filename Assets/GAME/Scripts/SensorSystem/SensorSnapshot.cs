using System.Collections.Generic;

public struct SensorSnapshot
{
    public Dictionary<SensorType, SensorSignal> CurrentSignals;
    public float TimeStamp;

    public static readonly SensorSnapshot Empty = new SensorSnapshot
    {
        CurrentSignals = new Dictionary<SensorType, SensorSignal>(),
        TimeStamp = 0f
    };
}