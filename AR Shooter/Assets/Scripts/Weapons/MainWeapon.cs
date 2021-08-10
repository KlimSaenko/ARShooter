using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        private UI.Reload _reloadState;

        private int _bulletCount;

        private int BulletCount
        {
            get => _bulletCount;
            set
            {
                BulletUI.UpdateCount(value);
                
                _bulletCount = value;
                
                if (value <= 0)
                {
                    StartReload();
                }
            }
        }

        private protected void SetWeaponBehaviour()
        {
            PlayerBehaviour.FiringAction += Shoot;
            // PlayerBehaviour.WeaponSwitchAction += weaponType => StartReload();
            // _reloadState = new UI.Reload(CompleteReload, reloadSlider);
            
            SetActive(true);
            BulletCount = weaponStats.bulletCount;
            
            if (shootAudio is null) return;

            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }

        private bool _isFiring;
        
        private bool CanShoot => IsActive && !_reloadState.IsReloading && !LogicIsRunning();
        
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
                ActiveWeaponStats = weaponStats;
                BulletUI.UpdateCount(BulletCount);

                _reloadState ??= new UI.Reload(CompleteReload, reloadSlider);
                
                if (_reloadState.IsReloading) StartReload();
            }
            else if (_reloadState.IsReloading) StartReload();
            
            gameObject.SetActive(value);
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

        private void StartReload()
        {
            _reloadState.StartReload();
        }

        private void CompleteReload()
        {
            if (_reloadState.IsReloading) _reloadState.StopReload(false);
            BulletCount = weaponStats.bulletCount;
        }
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
