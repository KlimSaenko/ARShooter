using System;
using System.Collections;
using TMPro;
using Game.Weapons;
using UnityEngine;

public interface IDamageable
{
    delegate void Apply(int damage);
    
    void ApplyDamage(int damage);
    
    int HP { get; set; }
    
    bool IsAlive { get; }
    
    IEnumerator Death(int deathType);
}

public interface IWeapon
{
    Weapon WeaponConfig { get; }

    GameObject InstantiateWeapon(Transform saveFolder, int index, TextMeshPro bulletsText = null);
}

internal interface IGameEvent
{
    
}
