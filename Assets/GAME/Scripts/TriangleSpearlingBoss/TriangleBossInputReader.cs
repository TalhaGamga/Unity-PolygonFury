using UnityEngine;
using UnityEngine.Events;

public class TriangleBossInputReader : InputReaderBase, IInputReader
{
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction Attack = delegate { };
    public event UnityAction Reload = delegate { };

    public override void Enable()
    {
    }


}