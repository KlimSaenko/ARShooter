using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;
using Game.Weapons;

namespace Game.SO
{
    [CreateAssetMenu(menuName = "Event/JsonSaver"), Serializable]
    public class JsonSaver : GameEvent
    {
        [SerializeField] private TextAsset weaponsConfigsJson;

        [Space]
        [SerializeField] private UnityEvent<WeaponsList> setWeaponConfigsEvent;

        private string playerProgressFolderPath;

        internal override void Raise()
        {
            var weaponConfigs = DeserializeData<WeaponsList>(weaponsConfigsJson.text);

            setWeaponConfigsEvent?.Invoke(weaponConfigs);
        }

        private void OnEnable()
        {
            playerProgressFolderPath = Path.Combine(Application.persistentDataPath, "saved_files");

            if (!Directory.Exists(playerProgressFolderPath))
                Directory.CreateDirectory(playerProgressFolderPath);

            playerProgressFolderPath += "/progress_data.json";
        }

        internal static string SerializeData(object objToJson)
        {
            //string jsonDataString = JsonUtility.ToJson(weaponConfigs, true);

            //File.WriteAllText(playerProgressFolderPath, jsonDataString);

            var data = JsonUtility.ToJson(objToJson, true);

            return data;
        }

        internal static T DeserializeData<T>(string jsonText)
        {
            //string loadedJsonDataString = File.ReadAllText(playerProgressFolderPath);

            //weaponConfigs = JsonUtility.FromJson<WeaponsList>(loadedJsonDataString);

            //Debug.Log("id: " + weaponConfigs[0].index.ToString() + " | name: " + weaponConfigs[0].name);

            var data = JsonUtility.FromJson<T>(jsonText);

            return data;
        }
    }
}