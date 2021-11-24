using System;
using System.Collections;
using DG.Tweening;
using Game.Mobs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using MoreMountains.NiceVibrations;
using Game.UI;

namespace Game.Weapons
{
    public class MainWeapon : MonoBehaviour, IWeapon
    {
        [SerializeField] private protected Animator stateMachine;
        private protected static readonly int blendId = Animator.StringToHash("Blend");
        private protected static readonly int noizeId = Animator.StringToHash("Noize");

        private protected float prevNoize = 0.5f;

        [Header("Weapon config")]
        [SerializeField] private protected WeaponStats weaponStats;
        [SerializeField] private protected HapticTypes hapticType;

        [Space]
        [SerializeField] private Transform model;
        [SerializeField] private Transform magazine;

        public string Name { get; set; }

        public virtual WeaponName WeaponName => WeaponName.Unsigned;
        public virtual WeaponType WeaponType => WeaponType.Unsigned;

        public void SetConfig(SO.WeaponsList weaponsList)
        {
            weaponStats = weaponsList.CurrentWeaponStats(WeaponName, out var name);
            Name = name;

            WeaponsListAccess.AddWeaponPrefab(this);
        }

        public MainWeapon InstantiateWeapon(Transform saveFolder, int index, TextMeshPro bulletsText)
        {
            var script = Instantiate(gameObject, saveFolder).GetComponent<MainWeapon>();
            script.bulletsText = bulletsText;
            script.SetWeaponBehaviour(index);

            return script;
        }

        internal Action<int, int> bulletsUpdateAction;

        private int _totalBulletCount;
        private int _bulletCount;
        private protected int BulletCount
        {
            get => _bulletCount;
            set
            {
                var delta = _bulletCount - value;
                if (_totalBulletCount > 0 && delta > 0) _totalBulletCount -= delta;
                _bulletCount = value;

                bulletsUpdateAction?.Invoke(value, _totalBulletCount);

                CanShoot = ValidateBullets(value);
            }
        }

        private protected bool CanShoot;

        private protected static bool HapticsSupported;
        
        [Space]
        [Header("Weapon Attachments")]
        [SerializeField] protected ParticleSystem shellsParticle;
        [SerializeField] protected ParticleSystem flashParticle;
        [SerializeField] protected AudioClip shootAudio;
        
        private protected static AudioSource AudioSource;

        private protected CommonUI.Aim Aim;

        #region Setters

        private void OnEnable()
        {
            _reloadState ??= new ReloadState(OnChangeValue, reloadSlider);
            _bulletUI ??= new BulletUI(bulletsText);

            DynamicHolder.BulletsLabelRef = bulletUIRef;
            
            bulletsUpdateAction += _bulletUI.UpdateCount;
            PlayerBehaviour.FiringAction += Fire;
            PlayerStates.WeaponReadyAction += SetReady;

            bulletsUpdateAction?.Invoke(BulletCount, _totalBulletCount);
        }
        
        private void OnDisable()
        {
            bulletsUpdateAction -= _bulletUI.UpdateCount;
            PlayerBehaviour.FiringAction -= Fire;
            PlayerStates.WeaponReadyAction -= SetReady;
        }

        private void SetWeaponBehaviour(int index)
        {
            Aim = new CommonUI.Aim(weaponStats);
            reloadId = Animator.StringToHash("Reload " + (int)WeaponName);

            weaponSwitchButton = weaponSwitchButton.SetButton(index + 1);
            bulletsUpdateAction += weaponSwitchButton.OnUpdateBullets;

            _reloadState ??= new ReloadState(OnChangeValue, reloadSlider);
            _bulletUI ??= new BulletUI(bulletsText);
            _totalBulletCount = weaponStats.allBullets;
            BulletCount = weaponStats.bulletInMagazineCount;

            HapticsSupported = MMVibrationManager.HapticsSupported();

            if (shootAudio == null || AudioSource != null) return;

            AudioSource = transform.parent.parent.gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }

        private void SetReady(bool ready)
        {
            Fire(ready && PlayerBehaviour.Firing);
        }
        
        #endregion

