using Common;
using Mobs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class M4_Carbine : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.M4_Carabine;
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
            AudioSource.volume = 1f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
