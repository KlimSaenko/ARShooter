using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Game.Managers
{
    public sealed class TestScene : GameSceneLogic
    {
        [Space, Range(0.1f, 3)]
        [SerializeField] private float raycastDistance = 2;

        private static Transform _mainCamera;
        private bool MainCameraReady()
        {
            var ready = _mainCamera != null;
            if (ready) return ready;

            _mainCamera = Camera.main.transform;
            return ready;
        }

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
            yield return new WaitUntil(Mobs.TestMob.Loaded);

            showLookPoint = false;
            yield return new WaitForSeconds(3);

            ActivateWeaponButtonsAction?.Invoke(Weapons.WeaponType.Light);
            NotificationManager.Notification(NotificationTypes.Hint, "Here's your first weapon.");
            yield return new WaitUntil(() => Mobs.TestMob.PercentHP < 0.75f);

            ActivateWeaponButtonsAction?.Invoke(Weapons.WeaponType.Medium);
            NotificationManager.Notification(NotificationTypes.Hint, "Try the another one.");
            yield return new WaitUntil(() => Mobs.TestMob.PercentHP <= 0);

            NotificationManager.Notification(NotificationTypes.Hint, "Mob is dead.");
            yield return new WaitForSeconds(2);

            
        }

        private IEnumerator DetectMobPos()
        {
            yield return new WaitUntil(MainCameraReady);

            while (!FindMobPos())
                yield return new WaitForEndOfFrame();

            Pool.Mobs.ActivateMob(hitsBuffer[0].pose.position);
            //Pool.Mobs.ActivateMob(new Vector3(0, -0.8f, 3));
        }

        private readonly List<ARRaycastHit> _hitsBuffer = new();
        private readonly Ray[] _additionalRays = new Ray[3];
        private bool FindMobPos()
        {
            var forward = _mainCamera.forward;
            forward = new Vector3(forward.x * Random.Range(0.7f, 1.3f), 0, forward.z * Random.Range(0.7f, 1.3f)).normalized * raycastDistance;
            
            if (!raycastManager.Raycast(new Ray(forward + _mainCamera.position, Vector3.down), _hitsBuffer, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                return false;

            float angle;

            for (var i = 0; i < 3; i++)
            {
                angle = (i * 120 - 90 - _mainCamera.eulerAngles.y) * Mathf.Deg2Rad;
                _additionalRays[i] = new Ray(forward + _mainCamera.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.35f, Vector3.down);
            }

            foreach (Ray ray in _additionalRays)
            {
                if (!raycastManager.Raycast(ray, null, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                    return false;
            }

            return true;
        }

        //private void Update()
        //{
        //    if (MainCameraReady()) FindMobPos();
        //}
    }
}
