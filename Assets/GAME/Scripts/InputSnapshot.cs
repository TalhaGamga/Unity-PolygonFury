using System.Collections.Generic;

public struct InputSnapshot
{
    public IReadOnlyDictionary<CharacterAction, InputSignal> CurrentInputs;
    public float TimeStamp;

    public static InputSnapshot Empty => new InputSnapshot
    {
        CurrentInputs = new Dictionary<CharacterAction, InputSignal>(),
        TimeStamp = 0f
    };
}