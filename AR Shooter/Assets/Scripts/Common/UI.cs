using System;
using System.Collections;
using DG.Tweening;
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

        AimInstance = new Aim(aimAnimation, aimTransform, images);
    }

    // public void Aiming(bool toAim)
    // {
    //     WeaponHolderScript.Aiming(toAim);
    // }

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
            // WeaponHolderScript.Aiming(false);

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
    [SerializeField] private Image[] images;

    internal static Aim AimInstance;
    
    public class Aim
    {
        public Aim(Animation aimAnimation, RectTransform aimTransform, Image[] images)
        {
            _aimAnimation = aimAnimation;
            _aimTransform = aimTransform;
            _images = images;
            _currentAimSpreadRadius = _startAimSpreadRadius;
        }

        private readonly RectTransform _aimTransform;
        private readonly Animation _aimAnimation;
        private readonly Image[] _images;

        private int _aimedAimSpreadRadius = 30;
        private int _startAimSpreadRadius = 110;
        private float _currentAimSpreadRadius;
        
        internal float TransformTime = 1.5f;

        private bool _isVisible = true;
    
        internal Ray GetRay()
        {
            var aimPos = _aimTransform.position;
            
            return Camera.main.ScreenPointToRay(_currentAimSpreadRadius * Random.insideUnitCircle / 4f + new Vector2(aimPos.x, aimPos.y));
        }

        internal void AimAnimation()
        {
            if (!_isVisible) return;
            
            _currentAimSpreadRadius += 90;
            _aimAnimation.Play();
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _currentAimSpreadRadius);
        }
    
        internal void Update()
        {
            if (_currentAimSpreadRadius <= _startAimSpreadRadius) return;
            
            var rect = _aimTransform.rect;
            _currentAimSpreadRadius = rect.width - 1.5f * Time.deltaTime * rect.width;
            if (_currentAimSpreadRadius < _startAimSpreadRadius) _currentAimSpreadRadius = _startAimSpreadRadius;
            
            _aimTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _currentAimSpreadRadius);
        }
        
        internal void SetActive(bool value)
        {
            var fadeTo = value ? 1 : 0;
            
            foreach (var image in _images)
            {
                image.DOFade(fadeTo, 0.3f).OnComplete(() => _isVisible = value);
            }

            DOTween.To(() => _currentAimSpreadRadius, x => _currentAimSpreadRadius = x, 
                fadeTo * _startAimSpreadRadius + _aimedAimSpreadRadius, 0.3f);
        }
    }
}
