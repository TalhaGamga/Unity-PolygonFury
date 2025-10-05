using UnityEngine;

public class AutomaticGun : MonoBehaviour, IWeapon
{
    [SerializeField] private AutomaticGunCombat _combat;
    [SerializeField] private HitscanBullet _hitscan;

    public ICombat CreateCombat(CombatSystem combatSystem)
    {
        return _combat;
    }
}