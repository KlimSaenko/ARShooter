using Game.SO;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons
{
    [CreateAssetMenu(menuName = "Event/WeaponsList")]
    public class WeaponsListAccess : GameEvent
    {
        //private static List<MainWeapon> weaponsPrefabs;
        private static Dictionary<WeaponName, MainWeapon> _allWeaponsPrefabs = new();

        internal static Dictionary<WeaponName, MainWeapon> AllowableWeaponsPrefabs
        {
            get 
            {
                var allowableWeapons = PlayerProgress.AllowableWeapons;
                
                Dictionary<WeaponName, MainWeapon> allowableWeaponsPrefabs = new(allowableWeapons.Count);

                foreach(WeaponName weaponType in allowableWeapons)
                {
                    if (_allWeaponsPrefabs.TryGetValue(weaponType, out var weaponScript)) allowableWeaponsPrefabs.Add(weaponType, weaponScript);
                }

                return allowableWeaponsPrefabs;
            }
        }

        internal static void AddWeaponPrefab(MainWeapon weaponScript)
        {
            //weaponsPrefabs.Add(weaponScript);
            if (_allWeaponsPrefabs.ContainsKey(weaponScript.WeaponName)) return;

            _allWeaponsPrefabs.Add(weaponScript.WeaponName, weaponScript);
        }

        internal static GameObject GetWeaponPrefab(WeaponName type)
        {
            var weaponPrefab = _allWeaponsPrefabs.TryGetValue(type, out MainWeapon prefab) ? prefab.gameObject : null;

            return weaponPrefab;
        }

        internal static GameObject GetWeaponPrefab(WeaponName type, out IWeapon weaponScript)
        {
            var weaponPrefab = _allWeaponsPrefabs.TryGetValue(type, out MainWeapon prefab) ? prefab : null;
            weaponScript = weaponPrefab;

            return weaponPrefab != null ? weaponPrefab.gameObject : null;
        }

        private static bool _listReady;
        internal static bool ListReady() => _listReady && _allWeaponsPrefabs.Count > 0;
        //internal static bool ListReady()
        //{
        //    var listReady = _listReady && allWeaponsPrefabs.Count > 0;

        //    if (!listReady)
        //    {
        //        //Raise()
        //    }

        //    _listReady = listReady;
        //    return listReady;
        //}

        public static void SetListReady(bool value)
        {
            _listReady = value;
        }
    }
}
