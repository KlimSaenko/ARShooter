using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Weapons;
using Game.Managers;

namespace Game.SO
{
    [CreateAssetMenu]
    public class PlayerProgress : ScriptableObject
    {
        private static PlayerProgressVariables _playerProgressVariables;

        internal static List<WeaponName> AllowableWeapons
        {
            get
            {
                if (_playerProgressVariables.allowableWeapons == null) return new();

                return _playerProgressVariables.allowableWeapons;
            }
        }

        internal static WeaponName FavoriteWeapon =>
            _playerProgressVariables.favoriteWeapon;

        private void OnEnable()
        {
            GameScenesManager.ApplicationPauseAction += (pause) =>
            {
                if (pause) OnSaveData();
            };
            
            //if (jsonToRead is null) return;

            //pathOfJson = AssetDatabase.GetAssetPath(jsonToRead.GetInstanceID());
            //PlayerProgressVariables.allowableWeapons = new() { WeaponType.Pistol, WeaponType.M4Carabine };
            //pathOfJson = AssetDatabase.GetAssetPath(jsonToRead.GetInstanceID());
            //var data = JsonSaver.SerializeData(PlayerProgressVariables);
            //File.WriteAllText(pathOfJson, data);
            //PlayerProgressVariables = JsonSaver.DeserializeData<PlayerProgressVariables>(jsonToRead.text);

            if (JsonSaver.OpenJson(_fileName, out string json))
                _playerProgressVariables = JsonSaver.DeserializeData<PlayerProgressVariables>(json);
        }

        internal static void ChangeAllowableWeapons(WeaponName weaponName, bool add = true)
        {
            if (weaponName == WeaponName.Unsigned) return;

            if (add && !_playerProgressVariables.allowableWeapons.Contains(weaponName))
                _playerProgressVariables.allowableWeapons.Add(weaponName);
            
            else if (!add && _playerProgressVariables.allowableWeapons.Contains(weaponName))
                _playerProgressVariables.allowableWeapons.Remove(weaponName);
        }

        private static readonly string _fileName = "player_progress";

        internal static void ChangeAllowableWeapons(IEnumerable<WeaponName> weaponNames, bool add = true)
        {
            foreach (var weaponName in weaponNames)
            {
                ChangeAllowableWeapons(weaponName, add);
            }
        }

        private static void OnSaveData()
        {
            var data = JsonSaver.SerializeData(_playerProgressVariables);
            JsonSaver.SaveJson(_fileName, data);
        }
    }

    [Serializable]
    public struct PlayerProgressVariables
    {
        public List<WeaponName> allowableWeapons;

        public WeaponName favoriteWeapon;

        public int gamesPlayed, kills;
    }
}