using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Game.Mobs;
using Game.SO;

namespace Game.Managers
{
    public sealed class TestScene : GameSceneLogic
    {
        [Space, Range(0.1f, 3)]
        [SerializeField] private float raycastDistance = 2;

        [Space]
        [SerializeField] private PauseMenu pauseMenu;

        private static MainMob _mobScript;

        private protected override IEnumerator MainSequence()
        {
            showLookPoint = true;
            NotificationManager.Notification(NotificationTypes.Hint, "You can find up more surfaces before go ahead.");
            yield return new WaitUntil(() => GameplayActive);

            // delete
            //ActivateWeaponButtonsAction?.Invoke(Weapons.WeaponType.Light);
            //yield return new WaitUntil(() => Mobs.TestMob.PercentHP < 0.75f);
            //ActivateWeaponButtonsAction?.Invoke(Weapons.WeaponType.Medium);
            //

            NotificationManager.Notification(NotificationTypes.Hint, "Stand in front of a free surface.", false);
            yield return DetectMobPos();

            NotificationManager.Notification(NotificationTypes.Hint, "Keep the distance to the enemy spawn place.", false);
            yield return new WaitUntil(_mobScript.Loaded);

            showLookPoint = false;
            yield return new WaitForSeconds(3);
            
            OnActivateWeaponButtons(Weapons.WeaponType.Light);
            NotificationManager.Notification(NotificationTypes.Hint, "Here's your first weapon.");
            yield return new WaitUntil(() => _mobScript.PercentHP < 0.75f);

            OnActivateWeaponButtons(Weapons.WeaponType.Medium);
            NotificationManager.Notification(NotificationTypes.Hint, "Try the another one.");
            yield return new WaitUntil(() => _mobScript.PercentHP <= 0);

            NotificationManager.Notification(NotificationTypes.Hint, "Mob is dead.");
            yield return new WaitForSeconds(2);

            var weapons = new List<Weapons.WeaponName> { Weapons.WeaponName.Pistol, Weapons.WeaponName.M4Carabine };
            CurrentAward = new EndGameAward(weapons, 10, 10);

            pauseMenu.GameEnd();
        }

        private IEnumerator DetectMobPos()
        {
            yield return new WaitUntil(MainCameraReady);

            Vector3 mobPos;
            while (!FindMobPos(out mobPos))
                yield return new WaitForEndOfFrame();

            _mobScript = Pool.Mobs.ActivateMob(mobPos);
            //Pool.Mobs.ActivateMob(new Vector3(0, -0.8f, 3));
        }

        private static readonly List<ARRaycastHit> _hitsBuffer = new();
        private bool FindMobPos(out Vector3 mobPos)
        {
            var forward = _mainCamera.forward;
            forward = new Vector3(forward.x, 0, forward.z).normalized * raycastDistance;
            var circleRandom = 0.9f * Random.insideUnitCircle;
            forward += new Vector3(circleRandom.x, 0, circleRandom.y) + _mainCamera.position;

            if (!raycastManager.Raycast(new Ray(forward, Vector3.down), _hitsBuffer, TrackableType.PlaneWithinPolygon))
            {
                mobPos = Vector3.zero;
                return false;
            }

            var mobPose = _hitsBuffer[0].pose;
            mobPos = mobPose.position;

            float angle;
            var additionalRays = new List<ARRaycastHit>();

            for (var i = 0; i < 3; i++)
            {
                angle = (i * 120 - 90 - _mainCamera.eulerAngles.y) * Mathf.Deg2Rad;
                var ray = new Ray(forward + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.35f, Vector3.down);

                if (!raycastManager.Raycast(ray, additionalRays, TrackableType.PlaneWithinPolygon) 
                    || Mathf.Abs(additionalRays[0].pose.position.y - mobPos.y) > 0.1f)
                    return false;
            }
            return true;
        }
    }
}
