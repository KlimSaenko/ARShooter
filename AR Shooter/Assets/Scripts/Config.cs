using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public static class Config
{
    public static WeaponStatsTemp AKM;
    public static WeaponStatsTemp Sniper;

    public static Vector3 StandardFreePos = new Vector3(0.122f, -0.122f, 0.25f);
    public static Vector3 StandardBackwardPos = new Vector3(0.167f, -0.167f, -0.1f);

    public static Vector3 AKMAimingPos = new Vector3(0, -0.115f, 0.178f);
    public static Vector3 SniperAimingPos = new Vector3(0, -0.1335f, 0.26f);

    public struct WeaponStatsTemp
    {
        public readonly int Damage;
        public float TimeDelay;
        public Vector3 AimingPos;
        public MainWeapon WeaponScript;

        public WeaponStatsTemp(int damage, float timeDelay, Vector3 aimingPos, MainWeapon weaponScript)
        {
            Damage = damage;
            TimeDelay = timeDelay;
            AimingPos = aimingPos;
            WeaponScript = weaponScript;
        }
    }

    // internal static WeaponStatsTemp? GetStats(MainWeapon.WeaponType weaponType)
    // {
    //     return weaponType switch
    //     {
    //         MainWeapon.WeaponType.AKM => AKM,
    //         MainWeapon.WeaponType.Sniper => Sniper,
    //         _ => null
    //     };
    // }

    #region SingleplayerStats

    // private static int _mobsKills = 0;
    internal static int MobsKills
    {
        get => MobsKills;
        set
        {
            try
            {
                UI.KillsUI(value);
                RecordKills = value;
                MobsKills = value;
            }
            catch { /* ignored */ }
        }
    }

    // private static int _recordKills;
    internal static int RecordKills
    {
        get => RecordKills;
        
        private set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value) + " <= 0");
            
            if (RecordKills < value)
                RecordKills = value;
        }
    }

    #endregion

    #region Settings

    public struct GameSettings
    {
        public static bool IsStaticSpawnZone = false;
        
        public static int OcclusionLevel = 2;
    }

    #endregion

    internal static void SaveGame()
    {
        PlayerPrefs.SetInt(nameof(RecordKills), RecordKills);
        PlayerPrefs.SetInt(nameof(GameSettings.IsStaticSpawnZone), GameSettings.IsStaticSpawnZone ? 1 : 0);
        PlayerPrefs.SetInt(nameof(GameSettings.OcclusionLevel), GameSettings.OcclusionLevel);

        PlayerPrefs.Save();
    }

    internal static void LoadGame()
    {
        if (PlayerPrefs.HasKey(nameof(RecordKills)))
            RecordKills = PlayerPrefs.GetInt(nameof(RecordKills));
        
        if (PlayerPrefs.HasKey(nameof(GameSettings.IsStaticSpawnZone)))
            GameSettings.IsStaticSpawnZone = PlayerPrefs.GetInt(nameof(GameSettings.IsStaticSpawnZone)) == 1;
        
        if (PlayerPrefs.HasKey(nameof(GameSettings.OcclusionLevel)))
            GameSettings.OcclusionLevel = PlayerPrefs.GetInt(nameof(GameSettings.OcclusionLevel));
    }
}
