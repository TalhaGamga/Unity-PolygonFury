using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    private IMachine _machine;

    public void SetCombatMachine(IMachine newMachine)
    {
        _machine.End();
        _machine = newMachine;
        _machine.Init();
    }

    public void HandleInput(InputSignal inputSignal)
    {
        _machine.HandleInput(inputSignal);
    }

    private void Update()
    {
        _machine?.Update();
    }
}