using UnityEngine;

public class AutomaticGun : MonoBehaviour, IWeapon
{
    [SerializeField] private AutomaticGunCombat _rifleCombat;
    [SerializeField] private HitscanBullet _hitscan;

    public ICombat CreateCombat(CombatSystem combatSystem)
    {
        return _rifleCombat;
    }
}