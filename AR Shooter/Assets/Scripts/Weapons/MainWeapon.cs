using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class MainWeapon : MonoBehaviour
{   
    private Camera mainCam;
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

    protected bool isShoot = false;

    private int damage;
    private Vector3 standartWeaponPos;
    private Quaternion standartWeaponRot;

    private void Awake()
    {
        standartWeaponPos = transform.localPosition;
        standartWeaponRot = transform.localRotation;
    }

    private void Start()
    {
        damage = Config.GetStats(weaponType).Value.damage;
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        transform.localPosition = standartWeaponPos;
        transform.localRotation = standartWeaponRot;
    }

    public void Shoot(bool start, bool isMine = true)
    {
        isShoot = start;
        if (start && !IsAnimating())
        {
            StartCoroutine(Shooting(isMine));
        }
    }

    private IEnumerator Shooting(bool isMine)
    {
        if (isMine)
        {
            while (isShoot)
            {
                shellsParticle.Play();
                shootAnimation.Play();
                flashParticle.Play(true);
                audioSource.pitch = Random.Range(0.94f, 1.06f);
                audioSource.PlayOneShot(shootAudio);

                Vector3 startPos = UI.weaponHolderScript.isAimed ? aim.position : virtualAim.position + new Vector3(Random.Range(-0.003f, 0.003f), Random.Range(-0.003f, 0.003f));

                if (Physics.Raycast(startPos, (startPos - mainCam.transform.position).normalized, out RaycastHit hitInfo) && hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
                {
                    hitZone.ApplyDamage(damage, hitInfo.point);
                }

                yield return new WaitWhile(IsAnimating);
            }
        }
        else
        {
            while (isShoot)
            {
                shellsParticle.Play();
                shootAnimation.Play();
                flashParticle.Play(true);

                yield return new WaitWhile(IsAnimating);
            }
        }
    }

    protected bool IsAnimating()
    {
        return shootAnimation.isPlaying;
    }
}
