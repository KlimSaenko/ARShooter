using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static Config;

namespace Weapons
{
    public class WeaponHolderAndSwitcher : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform virtualHands;
        private Transform _gunHolderTransform;
    
        protected readonly Dictionary<int, MainWeapon> MainWeapons = new();

        protected MainWeapon CurrentWeaponScript;
        protected int CurrentWeaponIndex;
        protected bool CurrentWeaponShoot;

        protected virtual void Awake()
        {
            UI.weaponHolderScript = this;
        }

        private void Start()
        {
            _gunHolderTransform = transform;

            foreach (var mainWeapon in _gunHolderTransform.GetComponentsInChildren<MainWeapon>(true))
            {
                switch (mainWeapon.weaponType)
                {
                    case MainWeapon.WeaponType.AKM:
                        AKM = new WeaponStats(1, 1 / 6f, new Vector3(0, -0.117f, 0.178f), mainWeapon);
                        break;
                    case MainWeapon.WeaponType.Sniper:
                        Sniper = new WeaponStats(10, 1.82f, new Vector3(0, -0.1335f, 0.26f), mainWeapon);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
                MainWeapons.Add((int)mainWeapon.weaponType, mainWeapon);

                if (mainWeapon.gameObject.activeSelf)
                {
                    CurrentWeaponScript = mainWeapon;
                }
            }
        }

        private float _translationTime;

        private void Update()
        {
            if (IsTranslating()) Translation();
        }

        public virtual void Shoot(bool start)
        {
            CurrentWeaponShoot = start;
            if (!SwitchAnimation && MainWeapons.TryGetValue(CurrentWeaponIndex, out var mainWeaponScript)) mainWeaponScript.Shoot(start);
        }

        public bool isAimed;

        #region Weapon Switching

        protected bool SwitchAnimation;
        private Vector3 _weaponPosTo;
        private Quaternion _weaponRotTo = Quaternion.Euler(0, 0, 0);

        public virtual void SwitchWeapon(int toWeaponIndex)
        {
            CurrentWeaponIndex = toWeaponIndex;
            CurrentWeaponScript.Shoot(false);
            StopAllCoroutines();

            MainWeapons.TryGetValue(CurrentWeaponIndex, out var newWeaponScript);

            StartCoroutine(Switching(newWeaponScript));
        }

        protected IEnumerator Switching(MainWeapon newWeaponScript)
        {
            var currentWeaponObj = CurrentWeaponScript.gameObject;

            if (newWeaponScript != CurrentWeaponScript)
            {
                TranslateWeapon(StandardBackwardPos, Quaternion.Euler(-60, 0, 0), 0.32f);
            }

            yield return new WaitWhile(IsTranslating);
            CurrentWeaponScript = newWeaponScript;

            currentWeaponObj.SetActive(false);
            CurrentWeaponScript.gameObject.SetActive(true);

            TranslateWeapon(StandardFreePos, Quaternion.Euler(0, 0, 0), 0.32f);

            yield return new WaitWhile(IsTranslating);

            SwitchAnimation = false;

            if (CurrentWeaponShoot) CurrentWeaponScript.Shoot(true);
            if (isAimed) Aiming(true);
        }

        #endregion

        #region Weapon Aiming

        public void Aiming(bool toAim)
        {
            isAimed = toAim;
            if (!SwitchAnimation)
            {
                TranslateWeapon(
                    toAim ? GetStats((MainWeapon.WeaponType) CurrentWeaponIndex).Value.AimingPos : StandardFreePos,
                    0.16f);
            }
        }

        #endregion

        private void TranslateWeapon(Vector3 toPos, float time)
        {
            _translationTime = time - _translationTime;
            _weaponPosTo = toPos;
        }

        private void TranslateWeapon(Vector3 toPos, Quaternion toRot, float time)
        {
            _weaponRotTo = toRot;
            TranslateWeapon(toPos, time);
        }

        private void Translation()
        {
            virtualHands.localPosition = Vector3.Lerp(virtualHands.localPosition, _weaponPosTo, Time.deltaTime / _translationTime);
            if (virtualHands.localRotation != _weaponRotTo) virtualHands.localRotation = Quaternion.Lerp(virtualHands.localRotation, _weaponRotTo, Time.deltaTime / _translationTime);
            _translationTime -= Time.deltaTime;
            if (_translationTime < Time.deltaTime) virtualHands.localPosition = _weaponPosTo;
        }

        private bool IsTranslating()
        {
            return _translationTime > 0;
        }
    }
}
