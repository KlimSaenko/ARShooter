using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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

        internal int BulletCount
        {
            get => weaponStats.bulletCount;
            private set
            {
                BulletUI.UpdateCount(value);
                
                weaponStats.bulletCount = value;
            }
        }
        
        [Header("Weapon Attachments")]
        [SerializeField] protected ParticleSystem shellsParticle;
        [SerializeField] protected ParticleSystem flashParticle;
        [SerializeField] protected Animation shootAnimation;
        [SerializeField] protected AudioClip shootAudio;
        
        protected AudioSource AudioSource;
        private bool _isShoot;
        
        public bool IsActive => gameObject.activeSelf;
        
        public void SetActive(bool value)
        {
            if (value) ActiveWeaponStats = weaponStats;
            gameObject.SetActive(value);
        }

        protected void Shoot(bool start)
        {
            if (!IsActive) return;
            
            _isShoot = start;
            if (start && !LogicIsRunning())
            {
                StartCoroutine(Shooting());
            }
        }

        private IEnumerator Shooting()
        {
            while (_isShoot)
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

        protected virtual void RunWeaponLogic()
        {
            
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
