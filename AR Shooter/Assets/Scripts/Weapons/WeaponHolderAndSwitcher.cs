using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponHolderAndSwitcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform virtualHands;
    private Transform gunHolderTransform;
    
    protected Dictionary<int, MainWeapon> mainWeapons = new Dictionary<int, MainWeapon>();

    protected MainWeapon currentWeaponScript;
    protected int currentWeaponIndex = 0;
    protected bool currentWeaponShoot = false;

    protected virtual void Awake()
    {
        UI.weaponHolderScript = this;
    }

    private void Start()
    {
        gunHolderTransform = transform;

        foreach (MainWeapon mainWeapon in gunHolderTransform.GetComponentsInChildren<MainWeapon>(true))
        {
            switch (mainWeapon.weaponType)
            {
                case MainWeapon.WeaponType.AKM:
                    Config.AKM = new Config.WeaponStats(1, 1 / 6f, new Vector3(0, -0.117f, 0.178f), mainWeapon);
                    break;
                case MainWeapon.WeaponType.Sniper:
                    Config.Sniper = new Config.WeaponStats(10, 1.82f, new Vector3(0, -0.1335f, 0.26f), mainWeapon);
                    break;
            }
            
            mainWeapons.Add((int)mainWeapon.weaponType, mainWeapon);

            if (mainWeapon.gameObject.activeSelf)
            {
                currentWeaponScript = mainWeapon;
            }
        }
    }

    private float translationTime;

    private void Update()
    {
        if (IsTranslating()) Translation();
    }

    public virtual void Shoot(bool start)
    {
        currentWeaponShoot = start;
        if (!switchAnimation && mainWeapons.TryGetValue(currentWeaponIndex, out MainWeapon mainWeaponScript)) mainWeaponScript.Shoot(start);
    }

    public bool isAimed = false;

    #region Weapon Switching

    protected bool switchAnimation = false;
    private Vector3 weaponPosTo;
    private Quaternion weaponRotTo = Quaternion.Euler(0, 0, 0);

    public virtual void SwitchWeapon(int toWeaponIndex)
    {
        currentWeaponIndex = toWeaponIndex;
        currentWeaponScript.Shoot(false);
        StopAllCoroutines();

        mainWeapons.TryGetValue(currentWeaponIndex, out MainWeapon newWeaponScript);

        StartCoroutine(Switching(newWeaponScript));
    }

    protected IEnumerator Switching(MainWeapon newWeaponScript)
    {
        GameObject currentWeaponObj = currentWeaponScript.gameObject;

        if (newWeaponScript != currentWeaponScript)
        {
            TranslateWeapon(Config.standartBackwardPos, Quaternion.Euler(-60, 0, 0), 0.3f);
        }

        yield return new WaitWhile(IsTranslating);
        currentWeaponScript = newWeaponScript;

        currentWeaponObj.SetActive(false);
        currentWeaponScript.gameObject.SetActive(true);

        TranslateWeapon(Config.standartFreePos, Quaternion.Euler(0, 0, 0), 0.3f);

        yield return new WaitWhile(IsTranslating);

        switchAnimation = false;

        if (currentWeaponShoot) currentWeaponScript.Shoot(true);
        if (isAimed) Aiming(true);
    }

    #endregion

    #region Weapon Aiming

    public void Aiming(bool toAim)
    {
        isAimed = toAim;
        if (!switchAnimation)
        {
            if (toAim)
            {
                TranslateWeapon(Config.GetStats((MainWeapon.WeaponType)currentWeaponIndex).Value.aimingPos, 0.16f);
            }
            else
            {
                TranslateWeapon(Config.standartFreePos, 0.16f);
            }
        }
    }

    #endregion

    private void TranslateWeapon(Vector3 toPos, float time)
    {
        translationTime = time - translationTime;
        weaponPosTo = toPos;
    }

    private void TranslateWeapon(Vector3 toPos, Quaternion toRot, float time)
    {
        weaponRotTo = toRot;
        TranslateWeapon(toPos, time);
    }

    private void Translation()
    {
        virtualHands.localPosition = Vector3.Lerp(virtualHands.localPosition, weaponPosTo, Time.deltaTime / translationTime);
        if (virtualHands.localRotation != weaponRotTo) virtualHands.localRotation = Quaternion.Lerp(virtualHands.localRotation, weaponRotTo, Time.deltaTime / translationTime);
        translationTime -= Time.deltaTime;
        if (translationTime < Time.deltaTime) virtualHands.localPosition = weaponPosTo;
    }

    private bool IsTranslating()
    {
        return translationTime > 0;
    }
}
