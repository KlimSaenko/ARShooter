using System.Collections;
using UnityEngine;
using static Config;

namespace Weapons
{
    public class MainWeapon : MonoBehaviour
    {   
        private Camera _mainCam;
        [SerializeField] private Transform aim, virtualAim;

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

        private int _damage;
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
            _damage = GetStats(weaponType).Value.Damage;
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
            if (start && !IsAnimating())
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
                    shellsParticle.Play();
                    shootAnimation.Play();
                    flashParticle.Play(true);
                    audioSource.pitch = Random.Range(0.94f, 1.06f);
                    audioSource.PlayOneShot(shootAudio);

                    Vector3 startPos = UI.weaponHolderScript.isAimed ? aim.position : virtualAim.position + new Vector3(Random.Range(-0.003f, 0.003f), Random.Range(-0.003f, 0.003f));

                    if (Physics.Raycast(startPos, (startPos - _mainCam.transform.position).normalized, out RaycastHit hitInfo) && hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
                    {
                        hitZone.ApplyDamage(_damage, hitInfo.point);
                    }

                    yield return new WaitWhile(IsAnimating);
                }
            }
            else
            {
                while (_isShoot)
                {
                    shellsParticle.Play();
                    shootAnimation.Play();
                    flashParticle.Play(true);

                    yield return new WaitWhile(IsAnimating);
                }
            }
        }

        private bool IsAnimating()
        {
            return shootAnimation.isPlaying;
        }
    }
}
