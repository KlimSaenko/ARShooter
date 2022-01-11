using System;
using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.Weapons
{
    public class WeaponSetSpawner : MonoBehaviour
    {
        [SerializeField] private TextMeshPro bulletsText;
        [SerializeField] private WeaponSet currentWeaponsToUse;

        [Space]
        [SerializeField] private Transform[] weaponContainers;

        [Space]
        [SerializeField] private Transform buttonsSaveFolder;

        internal static Transform ButtonsSaveFolder;

        private IEnumerator Start()
        {
            ButtonsSaveFolder = buttonsSaveFolder;
            
            yield return new WaitUntil(WeaponsListAccess.ListReady);
            
            SetPlayerCurrentWeapons();
        }

        private void SetPlayerCurrentWeapons()
        {
            if (currentWeaponsToUse.weaponNames.Length <= 0) return;

            var weaponsCount = currentWeaponsToUse.weaponNames.Length;
            currentWeaponsToUse.weaponPrefabs = new GameObject[weaponsCount];

            WeaponName weaponType;
            for (var i = 0; i < weaponsCount; i++)
            {
                weaponType = currentWeaponsToUse.weaponNames[i];

                if (weaponType == WeaponName.Unsigned) continue;

                currentWeaponsToUse.weaponPrefabs[i] = WeaponsListAccess.GetWeaponPrefab(weaponType, out var weaponScript);

                _ = weaponScript.InstantiateWeapon(weaponContainers[i], i, bulletsText);
            }
        }
    }

    [Serializable]
    internal struct WeaponSet
    {
        [SerializeField] internal WeaponName[] weaponNames;

        internal GameObject[] weaponPrefabs;
    }
}
