using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public static class Config
{
    public static WeaponStats AKM;
    public static WeaponStats Sniper;

    public static Vector3 StandardFreePos = new Vector3(0.122f, -0.122f, 0.25f);
    public static Vector3 StandardBackwardPos = new Vector3(0.167f, -0.167f, -0.1f);

    public static Vector3 AKMAimingPos = new Vector3(0, -0.115f, 0.178f);
    public static Vector3 SniperAimingPos = new Vector3(0, -0.1335f, 0.26f);

    public struct WeaponStats
    {
        public readonly int Damage;
        public float TimeDelay;
        public Vector3 AimingPos;
        public MainWeapon WeaponScript;

        public WeaponStats(int damage, float timeDelay, Vector3 aimingPos, MainWeapon weaponScript)
        {
            Damage = damage;
            TimeDelay = timeDelay;
            AimingPos = aimingPos;
            WeaponScript = weaponScript;
        }
    }

    internal static WeaponStats? GetStats(MainWeapon.WeaponType weaponType)
    {
        return weaponType switch
        {
            MainWeapon.WeaponType.AKM => AKM,
            MainWeapon.WeaponType.Sniper => Sniper,
            _ => null
        };
    }

    #region Singleplayer

    private static int _mobsKills = 0;
    internal static int MobsKills
    {
        get => _mobsKills;
        set
        {
            try
            {
                UI.KillsUI(value);
                RecordKills = value;
                _mobsKills = value;
            }
            catch
            {
                // ignored
            }
        }
    }

    private static int _recordKills;
    internal static int RecordKills
    {
        get => _recordKills;
        private set
        {
            if (_recordKills < value)
                _recordKills = value;
        }
    }

    #endregion

    #region Settings

    public static bool IsStaticSpawnZone = false;

    public static int OcclusionLevel = 2;

    #endregion

    internal static void SaveGame()
    {
        PlayerPrefs.SetInt("KillsRecord", _recordKills);
        PlayerPrefs.SetInt("IsStaticSpawnZone", IsStaticSpawnZone ? 1 : 0);
        PlayerPrefs.SetInt("OcclusionLevel", OcclusionLevel);

        PlayerPrefs.Save();
    }

    internal static void LoadGame()
    {
        if (!PlayerPrefs.HasKey("KillsRecord") || !PlayerPrefs.HasKey("IsStaticSpawnZone") || !PlayerPrefs.HasKey("OcclusionLevel")) return;
        
        _recordKills = PlayerPrefs.GetInt("KillsRecord");
        IsStaticSpawnZone = PlayerPrefs.GetInt("IsStaticSpawnZone") == 1;
        OcclusionLevel = PlayerPrefs.GetInt("OcclusionLevel");
    }
}
