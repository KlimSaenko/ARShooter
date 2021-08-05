using Mobs;
using UnityEngine;

namespace Weapons
{
    public class M4_Carbine : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.M4_Carabine;

        public override WeaponType WeaponType => CurrentWeaponType;
        
        private void Awake()
        {
            PlayerBehaviour.FiringAction += Shoot;
            
            ActiveWeaponStats = weaponStats;
            
            if (shootAudio is null) return;

            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.maxDistance = 20;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            var currentRay = UI.AimInstance.GetRay();
            
            if (Physics.Raycast(currentRay, out var hitInfo) && 
                hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
            {
                hitZone.ApplyDamage(4, hitInfo.point);
            }
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
        
        protected override void VisualizeFiring()
        {
            shellsParticle.Play();
            shootAnimation.Play();
            flashParticle.Play(true);
            
            if (AudioSource is null) return;
            
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
