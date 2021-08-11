using System;
using System.Collections;
using Mobs;
using UnityEngine;
using Weapons;

public interface IDamageable
{
    delegate void Apply(int damage);
    
    event Action<MobStats> OnApplyDamage;
    
    void ApplyDamage(int damage);
    
    int HP { get; set; }
    
    bool IsAlive { get; }
    
    IEnumerator Death(int deathType);
}

public interface IWeaponConfig
{
    Vector3 PosToAim { get; }
        
    Vector3 PosFromAim { get; }
        
    WeaponType WeaponType { get; }

    bool IsActive { get; }
    
    int BulletCount { get; set; }

    void SetActive(bool value);
}