        protected virtual void VisualizeFiring()
        {
            
        }
        
        #region Fire

        private static readonly int shootingId = Animator.StringToHash("Shooting");

        private void Fire(bool fire)
        {
            if (fire && !CanShoot)
            {
                ValidateBullets(BulletCount);

                return;
            }

            stateMachine.SetBool(shootingId, fire);
        }

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

        protected virtual void RunWeaponLogic() { }

        #endregion

        [Header("UI")]

        [SerializeField] internal WeaponSwitchButton weaponSwitchButton;

        #region Reload

        [SerializeField] private Slider reloadSlider;

        private ReloadState _reloadState;

        private bool ValidateBullets(int currentCount)
        {
            var valid = currentCount > 0;

            if (!valid)
            {
                Fire(false);

                if (_totalBulletCount != 0) StartReload();
                else Managers.NotificationManager.Notification(Managers.NotificationTypes.Info, "No ammo.");
            }

            return valid;
        }

        private int reloadId;
        private readonly static int localReloadId = Animator.StringToHash("Reload");

        private void StartReload()
        {
            Fire(false);

            PlayerBehaviour.PlayerStateMachine.SetBool(reloadId, true);
            stateMachine.SetBool(localReloadId, true);

            model.localEulerAngles = new Vector3(0, 0, 4);
            magazine.localPosition = new Vector3(0, -0.16f);
            _reloadState.StartReload();
        }

        private void CompleteReload()
        {
            BulletCount = weaponStats.bulletInMagazineCount;
            model.localEulerAngles = Vector3.zero;
            magazine.localPosition = Vector3.zero;

            _reloadState.Reloading = false;

            if (HapticsSupported) MMVibrationManager.Haptic(hapticType);

            stateMachine.SetBool(localReloadId, false);
            PlayerBehaviour.PlayerStateMachine.SetBool(reloadId, false);
        }

        private void OnChangeValue(float value)
        {
            if (!_reloadState.Reloading) return;

            model.localEulerAngles = new Vector3(0, 0, 8 * (0.5f - value));
            magazine.localPosition = Vector3.Lerp(new Vector3(0, -0.16f), Vector3.zero, value);

            if (value > 0.995f) CompleteReload();
        }
        
        private class ReloadState
        {
            private readonly Slider _reloadSlider;

            private readonly EventTrigger _eventTrigger;
            private readonly EventTrigger.Entry _entry;

            internal bool Reloading;
        
            internal ReloadState(UnityEngine.Events.UnityAction<float> action, Slider reloadSlider)
            {
                _reloadSlider = reloadSlider;
                _reloadSlider.onValueChanged.AddListener(action);

                _eventTrigger = _reloadSlider.gameObject.TryGetComponent(out EventTrigger eventTrigger) 
                    ? eventTrigger : _reloadSlider.gameObject.AddComponent<EventTrigger>();
                _entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                _entry.callback.AddListener( _ => { StopReload(); } );

                _eventTrigger.triggers.Add(_entry);
            }

            internal void StartReload()
            {
                Reloading = true;
                _reloadSlider.value = 0;
            }

            private void StopReload()
            {
                if (!Reloading) return;

                _reloadSlider.DOValue(0, _reloadSlider.value * 0.6f).SetEase(Ease.OutSine);
            }
        }
        
#endregion

        #region Bullet Count UI
        
        private BulletUI _bulletUI;

        [Space]
        [SerializeField] private Transform bulletUIRef;
        internal TextMeshPro bulletsText;

        private class BulletUI
        {
            private readonly TextMeshPro _bulletText;

            internal BulletUI(TextMeshPro bulletText)
            {
                _bulletText = bulletText;

                //bulletsUpdateAction += UpdateCount;
            }

            internal void UpdateCount(int bullets, int totalBullets)
            {
                _bulletText.text = bullets.ToString();
            }
        }

        #endregion
    }
    
    public enum WeaponName
    {
        Unsigned,
        Pistol,
        M4Carabine,
        Shotgun,
        Flamethrower
    }

    public enum WeaponType
    {
        Unsigned,
        Light,
        Medium,
        Heavy
    }

