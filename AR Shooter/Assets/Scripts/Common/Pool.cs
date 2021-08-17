using Mobs;
using UnityEngine;
using UnityEngine.Pool;
using Weapons;

namespace Common
{
    public class Pool : MonoBehaviour
    {
        private void Awake()
        {
            DecalsInstance = new Decals(hitDecalPrefab, folder);
        }

        [Header("Decals")] 
        [SerializeField] private Transform folder;
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
    }
}
