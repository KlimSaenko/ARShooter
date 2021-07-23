using System.Collections;
using System.Collections.Generic;
using Common;
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
        damage = zoneType == ZoneType.Critical ? damage * 3 : damage;
        // UI.ActivateHitMarker((int)zoneType, hitPos);
        // HitDecal_Prev.instance.NewDecal(hitPos, damage, zoneType);
        Pool.Decals.ActivateHitMarker(hitPos, damage, zoneType);

        Damageable?.ApplyDamage(damage);
    }
}
