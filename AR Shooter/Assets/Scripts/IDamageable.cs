using System;
using System.Collections;
using Mobs;

public interface IDamageable
{
    delegate void Apply(int damage);
    
    event Action<MobStats> OnApplyDamage;
    
    void ApplyDamage(int damage);
    
    int HP { get; set; }
    
    bool IsAlive { get; }
    
    IEnumerator Death(int deathType);
}
