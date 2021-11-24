using MoreMountains.NiceVibrations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Weapons
{
    public class M4_Carbine : MainWeapon
    {
        private const WeaponName CurrentWeaponName = WeaponName.M4Carabine;
        public override WeaponName WeaponName => CurrentWeaponName;

        private const WeaponType CurrentWeaponType = WeaponType.Medium;
        public override WeaponType WeaponType => CurrentWeaponType;

        protected override void RunWeaponLogic()
        {
            prevNoize = Mathf.Clamp01(prevNoize + 0.4f * (Random.value - 0.5f));
            stateMachine.SetFloat(blendId, prevNoize);
            stateMachine.SetTrigger(noizeId);

            var currentRay = Aim.GetRay();
            ShootingPatterns.ProcessRays(currentRay);

            BulletCount--;

            VisualizeFiring();
            Aim.AimAnimation();
        }
        
        protected override void VisualizeFiring()
        {
            shellsParticle.Play();
            flashParticle.Play(true);

            if (HapticsSupported) MMVibrationManager.Haptic(hapticType);

            if (AudioSource is null) return;
            
            AudioSource.loop = false;
            AudioSource.volume = 1f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
