using UnityEngine;

public abstract class MoverMachineBaseSO : ScriptableObject
{
    protected abstract IMover Mover { get; }

    public IMover GetMover()
    {
        return Mover;
    }
}