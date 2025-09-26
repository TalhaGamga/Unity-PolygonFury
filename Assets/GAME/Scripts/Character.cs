using UnityEngine;
using R3;

public class Character : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private MovementSystem _movementSystem;
    [SerializeField] private CombatSystem _combatSystem;

    private void Awake()
    {
        if (_inputHandler != null)
        {
            _inputHandler.InputSnapshotStream.Subscribe(snapshot =>
            {
                dispatchInputsToSystems(snapshot);
            });
        }
    }

    private void dispatchInputsToSystems(InputSnapshot inputSnapshot)
    {
        foreach (var inputSignal in inputSnapshot.CurrentInputs.Values)
        {
            switch (inputSignal.System)
            {
                case SystemType.Movement:
                    _movementSystem?.HandleInput(inputSignal);
                    break;

                case SystemType.Combat:
                    _combatSystem?.HandleInput(inputSignal);
                    break;
                default:
                    break;
            }
        }
    }
}