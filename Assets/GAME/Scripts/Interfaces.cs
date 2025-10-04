using R3;
using System;
using UnityEngine;

public interface IMachine
{
    public void Init(Subject<Unit> transitionStream);
    public void End();
    public void Update();
    public void HandleInput(InputSignal inputSignal);
}

public interface IMover : IMachine
{
    public void Construct(MovementSystem.ExternalSources externalSources);
}

public interface ICombat : IMachine
{
}

public interface IWeapon
{
    public ICombat CreateCombat(CombatSystem combatSystem);
}

public interface IBullet
{
    public event Action<BulletHitInfo> OnBulletHit;
    public void Fire(Vector3 origin, Vector3 direction, float range);
}

public interface IBulletVisualizer
{
    IBullet Bullet { get; }
}