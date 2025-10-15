using R3;
using UnityEngine;

public class TriangleBossInputHandlerAI : MonoBehaviour, IInputHandler
{
    public BehaviorSubject<InputSnapshot> InputSnapshotStream { get; }

    [SerializeField] private TriangleBossInputReaderSO _input;


}