using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Pool;
using Weapons;
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

    private static LinkedPool<RectTransform> _hitMarkers;
    private static Camera MainCam => Camera.main;
    private static TextMeshProUGUI KillsText { get; set; }

    public static WeaponHolderAndSwitcher WeaponHolderScript;
    private Transform _player;

    private void Start()
    {
        MapCircle = mapCircle;

        _hitMarkers = new LinkedPool<RectTransform>(() => normalMarker,
            rt => rt.gameObject.SetActive(true),
            rt => rt.gameObject.SetActive(false),
            null, false,10);
        
        KillsText = killsText;
        Config.MobsKills = 0;

        AliveStateUI = transform.GetChild(0).gameObject;
        DeadStateUI = transform.GetChild(1).gameObject;

        AimInstance = new Aim(aimAnimation, aimTransform);
    }

    public void Aiming(bool toAim)
    {
        WeaponHolderScript.Aiming(toAim);
    }

    public void Shoot(bool start) =>
        WeaponHolderScript.Shoot(start);

    public void SwitchWeapon(int toWeaponIndex)
    {
        WeaponHolderScript.SwitchWeapon(toWeaponIndex);
    }

    private static float _hitTimeThreshold;

    public void DisableMarker()
    {
        
    }

    private void Update()
    {
        // if (_hitTimeThreshold > 0 && _hitMarkers.CountInactive > 0)
        // {
        //     _hitTimeThreshold -= Time.deltaTime;
        //     if (_hitTimeThreshold <= 0) foreach (var marker in _hitMarkers.) marker.gameObject.SetActive(false);
        // }

        if (_player != null)
        {
            //mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - player.rotation.eulerAngles.y);
            mapCircle.localRotation = Quaternion.Euler(0, 0, _player.rotation.eulerAngles.y);
        }
        else if (MainCam != null)
        {
            _player = MainCam.transform;
            mapLookImage.fillAmount = Screen.width / Screen.height * MainCam.fieldOfView / 360f;
            mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - _player.rotation.eulerAngles.y);
        }

        if (IsTranslating()) Translation();
        
        AimInstance.Update();
    }

    public static void ActivateHitMarker(int type, Vector3 pos)
    {
        _hitMarkers.Get().position = MainCam.WorldToScreenPoint(pos);
        // _hitMarkers[type].position = MainCam.WorldToScreenPoint(pos);

        _hitTimeThreshold = 0.1f;
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
            WeaponHolderScript.Shoot(false);
            WeaponHolderScript.Aiming(false);

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

    internal static Aim AimInstance;
    
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
            var aimWidth = _aimTransform.rect.width;
            _aimAnimation.Play();
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, aimWidth + 90);

            var aimPos = _aimTransform.position;
            
            return Camera.main.ScreenPointToRay(aimWidth * Random.insideUnitCircle / 4f + new Vector2(aimPos.x, aimPos.y));
        }
    
        internal void Update()
        {
            var rect = _aimTransform.rect;
            if (rect.width <= StartAimSpreadRadius) return;
            
            var newWidth = rect.width - 1.5f * Time.deltaTime * rect.width;
            if (newWidth < StartAimSpreadRadius) newWidth = StartAimSpreadRadius;
            
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }
    }
}
