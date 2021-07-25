using System;
using System.Collections;
using UnityEngine;
using static Config;

namespace Weapons
{
    public class MainWeapon : MonoBehaviour, IWeaponConfig
    {   
        private Camera _mainCam;
        [SerializeField] protected Transform virtualAim;
        
        [Header("Weapon config")]
        [SerializeField] private Vector3 posToAim;
        [SerializeField] private Vector3 posFromAim;
        [SerializeField] private WeaponStats weaponStats;

        public Vector3 PosToAim => posToAim;
        public Vector3 PosFromAim => posFromAim;

        internal enum WeaponType
        {
            AKM = 0,
            Sniper
        }

        [SerializeField] internal WeaponType weaponType;

        [Header("Weapon Attachments")]
        [SerializeField] protected ParticleSystem shellsParticle;
        [SerializeField] protected ParticleSystem flashParticle;
        [SerializeField] protected Animation shootAnimation;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip shootAudio;

        private bool _isShoot;

        protected int Damage;
        private Vector3 _standardWeaponPos;
        private Quaternion _standardWeaponRot;

        private void Awake()
        {
            var thisTransform = transform;
            _standardWeaponPos = thisTransform.localPosition;
            _standardWeaponRot = thisTransform.localRotation;
        }

        private void Start()
        {
            Damage = GetStats(weaponType).Value.Damage;
            _mainCam = Camera.main;
        }

        private void OnEnable()
        {
            var thisTransform = transform;
            thisTransform.localPosition = _standardWeaponPos;
            thisTransform.localRotation = _standardWeaponRot;
        }

        public void Shoot(bool start, bool isMine = true)
        {
            _isShoot = start;
            if (start && !LogicIsRunning())
            {
                StartCoroutine(Shooting(isMine));
            }
        }

        private IEnumerator Shooting(bool isMine)
        {
            if (isMine)
            {
                while (_isShoot)
                {
                    VisualizeFiring();

                    RunWeaponLogic();
                    
                    yield return new WaitWhile(LogicIsRunning);
                }
            }
            else
            {
                while (_isShoot)
                {
                    VisualizeFiring();

                    yield return new WaitWhile(LogicIsRunning);
                }
            }
        }

        protected virtual bool LogicIsRunning() => false;

        private void VisualizeFiring()
        {
            shellsParticle.Play();
            shootAnimation.Play();
            flashParticle.Play(true);
            // audioSource.pitch = Random.Range(0.94f, 1.06f);
            // audioSource.PlayOneShot(shootAudio);
            WeaponStats = new WeaponStats(3, 0.3f);
        }

        protected virtual void RunWeaponLogic()
        {
            
        }

        public WeaponStats WeaponStats { get; set; }
    }

    internal interface IWeaponConfig
    {
        WeaponStats WeaponStats { get; set; }
        
        Vector3 PosToAim { get; }
        
        Vector3 PosFromAim { get; }
    }

    [Serializable]
    public struct WeaponStats
    {
        public WeaponStats(int damage, float spreadRadius)
        {
            Damage = damage;
            SpreadRadius = spreadRadius;
        }

        public int Damage;

        public float SpreadRadius;
    }
}
