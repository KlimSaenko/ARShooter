using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
    
public class MobSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private GameObject bossMob;
    [SerializeField] private Transform spawnZone;
    [SerializeField] internal Transform target;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject prediction;
    [SerializeField] private AROcclusionManager AROcclusion;

    private Transform poolFolder;
    private int mobsSpawned = 1;

    [SerializeField] private ARRaycastManager raycastManager;
    internal static MobSpawner instance;

    internal LinkedList<GameObject> mobPool = new LinkedList<GameObject>();
    private Vector2 screenCenter;

    private void Awake()
    {
        instance = this;

        AROcclusion.requestedEnvironmentDepthMode = (EnvironmentDepthMode)Config.occlusionLevel;
    }

    private float spawnHeight = -0.1f;
    private bool timer = false;

    private void Start()
    {
        poolFolder = transform;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        yield return new WaitUntil(FindSurface);

        timer = true;

        spawnZone.position = new Vector3(target.position.x, spawnHeight, target.position.z);
        spawnZone.gameObject.SetActive(true);

        StartCoroutine(EnemiesSpawn());

        while (!FullPool())
        {
            GameObject mob = Instantiate(mobPrefab, new Vector3(0, -1, 1), Quaternion.Euler(0, 0, 0), poolFolder);
            mobPool.AddLast(mob);
            mob.SetActive(false);

            yield return new WaitForEndOfFrame();
        }

        timerText.gameObject.SetActive(true);

        for (int i = 9; i > 0; i--)
        {
            yield return new WaitForSeconds(1);

            timerText.text = i.ToString();

            if (i == 5) prediction.SetActive(false);
        }

        yield return new WaitForSeconds(1);

        timer = false;

        timerText.gameObject.SetActive(false);
        spawnZone.gameObject.SetActive(false);

        if (!Config.isStaticSpawnZone)
        {
            while (true)
            {
                if (FindSurface()) spawnHeight = hitResults[0].pose.position.y;

                yield return new WaitForFixedUpdate();
            }
        }
    }

    private List<ARRaycastHit> hitResults = new List<ARRaycastHit>();

    private bool FindSurface()
    {
        return target != null && raycastManager.Raycast(screenCenter, hitResults, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds);
    }

    private IEnumerator EnemiesSpawn()
    {
        while (timer)
        {
            if (FindSurface()) spawnHeight = hitResults[0].pose.position.y;

            if (target != null) spawnZone.position = new Vector3(target.position.x, spawnHeight, target.position.z);

            yield return new WaitForEndOfFrame();
        }

        while (true)
        {
            if (!UI.isPaused)
            {
                if (mobPool.Count > 0 && target != null)
                {
                    float angle = Random.Range(-Mathf.PI / 8f, Mathf.PI / 8f) + target.rotation.eulerAngles.y * Mathf.Deg2Rad;
                    GameObject mob;

                    if (mobsSpawned % 25 == 0)
                    {
                        yield return new WaitUntil(FullPool);

                        mob = bossMob;
                    }
                    else
                    {
                        yield return new WaitWhile(BossActive);

                        mob = mobPool.Last.Value;
                        mobPool.RemoveLast();
                    }

                    mob.transform.position = new Vector3(target.position.x + 4.5f * Mathf.Sin(angle), spawnHeight, target.position.z + 4.5f * Mathf.Cos(angle));
                    mob.SetActive(true);
                    mobsSpawned++;
                }
            }

            yield return new WaitForSeconds(2);
        }
    }

    private bool FullPool()
    {
        return mobPool.Count == 6;
    }

    private bool BossActive()
    {
        return bossMob.activeSelf;
    }
}
