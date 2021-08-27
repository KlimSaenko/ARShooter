using System;
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

    private Transform _player;

    private void Awake()
    {
        AimInstance = new Aim(aimTransform, images);
    }

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
    }

    private void Update()
    {
        if (_player != null)
        {
            //mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - player.rotation.eulerAngles.y);
            mapCircle.localRotation = Quaternion.Euler(0, 0, _player.rotation.eulerAngles.y);
        }
        else if (MainCam != null)
        {
            _player = MainCam.transform;
            mapLookImage.fillAmount = Screen.width / Screen.height * MainCam.fieldOfView / 360f;
            mapLookImage.rectTransform.localRotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180);
        }
    
        // if (IsTranslating()) Translation();
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

    private bool IsTranslating() => _translationTime > 0;

    internal static void KillsUI(int kills)
    {
        KillsText.text = kills.ToString();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) Config.SaveGame();
        else Config.LoadGame();
    }

#region Aim

    [Header("Aim")]
    [SerializeField] private RectTransform aimTransform;
    [SerializeField] private Image[] images;

    internal static Aim AimInstance;

    public class Aim
    {
        public Aim(RectTransform aimTransform, Image[] images)
        {
            _aimTransform = aimTransform;
            _images = images;

            PlayerBehaviour.AimingAction += SetActive;
            _currentAimSpreadDiameter = CurrentAimSpreadDiameter;
        }

        private readonly RectTransform _aimTransform;
        private readonly Image[] _images;

        private float _currentAimSpreadDiameter;
        internal float CurrentAimSpreadDiameter => _isVisible ? _aimTransform.rect.width : _currentAimSpreadDiameter;

        private bool _isVisible = true;
        private WeaponStats _weaponStats;
    
        internal Ray GetRay() =>
            Camera.main.ScreenPointToRay(RawRaycastPoint);

        internal Vector2 RawRaycastPoint
        {
            get
            {
                var aimPos = _aimTransform.position;
                return CurrentAimSpreadDiameter * Random.insideUnitCircle / 2f + new Vector2(aimPos.x, aimPos.y);
            }
        }

        private const int TweenId = 100;

        internal void AimAnimation()
        {
            DOTween.Kill(TweenId);
            
            _aimTransform.DOSizeDelta(_aimTransform.rect.size + new Vector2(_weaponStats.aimSpreadIncrement, 0), _weaponStats.aimSpreadIncrement / 900f).SetEase(Ease.OutBack)
                .OnComplete(() => _aimTransform.DOSizeDelta(new Vector2(_weaponStats.freeAimSpreadDiameter, 100), _weaponStats.aimRecoveryTime / 2f * Mathf.Pow(_aimTransform.rect.width / 200f, 0.75f))
                    .SetEase(Ease.OutSine).SetId(TweenId));
        }

        internal void SetActive(WeaponStats weaponStats, bool value)
        {
            _weaponStats = weaponStats;
            
            var fadeTo = value ? 1 : 0;
            
            foreach (var image in _images)
            {
                image.DOFade(fadeTo, 0.25f);
            }

            if (value)
            {
                DOTween.Kill(TweenId);
                _aimTransform.sizeDelta = new Vector2(_weaponStats.freeAimSpreadDiameter, 100);
            }
            _isVisible = value;
            // DOTween.To(() => CurrentAimSpreadDiameter, newValue => _currentAimSpreadDiameter = newValue,
            //     value ? _aimTransform.rect.width : _weaponStats.aimedAimSpreadDiameter, 0.25f).OnStart(() => _isVisible = value);
        }

        private void SetActive(bool toAim)
        {
            var fadeTo = toAim ? 0 : 1;
            
            foreach (var image in _images)
            {
                image.DOFade(fadeTo, 0.25f);
            }

            DOTween.To(() => CurrentAimSpreadDiameter, newValue => _currentAimSpreadDiameter = newValue,
                !toAim ? _aimTransform.rect.width : _weaponStats.aimedAimSpreadDiameter, 0.25f).OnStart(() => _isVisible = !toAim);
        }
    }
    
#endregion
}
