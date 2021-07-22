using System.Collections;
using UnityEngine;
using static Config;

namespace Weapons
{
    public class MainWeapon : MonoBehaviour
    {   
        private Camera _mainCam;
        [SerializeField] protected Transform virtualAim;

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
            if (start && !IsRunning())
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

                    // var startPos = UI.weaponHolderScript.isAimed ? aim.position : virtualAim.position + new Vector3(Random.Range(-0.003f, 0.003f), Random.Range(-0.003f, 0.003f));
                    // var startBulletPos = virtualAim.position;

                    // if (Physics.Raycast(startPos, (startPos - _mainCam.transform.position).normalized, out var hitInfo) && hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
                    // {
                    //     hitZone.ApplyDamage(_damage, hitInfo.point);
                    // }
                    
                    yield return new WaitWhile(IsRunning);
                    // yield return StartCoroutine(RunGunLogic());
                }
            }
            else
            {
                while (_isShoot)
                {
                    VisualizeFiring();

                    yield return new WaitWhile(IsRunning);
                }
            }
        }

        private bool IsRunning() =>
            shootAnimation.isPlaying;

        protected virtual void VisualizeFiring()
        {
            shellsParticle.Play();
            shootAnimation.Play();
            flashParticle.Play(true);
            // audioSource.pitch = Random.Range(0.94f, 1.06f);
            // audioSource.PlayOneShot(shootAudio);
        }

        protected virtual void RunWeaponLogic()
        {
            
        }
    }
}
