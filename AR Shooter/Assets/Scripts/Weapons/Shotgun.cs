using UnityEngine;

namespace Game.Weapons
{
    public class Shotgun : MainWeapon
    {
        private const WeaponName CurrentWeaponName = WeaponName.Shotgun;
        public override WeaponName WeaponName => CurrentWeaponName;

        private const WeaponType CurrentWeaponType = WeaponType.Heavy;
        public override WeaponType WeaponType => CurrentWeaponType;

        protected override void RunWeaponLogic()
        {
            var currentRays = new Ray[4];
            for (var i = 0; i < currentRays.Length; i++)
            {
                currentRays[i] = Aim.GetRay();
            }
            
            ShootingPatterns.ProcessRays(currentRays);
            
            Aim.AimAnimation();
        }
        
        //protected override bool LogicIsRunning() =>
        //    shootAnimator.isPlaying;

        private void OnShellAnimation() =>
            shellsParticle.Play();
        
        protected override void VisualizeFiring()
        {
            //shootAnimator.Play();
            flashParticle.Play(true);
            
            if (AudioSource is null) return;
            
            AudioSource.loop = false;
            AudioSource.volume = 1f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
