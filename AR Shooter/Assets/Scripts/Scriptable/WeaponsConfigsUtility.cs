using Game.Weapons;
using System;
using System.IO;
using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(menuName = "ConfigSetter/WeaponsConfigsUtility"), ExecuteInEditMode]
    public class WeaponsConfigsUtility : ScriptableObject
    {
        [SerializeField] private TextAsset jsonToWrite;
        [SerializeField] private Profile profile;

        [Space]
        [SerializeField] internal WeaponsList weaponConfigs;

        private string pathOfJson;

        private void OnEnable()
        {
            //pathOfJson = AssetDatabase.GetAssetPath(jsonToWrite.GetInstanceID());
        }

        private void OnValidate()
        {
            if (jsonToWrite is null) return;

            switch (profile)
            {
                case Profile.Write:
                    var data = JsonSaver.SerializeData(weaponConfigs);
                    File.WriteAllText(pathOfJson, data);
                    break;

                case Profile.Read:
                    weaponConfigs = JsonSaver.DeserializeData<WeaponsList>(jsonToWrite.text);
                    break;

                default:
                    break;
            }
        }

        private enum Profile
        {
            Sleep,
            Write,
            Read
        }
    }

    [Serializable]
    public class WeaponsList
    {
        internal static int id;
        public Weapon[] weapons;

        public WeaponsList()
        {
            id++;
        }

        //internal WeaponStats CurrentWeaponStats(WeaponName type)
        //{
        //    var weaponStats = new WeaponStats();

        //    foreach (Weapon weapon in weapons)
        //    {
        //        if (weapon.type == type)
        //            weaponStats = weapon.stats;
        //    }

        //    return weaponStats;
        //}

        internal Weapon CurrentWeaponStats(WeaponName type)
        {
            var weaponStats = new Weapon();

            foreach (Weapon weapon in weapons)
            {
                if (weapon.weaponName == type)
                {
                    weaponStats = weapon;
                }
            }

            return weaponStats;
        }
    }
}
