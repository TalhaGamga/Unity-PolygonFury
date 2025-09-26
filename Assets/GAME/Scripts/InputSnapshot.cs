using System.Collections.Generic;

public struct InputSnapshot
{
    public IReadOnlyDictionary<PlayerAction, InputSignal> CurrentInputs;
    public float TimeStamp;

    public static InputSnapshot Empty => new InputSnapshot
    {
        CurrentInputs = new Dictionary<PlayerAction, InputSignal>(),
        TimeStamp = 0f
    };
}