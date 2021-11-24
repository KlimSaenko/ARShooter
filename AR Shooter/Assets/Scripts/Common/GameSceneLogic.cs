using Game.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Game.Managers
{
    public class GameSceneLogic : MonoBehaviour
    {
        [SerializeField] private Animator mainStateMachine;

        [Space]
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARPointCloudManager pointCloudManager;
        [SerializeField] private protected ARRaycastManager raycastManager;

        private protected static bool GameplayActive = false;

        private static readonly int scanningId = Animator.StringToHash("Scanning");
        private IEnumerator Start()
        {
            yield return new WaitUntil(PlanesFound);

            mainStateMachine.SetBool(scanningId, false);

            StartCoroutine(MainSequence());
        }

        private protected virtual IEnumerator MainSequence()
        {
            yield return new WaitUntil(() => GameplayActive);

            ActivateWeaponButtonsAction?.Invoke(WeaponType.Light);
            ActivateWeaponButtonsAction?.Invoke(WeaponType.Medium);
            ActivateWeaponButtonsAction?.Invoke(WeaponType.Heavy);

            yield break;
        }

        private bool PlanesFound() => planeManager && planeManager.trackables.count > 0;
        //private bool PlanesFound() => true;

        internal static Action<WeaponType> ActivateWeaponButtonsAction;

        private static readonly int gameplayId = Animator.StringToHash("Gameplay");
        public void SetGameplay(bool start)
        {
            mainStateMachine.SetBool(gameplayId, start);
            GameplayActive = start;

            //planeManager.SetTrackablesActive(false);
            //planeManager.enabled = false;
            foreach (var trackable in planeManager.trackables)
            {
                var meshRenderer = trackable.GetComponent<MeshRenderer>();
                var newMaterials = new Material[] { meshRenderer.materials[0] };
                meshRenderer.materials = newMaterials;
            }

            pointCloudManager.SetTrackablesActive(false);
            pointCloudManager.enabled = false;
            //if (start) OnWeaponSwitchAction(1);
        }

        [Space]
        [SerializeField] private GameObject lookMarker;

        private protected bool showLookPoint;

        private protected readonly List<ARRaycastHit> hitsBuffer = new();

        private void Update()
        {
            if (showLookPoint)
            {
                var screenCenter = new Vector2(Screen.width, Screen.height) / 2;

                if (raycastManager.Raycast(screenCenter, hitsBuffer, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                {
                    lookMarker.SetActive(true);
                    lookMarker.transform.position = Vector3.Lerp(lookMarker.transform.position, hitsBuffer[0].pose.position, 12 * Time.deltaTime);
                }
            }
            else lookMarker.SetActive(false);
        }
    }
}
