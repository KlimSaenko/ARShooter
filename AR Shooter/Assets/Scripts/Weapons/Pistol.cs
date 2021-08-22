using Mobs;
using UnityEngine;

namespace Weapons
{
    public class Pistol : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.Pistol;
        public override WeaponType WeaponType => CurrentWeaponType;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            var currentRay = UI.AimInstance.GetRay();
            
            ShootingPatterns.ProcessRays(currentRay);
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
        
        protected override void VisualizeFiring()
        {
            shellsParticle.Play();
            shootAnimation.Play();
            flashParticle.Play(true);
            Vibration.VibratePop();
            
            if (AudioSource is null) return;

            AudioSource.loop = false;
            AudioSource.volume = 0.8f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