    [Serializable]
    public struct WeaponStats
    {
        //public WeaponStats(int damageMin, int damageMax, int aimedAimSpreadDiameter, int freeAimSpreadDiameter, float aimSpreadIncrement, float aimRecoveryTime, 
        //    int bulletCount)
        //{
        //    this.damageMin = damageMin;
        //    this.damageMax = damageMax;
        //    this.aimedAimSpreadDiameter = aimedAimSpreadDiameter;
        //    this.freeAimSpreadDiameter = freeAimSpreadDiameter;
        //    this.aimSpreadIncrement = aimSpreadIncrement;
        //    this.aimRecoveryTime = aimRecoveryTime;
        //    this.bulletCount = bulletCount;
        //}

        public int damageMin, damageMax;

        [Range(0.01f, 1)]
        public float aimSpreadIncrement;

        [Range(0.01f, 5)]
        public float aimRecoveryTime;

        public int bulletInMagazineCount, allBullets;
    }

    internal class Shooting
    {
        private protected readonly WeaponStats WeaponStats;
        private readonly RaycastHit[] _hitsBuffer;

        internal Shooting(WeaponStats weaponStats)
        {
            WeaponStats = weaponStats;
            _hitsBuffer = new RaycastHit[8];
        }
        
        internal virtual void ProcessRays(Ray currentRay)
        {
            var hitsCount = Physics.RaycastNonAlloc(currentRay, _hitsBuffer);
            if (hitsCount < 1 || !_hitsBuffer[0].transform.TryGetComponent(out HitZone hitZone)) return;
            
            ApplyDamage(hitZone, _hitsBuffer[0].point);
        }
        
        internal virtual void ProcessRays(Ray[] currentRays)
        {
            foreach (var currentRay in currentRays)
            {
                ProcessRays(currentRay);
            }
        }

        internal virtual void ProcessCapsuleRay(float radius, float length)
        {
            var mainCam = Camera.main.transform;
            var forward = mainCam.forward;
            var camPosition = mainCam.position;
                
            var hitsCount = Physics.CapsuleCastNonAlloc(camPosition + forward * 0.5f, camPosition + forward * length,
                radius, forward, _hitsBuffer, 0);
    
            for (var i = 0; i < hitsCount; i++)
            {
                if (!_hitsBuffer[i].collider.TryGetComponent(out HitZone hitZone)) continue;
                
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
                radius, forward, _hitsBuffer, 0);

            float time = 0;
            for (var i = 0; i < hitsCount; i++)
            {
                time = Vector3.Distance(camPosition, _hitsBuffer[i].collider.transform.position) * delay - time;
                
                yield return new WaitForSeconds(time);

                if (_hitsBuffer[i].collider.TryGetComponent(out HitZone hitZone)) ApplyDamage(hitZone);
            }
        }

        private void ApplyDamage(HitZone hitZone)
        {
            var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
            
            hitZone.ApplyDamage(damage);
        }
        
        private void ApplyDamage(HitZone hitZone, Vector3 hitPos)
        {
            var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
            
            hitZone.ApplyDamage(damage, hitPos);
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
            //var count = currentRays.Length;
            
            //if (count <= 0) return;
            
            //var screenPoints = new Vector2[count];
            //for (var i = 0; i < count; i++)
            //{
            //    screenPoints[i] = UI.AimInstance.RawRaycastPoint;
            //}
            
            //var hitZoneTypes = HumanRecognitionVisualizer.Instance.ProcessRaycast(screenPoints, out var distance);
            
            //if (distance < 0.05f) return;
            
            //for (var i = 0; i < count; i++)
            //{
            //    if ((HitZone.ZoneType)hitZoneTypes[i] == HitZone.ZoneType.None) continue;
                
            //    var damage = Random.Range(WeaponStats.damageMin, WeaponStats.damageMax + 1);
                                                    
            //    Pool.Decals.ActivateHitMarker(currentRays[i].GetPoint(distance), damage, (HitZone.ZoneType)hitZoneTypes[i]);
                
            //    // Debug.Log(hitZoneTypes[i]);
            //}
        }
    }
}
