using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class Carbine : MainWeapon
    {
        private void Awake()
        {
            PlayerBehaviour.FiringAction += delegate(bool start) { Shoot(start); };
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            var currentRay = UI.AimInstance.StartAnim();

            if (Physics.Raycast(currentRay, out var hitInfo) && 
                hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
            {
                hitZone.ApplyDamage(Damage, hitInfo.point);
            }
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
    }
}
