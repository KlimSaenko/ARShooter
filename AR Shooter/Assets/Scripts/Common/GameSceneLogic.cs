using Game.SO;
using Game.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
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
        [SerializeField] private GameObject gameplayARPlanePrefab;
        [SerializeField] private VideoPlayer videoPlayerSearch;

        private protected bool GameplayActive = false;

        private static EndGameAward _currentAward;
        internal static EndGameAward CurrentAward
        {
            get => _currentAward;
            set
            {
                _currentAward = value;

                ShowWeapon.SetNewWeapon(value.NewWeapons);
                PlayerProgress.ChangeAllowableWeapons(value.NewWeapons);
            }
        }

        private protected static Transform _mainCamera;
        private protected bool MainCameraReady()
        {
            if (_mainCamera != null) return true;

            _mainCamera = Camera.main.transform;
            return _mainCamera != null;
        }
        
        private static readonly int scanningId = Animator.StringToHash("Scanning");
        private IEnumerator Start()
        {
            videoPlayerSearch.prepareCompleted += (VideoPlayer source) =>
            {
                mainStateMachine.SetBool(scanningId, true);
                _videoPlayerReady = true;
            };

            yield return new WaitUntil(PlanesFound);
            //yield return new WaitUntil(() => _videoPlayerReady);
            yield return new WaitForEndOfFrame();
            //yield return new WaitForSeconds(1);

            mainStateMachine.SetBool(scanningId, false);

            StartCoroutine(MainSequence());
        }

        private bool _videoPlayerReady;

        private protected virtual IEnumerator MainSequence()
        {
            yield return new WaitUntil(() => GameplayActive);

            OnActivateWeaponButtons(WeaponType.Light);
            OnActivateWeaponButtons(WeaponType.Medium);
            //OnActivateWeaponButtons(WeaponType.Heavy);

            yield break;
        }

        private bool PlanesFound() => planeManager && planeManager.trackables.count > 0 && _videoPlayerReady;

        internal static event Action<WeaponType> ActivateWeaponButtonsAction;
        private protected void OnActivateWeaponButtons(WeaponType weaponType)
        {
            ActivateWeaponButtonsAction?.Invoke(weaponType);
        }

        private static readonly int gameplayId = Animator.StringToHash("Gameplay");
        public void SetGameplay(bool start)
        {
            mainStateMachine.SetBool(gameplayId, start);
            GameplayActive = start;

            _videoPlayerReady = false;
            videoPlayerSearch.enabled = false;

            //var newPlanePrefab = planeManager.planePrefab;
            //var meshRenderer = newPlanePrefab.GetComponent<MeshRenderer>();
            //var newMaterials = new Material[] { meshRenderer.materials[0] };
            //meshRenderer.materials = newMaterials;
            //planeManager.planePrefab.GetComponent<MeshRenderer>().materials = newMaterials;

            planeManager.planePrefab = gameplayARPlanePrefab;
            var newMaterials = gameplayARPlanePrefab.GetComponent<MeshRenderer>().sharedMaterials;
            //planeManager.SetTrackablesActive(false);

            foreach (var trackable in planeManager.trackables)
            {
                trackable.GetComponent<MeshRenderer>().materials = newMaterials;
            }

            pointCloudManager.pointCloudPrefab = null;
            pointCloudManager.SetTrackablesActive(false);
            
            //if (start) OnWeaponSwitchAction(1);
        }

        [Space]
        [SerializeField] private GameObject lookMarker;
        [SerializeField] private Transform lookMarkerUI;

        private protected bool showLookPoint;

        private readonly List<ARRaycastHit> hitsBuffer = new();

        private void Update()
        {
            if (showLookPoint)
            {
                var screenCenter = new Vector2(Screen.width, Screen.height) / 2;
                
                if (raycastManager.Raycast(screenCenter, hitsBuffer, TrackableType.PlaneWithinPolygon))
                {
                    lookMarker.SetActive(true);
                    lookMarker.transform.position = Vector3.Lerp(lookMarker.transform.position, hitsBuffer[0].pose.position, 12 * Time.deltaTime);

                    if (MainCameraReady())
                    {
                        lookMarkerUI.rotation = Quaternion.Lerp(lookMarkerUI.rotation,
                            Quaternion.LookRotation(_mainCamera.transform.position - lookMarkerUI.position, Vector3.up), 6 * Time.deltaTime);
                    }
                }
            }
            else lookMarker.SetActive(false);
        }
    }

    internal struct EndGameAward
    {
        internal List<WeaponName> NewWeapons;
        internal int Coins;
        internal int Xp;

        internal EndGameAward(List<WeaponName> newWeapons, int coins, int xp)
        {
            NewWeapons = newWeapons;
            Coins = coins;
            Xp = xp;
        }
    }
}
