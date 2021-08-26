using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BodyPix;
using DG.Tweening;
using Klak.TestTools;
using TMPro;
using UnityEngine.XR.ARFoundation;

public sealed class HumanRecognitionVisualizer : MonoBehaviour
{
    [Header("Barracuda preferences")]
    [SerializeField] private SourceAR sourceAR;
    [SerializeField] private ImageSource sourceImage;
    [SerializeField] private ResourceSet resources;
    [SerializeField] private Vector2Int resolution = new(512, 384);
    
    [Space]
    [SerializeField] private RawImage maskUI;
    [SerializeField] private Shader maskShader;

    private BodyDetector _detector;
    private Material _material;
    private RenderTexture _mask;

    internal static HumanRecognitionVisualizer Instance;

    private void Awake() =>
        Instance = this;

    private void Start()
    {
        _detector = new BodyDetector(resources, resolution.x, resolution.y);

        _material = new Material(maskShader);

        var reso = new Vector2Int(Screen.width, Screen.height);
        _mask = new RenderTexture(reso.x, reso.y, 0);
        maskUI.texture = _mask;
    }
    
    private void OnDestroy()
    {
        _detector?.Dispose();
        Destroy(_material);
        Destroy(_mask);
    }

    // BODYPIX_KEYPOINT_NOSE               0 true
    // BODYPIX_KEYPOINT_LEFT_EYE           1
    // BODYPIX_KEYPOINT_RIGHT_EYE          2
    // BODYPIX_KEYPOINT_LEFT_EAR           3
    // BODYPIX_KEYPOINT_RIGHT_EAR          4
    // BODYPIX_KEYPOINT_LEFT_SHOULDER      5 true
    // BODYPIX_KEYPOINT_RIGHT_SHOULDER     6 true
    // BODYPIX_KEYPOINT_LEFT_ELBOW         7
    // BODYPIX_KEYPOINT_RIGHT_ELBOW        8
    // BODYPIX_KEYPOINT_LEFT_WRIST         9
    // BODYPIX_KEYPOINT_RIGHT_WRIST        10
    // BODYPIX_KEYPOINT_LEFT_HIP           11 true
    // BODYPIX_KEYPOINT_RIGHT_HIP          12 true
    // BODYPIX_KEYPOINT_LEFT_KNEE          13
    // BODYPIX_KEYPOINT_RIGHT_KNEE         14
    // BODYPIX_KEYPOINT_LEFT_ANKLE         15
    // BODYPIX_KEYPOINT_RIGHT_ANKLE        16
    // BODYPIX_KEYPOINT_COUNT              17

    [Header("Real enemy preferences")]
    [SerializeField] private Transform speechText;
    [SerializeField] private Transform enemyHp;
    [SerializeField] private Transform enemyShield;
    [SerializeField] private RectTransform enemyMarker;
    
    private readonly Keypoint[] _keypoints = new Keypoint[17];
    
    private void LateUpdate()
    {
#if UNITY_IOS && !UNITY_EDITOR

        if (!sourceAR.enabled || sourceAR.Texture == null) return;
                
        _detector.ProcessImage(sourceAR.Texture);
        
#elif UNITY_EDITOR

        if (sourceImage.Texture == null) return;
        
        _detector.ProcessImage(sourceImage.Texture);

#endif

        _detector.KeypointBuffer.GetData(_keypoints);

        _realEnemy ??= new RealEnemy(speechText, enemyHp, enemyShield, enemyMarker);
        
        _prevDistance = ProcessKeypoints(_keypoints, new []{ 5, 6, 11, 12 }, out var headPos);

        _realEnemy.HeadPosition = headPos;
        // _realEnemy.MapMarker();
            
        Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
    }

    private readonly List<ARRaycastHit> _raycastHits = new();

    [Tooltip("[0] - virtual hit \n[1] - real shield hit \n[2] - real human hit")]
    [SerializeField] private ParticleSystem[] hitParticles;
    [SerializeField] private ComputeShader processShader;
    
    private ComputeBuffer _resultsBuffer;
    private Data[] _data;
    private Vector4[] _points;
    
    private float _prevDistance = 0;
    
    private struct Data
    {
        public int Type;
    }
    
    [Space]
    [SerializeField] private ARRaycastManager raycastManager;
    
    private static readonly int InputTexture = Shader.PropertyToID("InputTexture");
    private static readonly int Input = Shader.PropertyToID("Input");
    private static readonly int Result = Shader.PropertyToID("Result");
        
