﻿using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Weapons
{
    public sealed class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private Transform hideWeaponRef;

        private static Transform _thisTransform;

        private void Awake()
        {
            _thisTransform = transform;
            _hideWeaponRef = hideWeaponRef;
            
            PlayerBehaviour.AimingAction += Aiming;
            PlayerBehaviour.WeaponSwitchAction += SwitchWeapon;
        }

        private float _translationTime;

        private static bool _isAimed;

        #region Weapon Switching

        private static Transform _hideWeaponRef;
        
        private static void SwitchWeapon(WeaponType toWeapon)
        {
            if (_hideWeaponRef is null || CurrentWeaponConfig.WeaponType == toWeapon) return;
            
            HideWeapon(() => TakeWeapon(toWeapon));
        }

        private static void HideWeapon(TweenCallback onCompleteAction = null)
        {
            _thisTransform.DOMove(_hideWeaponRef.position, 0.35f).SetEase(Ease.InOutCubic);
            _thisTransform.DOLocalRotate(_hideWeaponRef.rotation.eulerAngles, 0.35f).SetEase(Ease.InOutCubic)
                .OnComplete(onCompleteAction);
            
            UI.AimInstance.SetActive(false);
        }
        
        private static void TakeWeapon(WeaponType toWeapon)
        {
            var weaponsConfigs = WeaponsConfigs;
            if (weaponsConfigs is null) return;

            CurrentWeaponConfig.SetActive(false);
            
            foreach (var config in weaponsConfigs)
            {
                if (config.WeaponType == toWeapon)
                    config.SetActive(true);
            }
            
            UI.AimInstance.SetActive(!_isAimed);
            
            var dest = _isAimed ? CurrentWeaponConfig.PosToAim : CurrentWeaponConfig.PosFromAim;
            
            _thisTransform.DOMove(dest, 0.35f).SetEase(Ease.InOutCubic).OnStart(() => UI.AimInstance.AimAnimation());
            _thisTransform.DOLocalRotate(Vector3.zero, 0.35f).SetEase(Ease.InOutCubic);
        }

        #endregion

        private static MainWeapon[] _weaponsConfigs;
        private static IEnumerable<IWeaponConfig> WeaponsConfigs
        {
            get
            {
                if (_thisTransform is null) return null;

                return _weaponsConfigs ??= _thisTransform.GetComponentsInChildren<MainWeapon>(true);
            }
        }
        
        private static IWeaponConfig CurrentWeaponConfig
        {
            get
            {
                var weaponsConfigs = WeaponsConfigs;
                if (weaponsConfigs is null) return null;
                
                IWeaponConfig config = null;
                
                foreach (var currentConfig in weaponsConfigs)
                {
                    if (!currentConfig.IsActive) continue;
                    
                    if (config is null)
                        config = currentConfig;
                    else currentConfig.SetActive(false);
                }

                return config;
            }
        }
        
        #region Weapon Aiming
        
        private static void Aiming(bool toAim)
        {
            if (CurrentWeaponConfig is null) return;
            
            var dest = toAim ? CurrentWeaponConfig.PosToAim : CurrentWeaponConfig.PosFromAim;
            _thisTransform.DOMove(dest, 0.3f).SetEase(Ease.InOutCubic);
            UI.AimInstance.SetActive(!toAim);

            _isAimed = toAim;
        }

        #endregion

        private bool IsTranslating() => _translationTime > 0;
    }
}
