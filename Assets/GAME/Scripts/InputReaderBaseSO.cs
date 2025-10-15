using UnityEngine;

public abstract class InputReaderBaseSO<T> : ScriptableObject
{
    protected T InputReader { get; set; }

    public abstract T GetInputReader();
}