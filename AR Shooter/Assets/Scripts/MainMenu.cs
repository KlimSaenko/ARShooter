using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static Config;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Transform mainCam;
    [SerializeField] private TextMeshPro recordText;
    [SerializeField] private Toggle[] togglesSettings;
    [SerializeField] private Slider sliderSettings;
    //[SerializeField] private TextMeshProUGUI textDebug;

    private float _translationTime;
    private Vector2 _menuPosTo;
    private Vector3 _startCamPos;

    private bool _singlePlayerLoaded;
    private bool _multiPlayerLoaded;
    private bool _previewLoaded;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _menuPosTo = rectTransform.anchoredPosition;

        _singlePlayerLoaded = false;
        _multiPlayerLoaded = false;

        recordText.text = RecordKills.ToString();
        togglesSettings[0].isOn = GameSettings.IsStaticSpawnZone;
        sliderSettings.value = GameSettings.OcclusionLevel;
    }

    private void Update()
    {
        if (IsTranslating()) Translation();

        CamTranslation();
    }

    public void MovePages(bool newPage)
    {
        _translationTime = 0.23f - _translationTime;
        _menuPosTo += newPage ? new Vector2(-1100, 0) : new Vector2(1100, 0);
    }

    private void Translation()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, _menuPosTo, Time.deltaTime / _translationTime);
        _translationTime -= Time.deltaTime;
        if (_translationTime < Time.deltaTime) rectTransform.anchoredPosition = _menuPosTo;
    }

    private bool IsTranslating()
    {
        return _translationTime > 0;
    }

    public void StartMultiplayer(bool isOwner)
    {
        if (_multiPlayerLoaded) return;
        
        _multiPlayerLoaded = true;

        if (isOwner) Lobby.CreateRoom();
        else Lobby.JoinRoom();
    }

    public void StartSingleplayer()
    {
        if (_singlePlayerLoaded) return;
        
        _singlePlayerLoaded = true;

        SceneManager.LoadSceneAsync(1);
    }

    public void StartPreview()
    {
        if (!_previewLoaded)
        {
            _previewLoaded = true;

            SceneManager.LoadSceneAsync(3);
        }
    }

    public void ExitGame()
    {
        SaveGame();

        Application.Quit();
    }

    #if UNITY_ANDROID && !UNITY_EDITOR

    private void OnApplicationPause(bool pause)
    {
        if (pause) Config.SaveGame();
        else
        {
            Config.LoadGame();

            recordText.text = Config.RecordKills.ToString();
        }
    }
    
    #endif

    #region Background

    private Vector3 _accelerationBefore = Vector3.zero;
    private Vector3 _acceleration = Vector3.zero;

    private void Start()
    {
        _startCamPos = mainCam.position;
        _accelerationBefore = Input.acceleration;
        _acceleration = Input.acceleration;
    }

    private static Vector3 LowPassFilterAccelerometer(Vector3 prevValue)
    {
        var newValue = Vector3.Lerp(prevValue, Input.acceleration, Time.deltaTime);
        return newValue;
    }

    private void CamTranslation()
    {
        _acceleration = LowPassFilterAccelerometer(_acceleration);
        var accelerationDelta = _acceleration - _accelerationBefore;
        mainCam.position = Vector3.Lerp(mainCam.position + 2.5f * accelerationDelta, _startCamPos, 10 * Time.deltaTime);
        _accelerationBefore = _acceleration;
    }

    #endregion

    #region Settings

    public void ChangeSpawnHeight(bool isStatic) =>
        GameSettings.IsStaticSpawnZone = isStatic;

    public void ChangeOcclusionLevel(float level) =>
        GameSettings.OcclusionLevel = (int)level;

    #endregion
}
