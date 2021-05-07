using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Config
{
    public static WeaponStats AKM;
    public static WeaponStats Sniper;

    public static Vector3 standartFreePos = new Vector3(0.122f, -0.122f, 0.25f);
    public static Vector3 standartBackwardPos = new Vector3(0.167f, -0.167f, -0.1f);

    public static Vector3 AKMAimingPos = new Vector3(0, -0.115f, 0.178f);
    public static Vector3 SniperAimingPos = new Vector3(0, -0.1335f, 0.26f);

    public struct WeaponStats
    {
        public int damage;
        public float timeDelay;
        public Vector3 aimingPos;
        public MainWeapon weaponScript;

        public WeaponStats(int damage, float timeDelay, Vector3 aimingPos, MainWeapon weaponScript)
        {
            this.damage = damage;
            this.timeDelay = timeDelay;
            this.aimingPos = aimingPos;
            this.weaponScript = weaponScript;
        }
    }

    internal static WeaponStats? GetStats(MainWeapon.WeaponType weaponType)
    {
        switch (weaponType)
        {
            case MainWeapon.WeaponType.AKM:
                return AKM;
            case MainWeapon.WeaponType.Sniper:
                return Sniper;
            default:
                return null;
        }
    }

    #region Singleplayer

    private static int mobsKills = 0;
    internal static int MobsKills
    {
        get
        {
            return mobsKills;
        }
        set
        {
            try
            {
                UI.KillsUI(value);
                RecordKills = value;
                mobsKills = value;
            }
            catch { }
        }
    }

    private static int recordKills;
    internal static int RecordKills
    {
        get
        {
            return recordKills;
        }
        set
        {
            if (recordKills < value)
            {
                recordKills = value;
            }
        }
    }

    #endregion

    #region Settings

    public static bool isStaticSpawnZone = false;

    public static int occlusionLevel = 2;

    #endregion

    internal static void SaveGame()
    {
        PlayerPrefs.SetInt("KillsRecord", recordKills);
        PlayerPrefs.SetInt("IsStaticSpawnZone", isStaticSpawnZone ? 1 : 0);
        PlayerPrefs.SetInt("OcclusionLevel", occlusionLevel);

        PlayerPrefs.Save();
    }

    internal static void LoadGame()
    {
        if (PlayerPrefs.HasKey("KillsRecord"))
        {
            recordKills = PlayerPrefs.GetInt("KillsRecord");
            isStaticSpawnZone = PlayerPrefs.GetInt("IsStaticSpawnZone") == 1;
            occlusionLevel = PlayerPrefs.GetInt("OcclusionLevel");
        }
    }
}