    /// <summary>
    /// [0] - None;
    /// [1] - Body;
    /// [2] - Head.
    /// </summary>
    /// <param name="raycastPoint">Screen points to raycast from</param>
    /// <param name="distance">Distance to the point</param>
    /// <returns>Array of ints witch describes segments of human body</returns>
    internal int[] ProcessRaycast(Vector2[] raycastPoint, out float distance)
    {
        distance = 0;

        if (!sourceAR.enabled || sourceAR.Texture == null) return new []{ 0 };
            
        var hitsCount = raycastPoint.Length; // <= 16
            
        _resultsBuffer = new ComputeBuffer(hitsCount, sizeof(int));
        _data = new Data[hitsCount];
        
        _points = new Vector4[hitsCount];
        for (var i = 0; i < hitsCount; i++)
        {
            _points[i] = new Vector4((int)raycastPoint[i].x, (int)raycastPoint[i].y, 1);
        }
        
        processShader.SetTexture(0, InputTexture, _mask);
        processShader.SetVectorArray(Input, _points);
        processShader.SetBuffer(0, Result, _resultsBuffer);
        processShader.Dispatch(0, _mask.width / 8 , _mask.height / 8, hitsCount);
            
        _resultsBuffer.GetData(_data);
            
        _resultsBuffer.Release();
        
        distance = _prevDistance;

        var maxZone = 0;
        var zoneTypes = new int[hitsCount];
        for (var i = 0; i < hitsCount; i++)
        {
            var player = Camera.main;
            var findVirtual = Physics.Raycast(player.ScreenPointToRay(raycastPoint[i]), out var hitInfo);
            
            if (_data[i].Type != 0)
            {
                var virtualDistance = findVirtual ? Vector3.Distance(player.transform.position, hitInfo.point) : 1000;
                var realDistance = raycastManager.Raycast(raycastPoint[i], _raycastHits) ? _raycastHits[0].distance : 1111;
    
                if (virtualDistance >= realDistance)
                {
                    var type = _realEnemy.WithShield ? 1 : 2;
                    hitParticles[type].transform.position = _raycastHits[0].pose.position;
                    // hitParticles[type].transform.rotation = _raycastHits[0].pose.rotation;
                    hitParticles[type].transform.LookAt(Camera.main.transform);
                    hitParticles[type].Play();
                    
                    zoneTypes[i] = _data[i].Type;
                }
                else if (findVirtual)
                {
                    hitParticles[0].transform.position = hitInfo.point;
                    hitParticles[0].transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                    hitParticles[0].Play();
                    
                    zoneTypes[i] = 0;
                }
                
                if (maxZone < zoneTypes[i]) maxZone = zoneTypes[i];
            }
            else if (findVirtual)
            {
                hitParticles[0].transform.position = hitInfo.point;
                hitParticles[0].transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                hitParticles[0].Play();
                
                zoneTypes[i] = 0;
            }
        }
        
        _realEnemy.HP -= maxZone * 3;
        _realEnemy.ChangeEmotion((RealEnemy.Emotions)maxZone);
        
        return zoneTypes;
    }
        
    private float ProcessKeypoints(Keypoint[] keypoints, IEnumerable<int> types, out Vector3 headPos)
    {
        var prevDistance = _prevDistance;
        var currentDistance = 0f;
        var validatedPoints = 0;
        headPos = Vector3.zero;

        var screen = new Vector2(Screen.width, Screen.height);
        
        // other points
        foreach (var type in types)
        {
            if (keypoints[type].Score < 0.9f) continue;

            var thisMarkerPos = new Vector2(keypoints[type].Position.x * screen.x, keypoints[type].Position.y * screen.y);

            if (raycastManager.Raycast(thisMarkerPos, _raycastHits))
            {
                prevDistance = prevDistance == 0 ? _raycastHits[0].distance : 
                    Mathf.Lerp(prevDistance, _raycastHits[0].distance, 5 * Time.deltaTime);
            }
            else continue;

            currentDistance += prevDistance;
            validatedPoints++;
        }
        
        // nose point
        var noseMarkerPos = Vector2.zero;
        
        if (keypoints[0].Score > 0.88f)
        {
            noseMarkerPos = new Vector2(keypoints[0].Position.x * screen.x, keypoints[0].Position.y * screen.y);

            if (raycastManager.Raycast(noseMarkerPos, _raycastHits))
            {
                prevDistance = prevDistance == 0 ? _raycastHits[0].distance : 
                    Mathf.Lerp(prevDistance, _raycastHits[0].distance, 15 * Time.deltaTime);
                
                currentDistance += prevDistance;
                validatedPoints++;
            }
        }

        if (validatedPoints <= 0) return prevDistance;
        currentDistance /= validatedPoints;

        if (noseMarkerPos == Vector2.zero) return currentDistance;
        
        var rayToNose = Camera.main.ScreenPointToRay(noseMarkerPos);
        headPos = rayToNose.GetPoint(currentDistance);
        
        return currentDistance;
    }

    private const int TweenId = 102;

    private RealEnemy _realEnemy;
    /// <summary>
    /// Real enemy functionality class
    /// </summary>
    private class RealEnemy
    {
        private static readonly string[] EmotionText = {
            "I gonna kick your ass",
            ";(",
            ";(((",
            "X/"
        };

        internal enum Emotions
        {
            CanAttack = 0,
            GotHit,
            GotCrit,
            Dead
        }

        private readonly Transform _speechText;
        private readonly TextMeshPro _text;
        
        private readonly Transform _hpTransform;
        private readonly Transform _shieldTransform;

        private readonly RectTransform _mapMarker;
        private readonly RectTransform _map;
        
        private Vector3 _prevHeadPos = Vector3.zero;

