using Game.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static Game.Config;
    
namespace Game.Mobs
{
    public class MobSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject mobPrefab;
        [SerializeField] private GameObject bossMob;
        [SerializeField] private Transform spawnZone;
        [SerializeField] internal Transform target;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject prediction;
        //[SerializeField] private AROcclusionManager AROcclusion;

        private Transform _poolFolder;
        private int _mobsSpawned = 1;

        [SerializeField] private ARRaycastManager raycastManager;
        internal static MobSpawner Instance;

        internal readonly LinkedList<GameObject> MobPool = new();
        private Vector2 _screenCenter;

        private void Awake()
        {
            Instance = this;

            //AROcclusion.requestedEnvironmentDepthMode = (EnvironmentDepthMode)GameSettings.OcclusionLevel;
        }

        private float _spawnHeight = -0.1f;
        private bool _timer = false;

        private void Start()
        {
            _poolFolder = transform;
            _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            yield return new WaitUntil(FindSurface);

            _timer = true;

            spawnZone.position = new Vector3(target.position.x, _spawnHeight, target.position.z);
            spawnZone.gameObject.SetActive(true);

            StartCoroutine(EnemiesSpawn());

            while (!FullPool())
            {
                var mob = Instantiate(mobPrefab, new Vector3(0, -1, 1), Quaternion.Euler(0, 0, 0), _poolFolder);
                MobPool.AddLast(mob);
                mob.SetActive(false);

                yield return new WaitForEndOfFrame();
            }

            timerText.gameObject.SetActive(true);

            for (var i = 9; i > 0; i--)
            {
                yield return new WaitForSeconds(1);

                timerText.text = i.ToString();

                if (i == 5) prediction.SetActive(false);
            }

            yield return new WaitForSeconds(1);

            _timer = false;

            timerText.gameObject.SetActive(false);
            spawnZone.gameObject.SetActive(false);

            if (GameSettings.IsStaticSpawnZone) yield break;
        
            while (true)
            {
                if (FindSurface()) _spawnHeight = hitResults[0].pose.position.y;

                yield return new WaitForFixedUpdate();
            }
        }

        private readonly List<ARRaycastHit> hitResults = new();

        private bool FindSurface()
        {
            return target != null && raycastManager.Raycast(_screenCenter, hitResults, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds);
        }

        private IEnumerator EnemiesSpawn()
        {
            while (_timer)
            {
                if (FindSurface()) _spawnHeight = hitResults[0].pose.position.y;

                if (target != null) spawnZone.position = new Vector3(target.position.x, _spawnHeight, target.position.z);

                yield return new WaitForEndOfFrame();
            }

            while (true)
            {
                if (!CommonUI.IsPaused)
                {
                    if (MobPool.Count > 0 && target != null)
                    {
                        var angle = Random.Range(-Mathf.PI / 8f, Mathf.PI / 8f) + target.rotation.eulerAngles.y * Mathf.Deg2Rad;
                        GameObject mob;

                        if (_mobsSpawned % 25 == 0)
                        {
                            yield return new WaitUntil(FullPool);

                            mob = bossMob;
                        }
                        else
                        {
                            yield return new WaitWhile(BossActive);

                            mob = MobPool.Last.Value;
                            MobPool.RemoveLast();
                        }

                        mob.transform.position = new Vector3(target.position.x + 4.5f * Mathf.Sin(angle), _spawnHeight, target.position.z + 4.5f * Mathf.Cos(angle));
                        mob.SetActive(true);
                        _mobsSpawned++;
                    }
                }

                yield return new WaitForSeconds(2);
            }
        }

        private bool FullPool()
        {
            return MobPool.Count == 6;
        }

        private bool BossActive()
        {
            return bossMob.activeSelf;
        }
    }
}
