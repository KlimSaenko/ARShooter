using UnityEngine;
using MoreMountains.NiceVibrations;

namespace Game.Weapons
{
    public class Pistol : MainWeapon
    {
        private const WeaponName CurrentWeaponName = WeaponName.Pistol;
        public override WeaponName WeaponName => CurrentWeaponName;

        private const WeaponType CurrentWeaponType = WeaponType.Light;
        public override WeaponType WeaponType => CurrentWeaponType;

        protected override void RunWeaponLogic()
        {
            prevNoize = Mathf.Clamp(prevNoize + 0.32f * (Random.value - 0.5f), 0.15f, 0.85f);
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
            AudioSource.volume = 0.8f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
