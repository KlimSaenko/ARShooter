using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Game.Weapons
{
    public sealed class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private Transform hideWeaponRef;

        private static Transform _thisTransform;

        internal static event Action<bool> WeaponReadyAction;
        
        private static void OnWeaponReadyAction(bool ready) =>
            WeaponReadyAction?.Invoke(ready);

        private void Awake()
        {
            _thisTransform = transform;
            _hideWeaponRef = hideWeaponRef;
            
            PlayerBehaviour.AimingAction += Aiming;
            PlayerBehaviour.WeaponSwitchAction += SwitchWeapon;
        }

        private static bool _isAimed;

        #region Weapon Switching

        private static Transform _hideWeaponRef;
        
        private static void SwitchWeapon(WeaponName toWeapon)
        {
            if (_hideWeaponRef is null || CurrentWeaponConfig.WeaponName == toWeapon) return;
            
            HideWeapon(() => TakeWeapon(toWeapon));
        }

        private static void HideWeapon(TweenCallback onCompleteAction = null)
        {
            _thisTransform.DOLocalMove(_hideWeaponRef.localPosition, 0.35f).SetEase(Ease.InOutCubic).OnStart(() => OnWeaponReadyAction(false));
            _thisTransform.DOLocalRotate(_hideWeaponRef.localRotation.eulerAngles, 0.35f).SetEase(Ease.InOutCubic)
                .OnComplete(onCompleteAction);
        }
        
        private static void TakeWeapon(WeaponName toWeapon)
        {
            var weaponsConfigs = WeaponsConfigs;
            if (weaponsConfigs is null) return;

            //CurrentWeaponConfig.SetActive(false);
            
            //foreach (var config in weaponsConfigs)
            //{
            //    if (config.WeaponType == toWeapon)
            //        config.SetActive(true);
            //}
            
            //var dest = _isAimed ? CurrentWeaponConfig.PosToAim : CurrentWeaponConfig.PosFromAim;
            
            //_thisTransform.DOLocalMove(dest, 0.35f).SetEase(Ease.InOutCubic).OnComplete(() => OnWeaponReadyAction(true));
            _thisTransform.DOLocalRotate(Vector3.zero, 0.35f).SetEase(Ease.InOutCubic);
        }

        #endregion

        private static MainWeapon[] _weaponsConfigs;
        private static IEnumerable<IWeapon> WeaponsConfigs
        {
            get
            {
                if (_thisTransform is null) return null;

                return _weaponsConfigs ??= _thisTransform.GetComponentsInChildren<MainWeapon>(true);
            }
        }
        
        private static IWeapon CurrentWeaponConfig
        {
            get
            {
                var weaponsConfigs = WeaponsConfigs;
                if (weaponsConfigs is null) return null;
                
                IWeapon config = null;
                
                foreach (var currentConfig in weaponsConfigs)
                {
                    //if (!currentConfig.IsActive) continue;
                    
                    if (config is null)
                        config = currentConfig;
                    //else currentConfig.SetActive(false);
                }

                return config;
            }
        }
        
        #region Weapon Aiming
        
        private static void Aiming(bool toAim)
        {
            if (CurrentWeaponConfig is null) return;
            
            //var dest = toAim ? CurrentWeaponConfig.PosToAim : CurrentWeaponConfig.PosFromAim;
            //_thisTransform.DOLocalMove(dest, 0.25f).SetEase(Ease.InOutCubic);

            _isAimed = toAim;
        }

        #endregion
    }
}
