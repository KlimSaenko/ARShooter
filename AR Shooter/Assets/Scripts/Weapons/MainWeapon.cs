using System;
using System.Collections;
using DG.Tweening;
using TMPro;
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
        private Vector3 _startLocalPos;

        private int _bulletCount;
        public int BulletCount
        {
            get => _bulletCount;
            set
            {
                _bulletCount = value;
                
                _bulletUI.UpdateCount(value);
                
                if (value <= 0)
                {
                    StartReload();
                }
            }
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
        
        private protected AudioSource AudioSource;
        
        public bool IsActive => gameObject.activeSelf;

        #region Setters

        private protected void SetWeaponBehaviour()
        {
            PlayerBehaviour.FiringAction += Fire;
            WeaponHolder.WeaponReadyAction += SetReady;
            
            SetActive(true);
            _isReady = true;
            BulletCount = weaponStats.bulletCount;
            _startLocalPos = transform.localPosition;
            
            if (shootAudio is null) return;

            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }
        
        public void SetActive(bool value)
        {
            if (value)
            {
                _reloadState ??= new ReloadState(CompleteReload, reloadSlider);
                _bulletUI ??= new BulletUI(bulletText);

                ActiveWeaponStats = weaponStats;
                _bulletUI.UpdateCount(BulletCount);
            }
            else transform.localPosition = _startLocalPos;

            bulletText.transform.GetChild((int)WeaponType - 1).gameObject.SetActive(value);
            gameObject.SetActive(value);
        }

        private void SetReady(bool ready)
        {
            if (!IsActive) return;
            
            _isReady = ready;

            if (ready)
            {
                if (_reloadState.IsReloading) StartReload();
                else Fire(_isFiring);
            }
            else if (_reloadState.IsReloading) _reloadState.SkipReload();
        }
        
        #endregion
        
        protected virtual bool LogicIsRunning() => false;

        protected virtual void VisualizeFiring()
        {
            shootAnimation.Play();
        }

        protected virtual void RunWeaponLogic() { }
        
        #region Fire
        
        private void Fire(bool start)
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
        
        #endregion
        
        #region Reload
        
        [Header("UI")]
        [SerializeField] private Slider reloadSlider;

        private ReloadState _reloadState;
        
        private void StartReload() =>
            _reloadState.StartReload();

        private void CompleteReload()
        {
            BulletCount = weaponStats.bulletCount;
        }
        
        private class ReloadState
        {
            private readonly Slider _reloadSlider;
            private readonly Action _reloadActionCallback;
            internal bool IsReloading;

            private readonly EventTrigger _eventTrigger;
            private readonly EventTrigger.Entry _entry;
        
            internal ReloadState(Action reloadActionCallback, Slider reloadSlider)
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
                _reloadSlider.interactable = true;
                _reloadSlider.gameObject.SetActive(true);
                _reloadSlider.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 1);
                
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

                _reloadSlider.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 1).OnComplete(() =>
                {
                    _reloadSlider.value = 0;
                    _reloadSlider.gameObject.SetActive(false);
                });
                _reloadSlider.interactable = false;
            }
        }
        
#endregion

        #region Bullet Count UI
        
        private void Update() =>
            BulletUI.UpdatePos(bulletUIRef.position);
        
        private BulletUI _bulletUI;

        [Space]
        [SerializeField] private TextMeshPro bulletText;
        [SerializeField] private Transform bulletUIRef;

        private class BulletUI
        {
            private readonly TextMeshPro _bulletText;
            private static Transform _bulletTextTransform;

            internal BulletUI(TextMeshPro bulletText)
            {
                _bulletText = bulletText;
                _bulletTextTransform = bulletText.transform;
            }

            internal static void UpdatePos(Vector3 posTo)
            {
                _bulletTextTransform.position = Vector3.Lerp(_bulletTextTransform.position, posTo, 0.5f);
            }

            internal void UpdateCount(int value)
            {
                _bulletText.text = value.ToString();
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
            int bulletCount)
        {
            this.damageMin = damageMin;
            this.damageMax = damageMax;
            this.aimedAimSpreadDiameter = aimedAimSpreadDiameter;
            this.freeAimSpreadDiameter = freeAimSpreadDiameter;
            this.aimSpreadIncrement = aimSpreadIncrement;
            this.aimRecoveryTime = aimRecoveryTime;
            this.bulletCount = bulletCount;
        }

        public int damageMin, damageMax;

        public int aimedAimSpreadDiameter, freeAimSpreadDiameter, aimSpreadIncrement;
        public float aimRecoveryTime;

        public int bulletCount;
    }
}
