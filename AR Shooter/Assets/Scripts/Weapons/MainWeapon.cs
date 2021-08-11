using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Weapons
{
    public class MainWeapon : MonoBehaviour, IWeaponConfig
    {
        [Header("Weapon config")]
        [SerializeField] private Vector3 posToAim;
        [SerializeField] private Vector3 posFromAim;
        [SerializeField] protected WeaponStats weaponStats;

        public static WeaponStats ActiveWeaponStats;
        
        public Vector3 PosToAim => posToAim;
        public Vector3 PosFromAim => posFromAim;
        public virtual WeaponType WeaponType => WeaponType.Unsigned;
        
        private Reload _reloadState;

        private int _bulletCount;

        public int BulletCount
        {
            get => _bulletCount;
            set
            {
                _bulletCount = value;
                
                BulletUI.UpdateCount(this);
                
                if (value <= 0)
                {
                    StartReload();
                }
            }
        }

        private protected void SetWeaponBehaviour()
        {
            PlayerBehaviour.FiringAction += Shoot;
            WeaponHolder.WeaponReadyAction += SetReady;
            
            SetActive(true);
            SetReady(true);
            BulletCount = weaponStats.bulletCount;
            
            if (shootAudio is null) return;

            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }

        private static bool _isFiring;
        private bool _isReady;
        
        private bool CanShoot => IsActive && _isReady && !_reloadState.IsReloading && !LogicIsRunning();
        
        [Space]
        [Header("Weapon Attachments")]
        [SerializeField] protected ParticleSystem shellsParticle;
        [SerializeField] protected ParticleSystem flashParticle;
        [SerializeField] protected Animation shootAnimation;
        [SerializeField] protected AudioClip shootAudio;
        
        [Header("UI")]
        [SerializeField] private Slider reloadSlider;
        
        protected AudioSource AudioSource;
        
        public bool IsActive => gameObject.activeSelf;
        
        public void SetActive(bool value)
        {
            if (value)
            {
                _reloadState ??= new Reload(CompleteReload, reloadSlider);
                
                ActiveWeaponStats = weaponStats;
                BulletUI.UpdateCount(this);
            }
            
            gameObject.SetActive(value);
        }

        private void SetReady(bool ready)
        {
            _isReady = ready;

            if (ready)
            {
                if (_reloadState.IsReloading) StartReload();
                else Shoot(_isFiring);
            }
            else if (_reloadState.IsReloading) _reloadState.SkipReload();
        }

        private void Shoot(bool start)
        {
            _isFiring = start;
            if (start && CanShoot)
            {
                StartCoroutine(Shooting());
            }
        }

        private IEnumerator Shooting()
        {
            while (_isFiring && CanShoot)
            {
                VisualizeFiring();

                RunWeaponLogic();

                BulletCount--;

                yield return new WaitWhile(LogicIsRunning);
            }
        }

        protected virtual bool LogicIsRunning() => false;

        protected virtual void VisualizeFiring()
        {
            shootAnimation.Play();
        }

        protected virtual void RunWeaponLogic() { }

        private void StartReload() =>
            _reloadState.StartReload();

        private void CompleteReload()
        {
            BulletCount = weaponStats.bulletCount;
        }
        
        #region Reload

        private class Reload
        {
            private readonly Slider _reloadSlider;
            private readonly Action _reloadActionCallback;
            internal bool IsReloading;

            private readonly EventTrigger _eventTrigger;
            private readonly EventTrigger.Entry _entry;
        
            internal Reload(Action reloadActionCallback, Slider reloadSlider)
            {
                _reloadActionCallback += reloadActionCallback;
                _reloadSlider = reloadSlider;
            
                _eventTrigger = _reloadSlider.gameObject.TryGetComponent(out EventTrigger eventTrigger) 
                    ? eventTrigger : _reloadSlider.gameObject.AddComponent<EventTrigger>();
                _entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                _entry.callback.AddListener( _ => { StopReload(false); } );
            }
        
            private void ReloadValueChange(float value)
            {
                if (value > 0.99f) StopReload(true);
            }

            internal void StartReload()
            {
                IsReloading = true;
                _reloadSlider.gameObject.SetActive(true);
                
                _reloadSlider.onValueChanged.AddListener(ReloadValueChange);
                _eventTrigger.triggers.Add(_entry);
            }

            private void StopReload(bool complete)
            {
                IsReloading = !complete;

                if (complete)
                {
                    SkipReload();
                    _reloadActionCallback?.Invoke();
                }
                else _reloadSlider.DOValue(0, _reloadSlider.value * 0.7f).SetEase(Ease.OutQuad);
            }

            internal void SkipReload()
            {
                _reloadSlider.onValueChanged.RemoveAllListeners();
                _eventTrigger.triggers.Clear();
                
                _reloadSlider.value = 0;
                _reloadSlider.gameObject.SetActive(false);
            }
        }
        
        #endregion
    }
    
    public enum WeaponType
    {
        Unsigned = 0,
        M4_Carabine,
        Shotgun
    }

    [Serializable]
    public struct WeaponStats
    {
        public WeaponStats(int damageMin, int damageMax, int aimedAimSpreadDiameter, int freeAimSpreadDiameter, int aimSpreadIncrement, float aimRecoveryTime, 
            int bulletCount, Transform bulletUI)
        {
            this.damageMin = damageMin;
            this.damageMax = damageMax;
            this.aimedAimSpreadDiameter = aimedAimSpreadDiameter;
            this.freeAimSpreadDiameter = freeAimSpreadDiameter;
            this.aimSpreadIncrement = aimSpreadIncrement;
            this.aimRecoveryTime = aimRecoveryTime;
            this.bulletCount = bulletCount;
            this.bulletUI = bulletUI;
        }

        public int damageMin, damageMax;

        public int aimedAimSpreadDiameter, freeAimSpreadDiameter, aimSpreadIncrement;
        public float aimRecoveryTime;

        public int bulletCount;
        public Transform bulletUI;
    }
}
