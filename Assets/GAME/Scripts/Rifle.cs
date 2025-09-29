using UnityEngine;

public class Rifle : MonoBehaviour, IWeapon
{
    [SerializeField] private RifleCombat _rifleCombatMachine;

    public ICombat CreateCombat(CombatSystem combatSystem)
    {
        return _rifleCombatMachine;
    }
}