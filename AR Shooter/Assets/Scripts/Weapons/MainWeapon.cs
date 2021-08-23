using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using DG.Tweening;
using Mobs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class MainWeapon : MonoBehaviour, IWeaponConfig
    {
        [Header("Weapon config")]
        [SerializeField] private Vector3 posToAim;
        [SerializeField] private Vector3 posFromAim;
        [SerializeField] protected WeaponStats weaponStats;
        
        public Vector3 PosToAim => posToAim;
        public Vector3 PosFromAim => posFromAim;
        public virtual WeaponType WeaponType => WeaponType.Unsigned;
        private (Vector3, Quaternion) _startLocalPos;

        private int _bulletCount;

        protected int BulletCount
        {
            get => _bulletCount;
            set
            {
                _bulletCount = value;
                
                _bulletUI.UpdateCount(value);
                
                ValidateBullets(value);
            }
        }
        
        private protected static bool IsFiring;
        private bool _isReady;

        private protected bool CanShoot => IsActive && _isReady && !_reloadState.IsReloading && !LogicIsRunning();
        
        [Space]
        [Header("Weapon Attachments")]
        [SerializeField] protected ParticleSystem shellsParticle;
        [SerializeField] protected ParticleSystem flashParticle;
        [SerializeField] protected Animation shootAnimation;
        [SerializeField] protected AudioClip shootAudio;
        
        private protected static AudioSource AudioSource;
        
        public bool IsActive => gameObject.activeSelf;
        
        private void Start() =>
            SetWeaponBehaviour();

        #region Setters

        private void SetWeaponBehaviour()
        {
            PlayerBehaviour.FiringAction += Fire;
            WeaponHolder.WeaponReadyAction += SetReady;
            
            SetActive(true);
            BulletCount = weaponStats.bulletCount;
            
            SetReady(true);
            
            var transform1 = transform;
            _startLocalPos.Item1 = transform1.localPosition;
            _startLocalPos.Item2 = transform1.localRotation;
            
            if (shootAudio is null || AudioSource is not null) return;

            AudioSource = transform.parent.gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }
        
        public void SetActive(bool value)
        {
            if (value)
            {
                _reloadState ??= new ReloadState(CompleteReload, reloadSlider);
                _bulletUI ??= new BulletUI(bulletText);

                _bulletUI.UpdateCount(BulletCount);
            }
            else
            {
                var transform1 = transform;
                transform1.localPosition = _startLocalPos.Item1;
                transform1.localRotation = _startLocalPos.Item2;
            }

            bulletText.transform.GetChild((int)WeaponType - 1).gameObject.SetActive(value);
            gameObject.SetActive(value);
        }

        private void SetReady(bool ready)
        {
            if (!IsActive) return;
            
            _isReady = ready;
            UI.AimInstance.SetActive(weaponStats, ready);

            if (ready)
            {
                if (ValidateBullets(BulletCount)) Fire(IsFiring);
                
                DynamicHolder.Inertia = Mathf.Pow(weaponStats.mass, 0.6f);
            }
            else if (_reloadState.IsReloading) _reloadState.SkipReload();
        }
        
        #endregion
        
        protected virtual bool LogicIsRunning() => false;

        protected virtual void VisualizeFiring()
        {
            shootAnimation.Play();
        }
        
        #region Fire

        private Shooting _shootingPatterns;
        private protected Shooting ShootingPatterns 
        {
            get
            {
                return _shootingPatterns ??= Config.CurrentGameplayMode switch
                {
                    Config.GameplayMode.Virtual => new Shooting(weaponStats),
                    Config.GameplayMode.Real => new ShootingRealSpace(weaponStats),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        
        private void Fire(bool start)
        {
            IsFiring = start;

            if (!start || !CanShoot) return;

            StartCoroutine(Shooting());
        }

        private protected virtual IEnumerator Shooting()
        {
            while (IsFiring && CanShoot)
            {
                VisualizeFiring();

                RunWeaponLogic();

                BulletCount--;

                yield return new WaitWhile(LogicIsRunning);
            }
        }
        
        protected virtual void RunWeaponLogic() { }
        
        #endregion
        
        #region Reload
        
        [Header("UI")]
        [SerializeField] private Slider reloadSlider;

        private ReloadState _reloadState;

        private bool ValidateBullets(int currentCount)
        {
            var valid = currentCount > 0;
            
            if (!valid) StartReload();

            return valid;
        }
        
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
        Pistol,
        M4_Carabine,
        Shotgun,
        Flamethrower
    }

    [Serializable]
    public struct WeaponStats
    {
        public WeaponStats(int damageMin, int damageMax, int aimedAimSpreadDiameter, int freeAimSpreadDiameter, int aimSpreadIncrement, float aimRecoveryTime, 
            int bulletCount, float mass)
        {
            this.damageMin = damageMin;
            this.damageMax = damageMax;
            this.aimedAimSpreadDiameter = aimedAimSpreadDiameter;
            this.freeAimSpreadDiameter = freeAimSpreadDiameter;
            this.aimSpreadIncrement = aimSpreadIncrement;
            this.aimRecoveryTime = aimRecoveryTime;
            this.bulletCount = bulletCount;
            this.mass = mass;
        }

        public int damageMin, damageMax;

        public int aimedAimSpreadDiameter, freeAimSpreadDiameter, aimSpreadIncrement;
        public float aimRecoveryTime;

        public int bulletCount;

        public float mass;
    }

    internal class Shooting
    {
        private protected readonly WeaponStats WeaponStats;

        internal Shooting(WeaponStats weaponStats)
        {
            WeaponStats = weaponStats;
        }
        
        internal virtual void ProcessRays(Ray currentRay)
        {
            if (!Physics.Raycast(currentRay, out var hitInfo) ||
                !hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone)) return;
            
            ApplyDamage(hitZone);
        }
        
        internal virtual void ProcessRays(Ray[] currentRays)
        {
            foreach (var currentRay in currentRays)
            {
                ProcessRays(currentRay);
            }
        }

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        internal virtual void ProcessCapsuleRay(float radius, float length)
        {
            var mainCam = Camera.main.transform;
            var forward = mainCam.forward;
            var camPosition = mainCam.position;
                
            var hitsCount = Physics.CapsuleCastNonAlloc(camPosition + forward * 0.5f, camPosition + forward * length,
                radius, forward, _hits, 0);
    
            for (var i = 0; i < hitsCount; i++)
            {
                if (!_hits[i].collider.TryGetComponent(out HitZone hitZone)) continue;
                
                var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
            
                hitZone.ApplyDamage(damage);
            }
        }

        internal IEnumerator ProcessCapsuleRay(float radius, float length, float delay)
        {
            var mainCam = Camera.main.transform;
            var forward = mainCam.forward;
            var camPosition = mainCam.position;
                
            var hitsCount = Physics.CapsuleCastNonAlloc(camPosition + forward * 0.5f, camPosition + forward * length,
                radius, forward, _hits, 0);

            float time = 0;
            for (var i = 0; i < hitsCount; i++)
            {
                time = Vector3.Distance(camPosition, _hits[i].collider.transform.position) * delay - time;
                
                yield return new WaitForSeconds(time);

                if (_hits[i].collider.TryGetComponent(out HitZone hitZone)) ApplyDamage(hitZone);
            }
        }

        private void ApplyDamage(HitZone hitZone)
        {
            var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
            
            hitZone.ApplyDamage(damage);
        }
    }

    internal class ShootingRealSpace : Shooting
    {
        internal ShootingRealSpace(WeaponStats weaponStats) : base(weaponStats)
        {
            
        }

        internal override void ProcessRays(Ray currentRay) =>
            RealTargetHit(new[] { currentRay });
        
        internal override void ProcessRays(Ray[] currentRays) =>
            RealTargetHit(currentRays);
        
        private void RealTargetHit(Ray[] currentRays)
        {
            var count = currentRays.Length;
            
            if (count <= 0) return;
            
            var screenPoints = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                screenPoints[i] = UI.AimInstance.RawRaycastPoint;
            }
            
            var hitZoneTypes = HumanRecognitionVisualizer.Instance.ProcessRaycast(screenPoints, out var distance);
            
            if (distance < 0.05f) return;
            
            for (var i = 0; i < count; i++)
            {
                if ((HitZone.ZoneType)hitZoneTypes[i] == HitZone.ZoneType.None) continue;
                
                var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
                                                    
                Pool.Decals.ActivateHitMarker(currentRays[i].GetPoint(distance), damage, (HitZone.ZoneType)hitZoneTypes[i]);
                
                // Debug.Log(hitZoneTypes[i]);
            }
        }
    }
}
