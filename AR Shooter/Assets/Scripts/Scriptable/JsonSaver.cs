using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;

namespace Game.SO
{
    [CreateAssetMenu(menuName = "Event/JsonSaver"), Serializable]
    public class JsonSaver : GameEvent
    {
        [SerializeField] private TextAsset weaponsConfigsJson;

        [Space]
        [SerializeField] private UnityEvent<WeaponsList> setWeaponConfigsEvent;

        private static string saveFolderPath;

        internal static WeaponsList WeaponConfigs;

        internal override void Raise()
        {
            WeaponConfigs = DeserializeData<WeaponsList>(weaponsConfigsJson.text);

            setWeaponConfigsEvent?.Invoke(WeaponConfigs);
        }

        private void OnEnable()
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "saved_files");

            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
        }

        /// <summary>
        /// Serialize data to JSON format.
        /// </summary>
        /// <returns>The object's data in JSON format.</returns>
        internal static string SerializeData(object objToJson)
        {
            var data = JsonUtility.ToJson(objToJson, true);

            return data;
        }

        internal static T DeserializeData<T>(string jsonText)
        {
            var data = JsonUtility.FromJson<T>(jsonText);

            return data;
        }

        internal static bool OpenJson(string fileName, out string json)
        {
            fileName = saveFolderPath + $"/{fileName}.json";

            json = "";
            var exists = File.Exists(fileName);

            if (exists)
            {
                json = File.ReadAllText(fileName);
            }

            return exists;
        }

        internal static void SaveJson(string fileName, string jsonToSave)
        {
            fileName = saveFolderPath + $"/{fileName}.json";

            File.WriteAllText(fileName, jsonToSave);
        }
    }
}