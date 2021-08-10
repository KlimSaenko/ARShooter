using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Pool;
using static Weapons.MainWeapon;
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

        AimInstance = new Aim(aimTransform, images);
    }

    public void DisableMarker()
    {
        
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
            mapLookImage.transform.rotation = Quaternion.Euler(0, 0, mapLookImage.fillAmount * 180 - _player.rotation.eulerAngles.y);
        }

        if (IsTranslating()) Translation();
        
        // AimInstance.Update();
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

            _currentAimSpreadDiameter = CurrentAimSpreadDiameter;
        }

        private readonly RectTransform _aimTransform;
        private readonly Image[] _images;

        private float _currentAimSpreadDiameter;
        private float CurrentAimSpreadDiameter => _isVisible? _aimTransform.rect.width : _currentAimSpreadDiameter;

        private bool _isVisible = true;
    
        internal Ray GetRay()
        {
            var aimPos = _aimTransform.position;
            
            return Camera.main.ScreenPointToRay(CurrentAimSpreadDiameter * Random.insideUnitCircle / 2.2f + new Vector2(aimPos.x, aimPos.y));
        }

        private const int TweenId = 100;

        internal void AimAnimation()
        {
            DOTween.Kill(TweenId);
            
            _aimTransform.DOSizeDelta(_aimTransform.rect.size + new Vector2(ActiveWeaponStats.aimSpreadIncrement, 0), ActiveWeaponStats.aimSpreadIncrement / 900f).SetEase(Ease.OutBack)
                .OnComplete(() => _aimTransform.DOSizeDelta(new Vector2(ActiveWeaponStats.freeAimSpreadDiameter, 100), ActiveWeaponStats.aimRecoveryTime / 2f * Mathf.Pow(_aimTransform.rect.width / 200f, 0.75f))
                    .SetEase(Ease.OutSine).SetId(TweenId));
        }

        internal void SetActive(bool value)
        {
            var fadeTo = value ? 1 : 0;
            
            foreach (var image in _images)
            {
                image.DOFade(fadeTo, 0.25f).SetDelay(fadeTo * 0.1f);
            }

            DOTween.To(() => CurrentAimSpreadDiameter, newValue => _currentAimSpreadDiameter = newValue,
                value ? _aimTransform.rect.width : ActiveWeaponStats.aimedAimSpreadDiameter, 0.25f).OnStart(() => _isVisible = value);
        }
    }
    
#endregion

#region Reload

    private static event Action<bool> ReloadCompleteAction;

    public void ReloadSlider(float value)
    {
        if (value > 0.99f) ReloadCompleteAction?.Invoke(true);
    }
    
    public void SliderUp()
    {
        ReloadCompleteAction?.Invoke(false);
    }
    
    internal class Reload
    {
        private readonly Slider _reloadSlider;
        private readonly Action _reloadActionCallback;
        internal bool IsReloading;
        
        internal Reload(Action reloadActionCallback, Slider reloadSlider)
        {
            _reloadActionCallback += reloadActionCallback;
            _reloadSlider = reloadSlider;
            ReloadCompleteAction += StopReload;
        }

        internal void StartReload()
        {
            IsReloading = true;
            _reloadSlider.gameObject.SetActive(true);
        }
        
        internal void StopReload(bool complete)
        {
            IsReloading = !complete;

            if (complete)
            {
                _reloadSlider.value = 0;
                _reloadActionCallback?.Invoke();
            }
            else _reloadSlider.DOValue(0, _reloadSlider.value * 0.7f).SetEase(Ease.OutQuad);
            
            _reloadSlider.gameObject.SetActive(!complete);
        }
    }

#endregion
}
