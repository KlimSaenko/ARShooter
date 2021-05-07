using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Transform mainCam;
    [SerializeField] private TextMeshPro recordText;
    [SerializeField] private Toggle[] togglesSettings;
    [SerializeField] private Slider sliderSettings;
    //[SerializeField] private TextMeshProUGUI textDebug;

    private float translationTime;
    private Vector2 menuPosTo;
    private Vector3 startCamPos;

    private bool singlePlayerLoaded;
    private bool multiPlayerLoaded;
    private bool previewLoaded;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        menuPosTo = rectTransform.anchoredPosition;

        singlePlayerLoaded = false;
        multiPlayerLoaded = false;

        recordText.text = Config.RecordKills.ToString();
        togglesSettings[0].isOn = Config.isStaticSpawnZone;
        sliderSettings.value = Config.occlusionLevel;
    }

    private void Update()
    {
        if (IsTranslating()) Translation();

        CamTranslation();
    }

    public void MovePages(bool newPage)
    {
        translationTime = 0.23f - translationTime;
        menuPosTo += newPage ? new Vector2(-1100, 0) : new Vector2(1100, 0);
    }

    private void Translation()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, menuPosTo, Time.deltaTime / translationTime);
        translationTime -= Time.deltaTime;
        if (translationTime < Time.deltaTime) rectTransform.anchoredPosition = menuPosTo;
    }

    private bool IsTranslating()
    {
        return translationTime > 0;
    }

    public void StartMultiplayer(bool isOwner)
    {
        if (!multiPlayerLoaded)
        {
            multiPlayerLoaded = true;

            if (isOwner) Lobby.CreateRoom();
            else Lobby.JoinRoom();
        }
    }

    public void StartSingleplayer()
    {
        if (!singlePlayerLoaded)
        {
            singlePlayerLoaded = true;

            SceneManager.LoadSceneAsync(1);
        }
    }

    public void StartPreview()
    {
        if (!previewLoaded)
        {
            previewLoaded = true;

            SceneManager.LoadSceneAsync(3);
        }
    }

    public void ExitGame()
    {
        Config.SaveGame();

        Application.Quit();
    }

    #if UNITY_ANDROID

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

    private Vector3 accelerationBefore = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;

    void Start()
    {
        startCamPos = mainCam.position;
        accelerationBefore = Input.acceleration;
        acceleration = Input.acceleration;
    }

    private Vector3 LowPassFilterAccelerometer(Vector3 prevValue)
    {
        Vector3 newValue = Vector3.Lerp(prevValue, Input.acceleration, Time.deltaTime);
        return newValue;
    }

    private void CamTranslation()
    {
        acceleration = LowPassFilterAccelerometer(acceleration);
        Vector3 accelerationDelta = acceleration - accelerationBefore;
        mainCam.position = Vector3.Lerp(mainCam.position + 2.5f * accelerationDelta, startCamPos, 10 * Time.deltaTime);
        accelerationBefore = acceleration;
    }

    #endregion

    #region Settings

    public void ChangeSpawnHeight(bool isStatic)
    {
        Config.isStaticSpawnZone = isStatic;
    }

    public void ChangeOcclusionLevel(float level)
    {
        Config.occlusionLevel = (int)level;
    }

    #endregion
}
