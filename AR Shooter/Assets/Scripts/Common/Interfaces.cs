using System;
using System.Collections;
using Game.Mobs;
using TMPro;
using Game.Weapons;

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
    WeaponName WeaponName { get; }

    WeaponType WeaponType { get; }

    MainWeapon InstantiateWeapon(UnityEngine.Transform saveFolder, int index, TextMeshPro bulletsText);

    string Name { get; set; }
}

internal interface IGameEvent
{
    
}
