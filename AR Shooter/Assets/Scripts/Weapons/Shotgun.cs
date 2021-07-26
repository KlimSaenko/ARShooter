using Mobs;
using UnityEngine;

namespace Weapons
{
    public class Shotgun : MainWeapon
    {
        private void Awake()
        {
            PlayerBehaviour.FiringAction += delegate(bool start) { Shoot(start); };
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            for (var i = 0; i < 4; i++)
            {
                var currentRay = UI.AimInstance.GetRay();
                
                if (Physics.Raycast(currentRay, out var hitInfo) && 
                    hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
                {
                    hitZone.ApplyDamage(Damage, hitInfo.point);
                }
            }
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
    }
}
