using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    private IMachine _machine;

    private void Awake()
    {
        SetCombatMachine(GetComponentInChildren<IWeapon>());
    }

    public void SetCombatMachine(IWeapon newWeapon)
    {
        _machine?.End();
        _machine = newWeapon.CreateCombat(this);
        _machine?.Init();
    }

    public void HandleInput(InputSignal inputSignal)
    {
        _machine?.HandleInput(inputSignal);
    }

    private void Update()
    {
        _machine?.Update();
    }
}