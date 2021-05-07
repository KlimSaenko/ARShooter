using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(int damage);
    int HP { get; set; }
    bool IsAlive { get; }
    IEnumerator Death(int deathType);
}
