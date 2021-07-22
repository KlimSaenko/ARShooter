using System.Collections;
using System.Collections.Generic;
using Mobs;
using Player;
using UnityEngine;

public class HitZone : MonoBehaviour
{
    [SerializeField] private Transform transformWithController;
    //[SerializeField] private MainMob mainMob;
    //[SerializeField] private PlayerStatus playerStatus;

    private IDamageable Damageable
    {
        get
        {
            if (transformWithController.TryGetComponent(out MainMob mainMob)) return mainMob;
            return transformWithController.TryGetComponent(out PlayerStatus playerStatus) ? playerStatus : null;
        }
    }

    internal enum ZoneType
    {
        Standard = 0,
        Critical
    }

    [SerializeField] internal ZoneType zoneType;

    public void ApplyDamage(int damage, Vector3 hitPos)
    {
        if (Damageable == null) return;
        
        damage = zoneType == ZoneType.Critical ? damage * 3 : damage;
        UI.ActivateMarker((int)zoneType, hitPos);
        HitDecal.instance.NewDecal(hitPos, damage, zoneType);
        Damageable.ApplyDamage(damage);
    }
    
    //public IEnumerator Death()
    //{
    //    return ((IDamageable)mainMob).Death();
    //}
}
