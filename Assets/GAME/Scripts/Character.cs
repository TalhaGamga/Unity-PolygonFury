using UnityEngine;
using R3;

public class Character : MonoBehaviour
{
    [SerializeField] private MovementSystem _movementSystem;
    [SerializeField] private CombatSystem _combatSystem;
    private IInputHandler _inputHandler;

    private InputSnapshot _currentInputSnapshot = InputSnapshot.Empty;

    private CompositeDisposable _disposables = new();

    private void Awake()
    {
        _inputHandler = GetComponent<IInputHandler>();

        if (_inputHandler != null)
        {
            _inputHandler.InputSnapshotStream.Subscribe(snapshot =>
            {
                dispatchInputsToSystems(snapshot);
                _currentInputSnapshot = snapshot;
            }).AddTo(_disposables);
        }

        _movementSystem.TransitionStream.Subscribe((Unit) => dispatchInputsToSystems()).AddTo(_disposables);
        _combatSystem.TransitionStream.Subscribe((Unit) => dispatchInputsToSystems()).AddTo(_disposables);
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

    private void dispatchInputsToSystems()
    {
        dispatchInputsToSystems(_currentInputSnapshot);
    }
}