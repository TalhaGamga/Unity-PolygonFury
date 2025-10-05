using UnityEngine;

public class Spear : MonoBehaviour, IWeapon
{
    [SerializeField] private SpearCombat _combat;
    
    public ICombat CreateCombat(CombatSystem combatSystem)
    {
        return _combat;
    }
}