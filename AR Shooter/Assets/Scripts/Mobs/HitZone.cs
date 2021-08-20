using static Common.Pool;
using Player;
using UnityEngine;

namespace Mobs
{
    public class HitZone : MonoBehaviour
    {
        [SerializeField] private Transform transformWithController;

        private IDamageable _damageable;
        private IDamageable Damageable
        {
            get
            {
                if (_damageable is not null) return _damageable;
                
                if (transformWithController.TryGetComponent(out MainMob mainMob)) _damageable = mainMob;
                else
                {
                    _damageable = transformWithController.TryGetComponent(out PlayerStatus playerStatus) ? playerStatus : null;
                }

                return _damageable;
            }
        }

        internal enum ZoneType
        {
            None = 0,
            Standard,
            Critical
        }

        [SerializeField] internal ZoneType zoneType;

        internal void ApplyDamage(int damage, Vector3 hitPos)
        {
            damage = zoneType == ZoneType.Critical ? damage * 3 : damage;
            
            Decals.ActivateHitMarker(hitPos, damage, zoneType);

            Damageable?.ApplyDamage(damage);
        }

        internal void ApplyDamage(int damage)
        {
            damage = zoneType == ZoneType.Critical ? damage * 3 : damage;

            var hitPos = transform.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
            
            Decals.ActivateHitMarker(hitPos, damage, zoneType, false);
            
            Damageable?.ApplyDamage(damage);
        }
    }
}
