using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Weapons;

public class UI : MonoBehaviour
{
    [SerializeField] private RectTransform normalMarker;
    [SerializeField] private RectTransform critMarker;
    [SerializeField] private Image mapLookImage;
    [SerializeField] private Transform mapCircle;
    [SerializeField] private RectTransform pauseMenu;
    [SerializeField] private TextMeshProUGUI killsText;

    internal static GameObject aliveStateUI;
    internal static GameObject deadStateUI;

    internal static Transform MapCircle { get; set; }

    private static RectTransform[] hitMarkers;
    private static Camera MainCam { get { return Camera.main; } }
    private static TextMeshProUGUI KillsText { get; set; }

    public static WeaponHolderAndSwitcher weaponHolderScript;
    private Transform player;

    private void Start()
    {
        MapCircle = mapCircle;

        hitMarkers = new RectTransform[] { normalMarker, critMarker };
        KillsText = killsText;
        Config.MobsKills = 0;

        aliveStateUI = transform.GetChild(0).gameObject;
        deadStateUI = transform.GetChild(1).gameObject;
    }

    public void Aiming(bool toAim)
    {
        weaponHolderScript.Aiming(toAim);
    }

    public void Shoot(bool start)
    {
        weaponHolderScript.Shoot(start);
    }

    public void SwitchWeapon(int toWeaponIndex)
    {
        weaponHolderScript.SwitchWeapon(toWeaponIndex);
    }

    private static float hitTimeThreshold;

    private void Update()
    {
        if (hitTimeThreshold > 0 && hitMarkers.Length > 0)
        {
            hitTimeThreshold -= Time.deltaTime;
            if (hitTimeThreshold <= 0) foreach (RectTransform marker in hitMarkers) marker.gameObject.SetActive(false);
        }

        if (player != null)
        {
            //mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - player.rotation.eulerAngles.y);
            mapCircle.localRotation = Quaternion.Euler(0, 0, player.rotation.eulerAngles.y);
        }
        else if (MainCam != null)
        {
            player = MainCam.transform;
            mapLookImage.fillAmount = Screen.width / Screen.height * MainCam.fieldOfView / 360f;
            mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - player.rotation.eulerAngles.y);
        }

        if (IsTranslating()) Translation();
    }

    public static void ActivateMarker(int type, Vector3 pos)
    {
        hitMarkers[type].position = MainCam.WorldToScreenPoint(pos);
        hitMarkers[type].gameObject.SetActive(true);

        hitTimeThreshold = 0.11f;
    }

    public void Quit()
    {
        isPaused = false;

        SceneManager.LoadScene(0);
    }

    private float translationTime;
    private Vector2 menuPosTo;

    internal static bool isPaused = false;

    public void Pause(bool pause)
    {
        if (pause)
        {
            weaponHolderScript.Shoot(false);
            weaponHolderScript.Aiming(false);

            pauseMenu.gameObject.SetActive(true);
        }
        isPaused = pause;

        translationTime = 0.19f - translationTime;
        menuPosTo = pause ? new Vector2(0, 0) : new Vector2(850, 0);
    }

    public void Restart()
    {
        isPaused = false;

        SceneManager.LoadScene(1);
    }

    private void Translation()
    {
        pauseMenu.anchoredPosition = Vector2.Lerp(pauseMenu.anchoredPosition, menuPosTo, Time.deltaTime / translationTime);
        translationTime -= Time.deltaTime;
        if (translationTime < Time.deltaTime)
        {
            pauseMenu.anchoredPosition = menuPosTo;
            if (!isPaused) pauseMenu.gameObject.SetActive(false);
        }
    }

    private bool IsTranslating()
    {
        return translationTime > 0;
    }

    internal static void KillsUI(int kills)
    {
        KillsText.text = kills.ToString();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) Config.SaveGame();
        else Config.LoadGame();
    }
}
