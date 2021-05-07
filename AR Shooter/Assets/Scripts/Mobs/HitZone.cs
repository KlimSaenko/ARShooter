using System.Collections;
using System.Collections.Generic;
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
            else if (transformWithController.TryGetComponent(out PlayerStatus playerStatus)) return playerStatus;
            else return null;
        }
    }

    internal enum ZoneType
    {
        Standart = 0,
        Critical
    }

    [SerializeField] internal ZoneType zoneType;

    public void ApplyDamage(int damage, Vector3 hitPos)
    {
        if (Damageable != null)
        {
            damage = zoneType == ZoneType.Critical ? damage * 3 : damage;
            UI.ActivateMarker((int)zoneType, hitPos);
            HitDecal.instance.NewDecal(hitPos, damage, zoneType);
            Damageable.ApplyDamage(damage);
        }
    }
    
    //public IEnumerator Death()
    //{
    //    return ((IDamageable)mainMob).Death();
    //}
}
