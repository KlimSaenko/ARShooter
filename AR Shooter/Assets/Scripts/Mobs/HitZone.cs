using static Common.Pool;
using Player;
using UnityEngine;

namespace Mobs
{
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
            
            Decals.ActivateHitMarker(hitPos, damage, zoneType);

            Damageable?.ApplyDamage(damage);
        }
        
        public void ApplyDamage(int damage) =>
            ApplyDamage(damage, transform.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f)));
    }
}
