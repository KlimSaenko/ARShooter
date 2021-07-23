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

        protected override void RunWeaponLogic()
        {
            var currentRay = UI.AimInstance.StartAnim();
            
            // var startPos = UI.weaponHolderScript.isAimed ? aim.position : virtualAim.position + new Vector3(Random.Range(-0.003f, 0.003f), Random.Range(-0.003f, 0.003f));
            // var startBulletPos = virtualAim.position;

            if (Physics.Raycast(currentRay, out var hitInfo) && 
                hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
            {
                hitZone.ApplyDamage(Damage, hitInfo.point);
            }
            // return shootAnimation.isPlaying;
        }
        
        private bool IsRunning() =>
            shootAnimation.isPlaying;
    }
}
