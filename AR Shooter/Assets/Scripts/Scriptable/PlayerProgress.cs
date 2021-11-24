using System;
using System.Collections.Generic;
using System.IO;
//using UnityEditor;
using UnityEngine;
using Game.Weapons;

namespace Game.SO
{
    [CreateAssetMenu]
    public class PlayerProgress : ScriptableObject
    {
        [SerializeField] private TextAsset jsonToRead;

        internal static PlayerProgressVariables PlayerProgressVariables;

        private static string pathOfJson;

        private void OnEnable()
        {
            if (jsonToRead is null) return;

            //pathOfJson = AssetDatabase.GetAssetPath(jsonToRead.GetInstanceID());
            //PlayerProgressVariables.allowableWeapons = new() { WeaponType.Pistol, WeaponType.M4Carabine };
            //pathOfJson = AssetDatabase.GetAssetPath(jsonToRead.GetInstanceID());
            //var data = JsonSaver.SerializeData(PlayerProgressVariables);
            //File.WriteAllText(pathOfJson, data);
            PlayerProgressVariables = JsonSaver.DeserializeData<PlayerProgressVariables>(jsonToRead.text);
        }

        internal static void ChangeAllowableWeapons(WeaponName newWeapon)
        {
            if (newWeapon == WeaponName.Unsigned || PlayerProgressVariables.allowableWeapons.Contains(newWeapon)) return;

            PlayerProgressVariables.allowableWeapons.Add(newWeapon);

            var data = JsonSaver.SerializeData(PlayerProgressVariables);
            //File.WriteAllText(pathOfJson, data);
        }
    }

    [Serializable]
    public struct PlayerProgressVariables
    {
        public List<WeaponName> allowableWeapons;
    }
}