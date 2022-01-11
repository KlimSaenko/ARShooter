using Game.Mobs;
using UnityEngine;
using UnityEngine.Pool;
using Game.Weapons;

namespace Game.Managers
{
    public class Pool : MonoBehaviour
    {
        private void Awake()
        {
            DecalsInstance = new Decals(hitDecalPrefab, decalsFolder);
            MobsInstance = new Mobs(mobPrefab, mobsFolder);
        }

        [Header("Decals")] 
        [SerializeField] private Transform decalsFolder;
        [SerializeField] private GameObject hitDecalPrefab;

        internal static Decals DecalsInstance;
    
        internal class Decals
        {
            private static LinkedPool<HitDecal> _hitDecals;
            private static GameObject _hitDecalPrefab;
            private static Transform _folder;

            internal Decals(GameObject hitDecalPrefab, Transform folder)
            {
                _hitDecalPrefab = hitDecalPrefab;
                _folder = folder;

                _hitDecals = new LinkedPool<HitDecal>(hitDecalPrefab.GetComponent<HitDecal>,
                    rt => rt.gameObject.SetActive(true),
                    rt => rt.gameObject.SetActive(false),
                    null, false, 10);
            }
        
            internal static void ActivateHitMarker(Vector3 pos, int damage, HitZone.ZoneType type, bool withMarker = true)
            {
                if (_hitDecals.CountInactive <= 0)
                    _hitDecals.Release(Instantiate(_hitDecalPrefab, Vector3.zero, Quaternion.identity, _folder).GetComponent<HitDecal>());
            
                var currentHitDecal = _hitDecals.Get();
                currentHitDecal.transform.position = pos;
                currentHitDecal.ActivateHitMarker(damage, type, withMarker);
            }

            internal static void DeactivateHitMarker(HitDecal hitDecal)
            {
                _hitDecals.Release(hitDecal);
            }
        }

        [Space]
        [Header("Mobs")]
        [SerializeField] private Transform mobsFolder;
        [SerializeField] private GameObject mobPrefab;

        internal static Mobs MobsInstance;

        internal class Mobs
        {
            private static LinkedPool<MainMob> _mobs;
            private static GameObject _mobPrefab;
            private static Transform _folder;

            internal Mobs(GameObject mobPrefab, Transform folder)
            {
                _mobPrefab = mobPrefab;
                _folder = folder;

                _mobs = new LinkedPool<MainMob>(mobPrefab.GetComponent<MainMob>,
                    rt => rt.gameObject.SetActive(true),
                    rt => rt.gameObject.SetActive(false),
                    null, false, 50);
            }

            internal static MainMob ActivateMob(Vector3 pos)
            {
                if (_mobs.CountInactive <= 0)
                    _mobs.Release(Instantiate(_mobPrefab, Vector3.zero, Quaternion.identity, _folder).GetComponent<MainMob>());

                var currentMob = _mobs.Get();
                currentMob.transform.position = pos;

                return currentMob;
            }

            internal static void DeactivateHitMarker(MainMob hitDecal)
            {
                _mobs.Release(hitDecal);
            }
        }
    }
}