        private int _shield = 100;
        private int _hp = 100;
        internal int HP
        {
            get => WithShield ? _shield : _hp;
            set
            {
                if (value > _hp)
                {
                    _shieldTransform.localScale = new Vector3(value * 6 / 100f, 0.2f, 1);
                    _shield = value;
                    
                    _hpTransform.localScale = new Vector3(value * 6 / 100f, 0.2f, 1);
                    _hp = value;

                    return;
                }
                
                if (_hp <= 0) return;
                
                var newValue = value > 0 ? value : 0;
                
                if (WithShield)
                {
                    _shieldTransform.localScale = new Vector3(newValue * 6 / 100f, 0.2f, 1);
                    
                    _shield = newValue;
                }
                else
                {
                    _hpTransform.localScale = new Vector3(newValue * 6 / 100f, 0.2f, 1);
                            
                    if (newValue <= 0) ChangeEmotion(Emotions.Dead);
                    
                    _hp = newValue;
                }
            }
        }

        internal bool WithShield => _shield > 0;
        
        internal RealEnemy(Transform speechText, Transform hpTransform, Transform shieldTransform, RectTransform mapMarker)
        {
            _speechText = speechText;
            _text = speechText.GetComponentInChildren<TextMeshPro>();
            _hpTransform = hpTransform;
            _shieldTransform = shieldTransform;
            _mapMarker = mapMarker;
            _map = mapMarker.parent.TryGetComponent(out RectTransform rectTransform) ? rectTransform : null;
        }

        internal void ChangeEmotion(Emotions type)
        {
            if (HP > 0) _text.text = EmotionText[(int)type];
        }

        internal Vector3 HeadPosition
        {
            set
            {
                if (value == Vector3.zero)
                {
                    if (!_speechText.gameObject.activeSelf) return;
                    
                    DOTween.Kill(TweenId);
                    _speechText.DOScale(0, 0.4f).OnComplete(() =>
                    {
                        _speechText.gameObject.SetActive(false);
                        _mapMarker.gameObject.SetActive(false);
                    }).SetId(TweenId);
                }
                else
                {
                    if (_speechText.gameObject.activeSelf)
                    {
                        var distance = Vector2.Distance(_prevHeadPos, value);
                        
                        if (distance < 16 * Time.deltaTime)
                        {
                            _speechText.position = _prevHeadPos == Vector3.zero? value : Vector3.Lerp(_prevHeadPos, value, 
                                14 * Mathf.Pow(distance, 0.5f) * Time.deltaTime);
                            
                            var targetRot = Camera.main.transform.rotation;
                            _speechText.rotation = Quaternion.Lerp(_speechText.rotation, new Quaternion(targetRot.x, targetRot.y, 0, targetRot.w), 3 * Time.deltaTime);
                            
                            _prevHeadPos = _speechText.position;
                        }
                        else
                        {
                            _speechText.position = _prevHeadPos == Vector3.zero? value : Vector3.Lerp(_prevHeadPos, value, 
                                40 * Mathf.Pow(distance, 0.5f) * Time.deltaTime);
                            
                            var targetRot = Camera.main.transform.rotation;
                            _speechText.rotation = Quaternion.Lerp(_speechText.rotation, new Quaternion(targetRot.x, targetRot.y, 0, targetRot.w), 4 * Time.deltaTime);
    
                            _prevHeadPos = _speechText.position;

                            HP = 100;
                            ChangeEmotion(Emotions.CanAttack);
                        }
                    }
                    else
                    {
                        _speechText.position = value;
                        _prevHeadPos = value;
                        
                        DOTween.Kill(TweenId);
                        _speechText.DOScale(0.06f, 0.5f).SetEase(Ease.OutBack).OnStart(() =>
                        {
                            _speechText.gameObject.SetActive(true);
                            _mapMarker.gameObject.SetActive(true);
                        }).SetId(TweenId);
                    }

                    MapMarker(_prevHeadPos);
                }
            }
        }

        internal void MapMarker(Vector3 pos)
        {
            var player = Camera.main.transform;
            var distance = Vector3.Distance(pos, player.position);
            
            var rotation = Quaternion.LookRotation(pos - player.position).eulerAngles + player.rotation.eulerAngles;

            _map.localRotation = Quaternion.Euler(0, 0, rotation.y);

            _mapMarker.localPosition = new Vector2(0, distance * 48);
        }
    }
    
    private bool _drawSkeleton;
    public void VisualizeSkeleton(bool value) => _drawSkeleton = value;
    
    private static readonly int Keypoints = Shader.PropertyToID("_Keypoints");
    private static readonly int Aspect = Shader.PropertyToID("_Aspect");
    
    private void OnRenderObject()
    {
        if (!_drawSkeleton) return;

        _material.SetBuffer(Keypoints, _detector.KeypointBuffer);
        _material.SetFloat(Aspect, (float)resolution.x / resolution.y);

        _material.SetPass(1);
        Graphics.DrawProceduralNow
          (MeshTopology.Triangles, 6, Body.KeypointCount);

        _material.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 12);
    }
}
