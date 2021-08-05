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

    public interface IWeaponConfig
    {
        Vector3 PosToAim { get; }
        
        Vector3 PosFromAim { get; }
        
        WeaponType WeaponType { get; }

        bool IsActive { get; }

        void SetActive(bool value);
    }

    [Serializable]
    public struct WeaponStats
    {
        public WeaponStats(int damage, int aimedAimSpreadDiameter, int freeAimSpreadDiameter, int aimSpreadIncrement, float aimRecoveryTime)
        {
            Damage = damage;
            AimedAimSpreadDiameter = aimedAimSpreadDiameter;
            FreeAimSpreadDiameter = freeAimSpreadDiameter;
            AimSpreadIncrement = aimSpreadIncrement;
            AimRecoveryTime = aimRecoveryTime;
        }

        public int Damage;

        public int AimedAimSpreadDiameter, FreeAimSpreadDiameter, AimSpreadIncrement;

        public float AimRecoveryTime;
    }
}
