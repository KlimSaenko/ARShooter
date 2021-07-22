using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Weapons;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

public class UI : MonoBehaviour
{
    [SerializeField] private RectTransform normalMarker;
    [SerializeField] private RectTransform critMarker;
    [SerializeField] private Image mapLookImage;
    [SerializeField] private Transform mapCircle;
    [SerializeField] private RectTransform pauseMenu;
    [SerializeField] private TextMeshProUGUI killsText;

    internal static GameObject AliveStateUI;
    internal static GameObject DeadStateUI;

    internal static Transform MapCircle { get; set; }

    private static RectTransform[] hitMarkers;
    private static Camera MainCam => Camera.main;
    private static TextMeshProUGUI KillsText { get; set; }

    public static WeaponHolderAndSwitcher weaponHolderScript;
    private Transform player;

    private void Start()
    {
        MapCircle = mapCircle;

        hitMarkers = new[] { normalMarker, critMarker };
        KillsText = killsText;
        Config.MobsKills = 0;

        AliveStateUI = transform.GetChild(0).gameObject;
        DeadStateUI = transform.GetChild(1).gameObject;

        Aim_ = new Aim(aimAnimation, aimTransform);
    }

    public void Aiming(bool toAim)
    {
        weaponHolderScript.Aiming(toAim);
    }

    public void Shoot(bool start) =>
        weaponHolderScript.Shoot(start);

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
            if (hitTimeThreshold <= 0) foreach (var marker in hitMarkers) marker.gameObject.SetActive(false);
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
        
        Aim_.Update();
    }

    public static void ActivateMarker(int type, Vector3 pos)
    {
        hitMarkers[type].position = MainCam.WorldToScreenPoint(pos);
        hitMarkers[type].gameObject.SetActive(true);

        hitTimeThreshold = 0.11f;
    }

    public void Quit()
    {
        IsPaused = false;

        SceneManager.LoadScene(0);
    }

    private float _translationTime;
    private Vector2 _menuPosTo;

    internal static bool IsPaused = false;

    public void Pause(bool pause)
    {
        if (pause)
        {
            weaponHolderScript.Shoot(false);
            weaponHolderScript.Aiming(false);

            pauseMenu.gameObject.SetActive(true);
        }
        IsPaused = pause;

        _translationTime = 0.19f - _translationTime;
        _menuPosTo = pause ? new Vector2(0, 0) : new Vector2(850, 0);
    }

    public void Restart()
    {
        IsPaused = false;

        SceneManager.LoadScene(1);
    }

    private void Translation()
    {
        pauseMenu.anchoredPosition = Vector2.Lerp(pauseMenu.anchoredPosition, _menuPosTo, Time.deltaTime / _translationTime);
        _translationTime -= Time.deltaTime;
        
        if (!(_translationTime < Time.deltaTime)) return;
        
        pauseMenu.anchoredPosition = _menuPosTo;
        if (!IsPaused) pauseMenu.gameObject.SetActive(false);
    }

    private bool IsTranslating()
    {
        return _translationTime > 0;
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
    
    [Header("Aim")]
    [SerializeField] private Animation aimAnimation;
    [SerializeField] private RectTransform aimTransform;

    internal static Aim Aim_;
    
    public class Aim
    {
        public Aim(Animation aimAnimation, RectTransform aimTransform)
        {
            _aimAnimation = aimAnimation;
            _aimTransform = aimTransform;
        }

        private readonly RectTransform _aimTransform;
        private readonly Animation _aimAnimation;

        private int StartAimSpreadRadius = 110;
        internal float TransformTime = 1.5f;
    
        internal Ray StartAnim()
        {
            _aimAnimation.Play();
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _aimTransform.rect.width + 90);

            return Camera.main.ScreenPointToRay(_aimTransform.rect.width * Random.insideUnitCircle);
        }
    
        internal void Update()
        {
            if (_aimTransform.rect.width <= StartAimSpreadRadius) return;
            
            var rect = _aimTransform.rect;
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width - 1.5f * Time.deltaTime * rect.width);
        }
    }
}
