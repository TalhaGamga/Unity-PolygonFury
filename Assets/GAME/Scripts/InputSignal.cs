using UnityEngine;
public struct InputSignal
{
    public SystemType System;
    public PlayerAction Action;
    public bool IsHeld;
    public bool WasPresseedThisFrame;
    public bool WasReleasedThisFrame;
    public Vector2 Direction;
    public object Value;
}