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
    [SerializeField] SourceAR _sourceAR = null;
    [SerializeField] ImageSource _sourceImage = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Vector2Int _resolution = new(512, 384);
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] RawImage _maskUI = null;
    [SerializeField] bool _drawSkeleton = false;
    [SerializeField] Shader _shader = null;
    
    [SerializeField] private ARRaycastManager raycastManager;

    BodyDetector _detector;
    Material _material;
    RenderTexture _mask;

    internal static HumanRecognitionVisualizer Instance;
    
    private static readonly int Keypoints = Shader.PropertyToID("_Keypoints");
    private static readonly int Aspect = Shader.PropertyToID("_Aspect");

    private void Awake() =>
        Instance = this;

    private void Start()
    {
        _detector = new BodyDetector(_resources, _resolution.x, _resolution.y);

        _material = new Material(_shader);

        var reso = new Vector2Int(Screen.width, Screen.height);
        _mask = new RenderTexture(reso.x, reso.y, 0);
        _maskUI.texture = _mask;
        // _material.SetFloat(GetPosOfCenter, 0.4f);

        if (!_sourceAR.enabled) return;
        
        // Config.CurrentGameplayMode = Config.GameplayMode.Real;
    }
    
    private void OnDestroy()
    {
        _detector?.Dispose();
        Destroy(_material);
        Destroy(_mask);
    }

// #define BODYPIX_KEYPOINT_NOSE               0 true
// #define BODYPIX_KEYPOINT_LEFT_EYE           1
// #define BODYPIX_KEYPOINT_RIGHT_EYE          2
// #define BODYPIX_KEYPOINT_LEFT_EAR           3
// #define BODYPIX_KEYPOINT_RIGHT_EAR          4
// #define BODYPIX_KEYPOINT_LEFT_SHOULDER      5 true
// #define BODYPIX_KEYPOINT_RIGHT_SHOULDER     6 true
// #define BODYPIX_KEYPOINT_LEFT_ELBOW         7
// #define BODYPIX_KEYPOINT_RIGHT_ELBOW        8
// #define BODYPIX_KEYPOINT_LEFT_WRIST         9
// #define BODYPIX_KEYPOINT_RIGHT_WRIST        10
// #define BODYPIX_KEYPOINT_LEFT_HIP           11 true
// #define BODYPIX_KEYPOINT_RIGHT_HIP          12 true
// #define BODYPIX_KEYPOINT_LEFT_KNEE          13
// #define BODYPIX_KEYPOINT_RIGHT_KNEE         14
// #define BODYPIX_KEYPOINT_LEFT_ANKLE         15
// #define BODYPIX_KEYPOINT_RIGHT_ANKLE        16
// #define BODYPIX_KEYPOINT_COUNT              17

    // [SerializeField] private TextMeshPro tm;
    [SerializeField] private Transform speechText;
    [SerializeField] private Transform enemyHp;
    private readonly Keypoint[] _keypoints = new Keypoint[17];
    
    private void LateUpdate()
    {
        if (!_sourceAR.enabled || _sourceAR.Texture == null) return;
        
        _detector.ProcessImage(_sourceAR.Texture);

        // if (_sourceImage.Texture == null) return;
        //
        // _detector.ProcessImage(_sourceImage.Texture);

        _detector.KeypointBuffer.GetData(_keypoints);

        _realEnemy ??= new RealEnemy(speechText, enemyHp);
        
        _prevDistance = ProcessKeypoints(_keypoints, new []{ 5, 6, 11, 12 }, out var headPos);

        _realEnemy.HeadPosition = headPos;
            
        // tm.color = Color.Lerp(Color.red, Color.green, validatedPoints / 5f);
        // tm.text = $"{distance:N2}m";
            
        Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
    }

    private readonly List<ARRaycastHit> _raycastHits = new();

    [SerializeField] private ParticleSystem[] hitParticles; // 0 - virtual; 1 - real
    [SerializeField] private ComputeShader processShader;
    private ComputeBuffer _resultsBuffer;
    private Data[] _data;
    private Vector4[] _points;
    
    private float _prevDistance = 0;
    
    private struct Data
    {
        public int Type;
    }
    
    /// <summary>
    /// 0 - None;
    /// 1 - Body;
    /// 2 - Head.
    /// </summary>
    /// <param name="raycastPoint">Screen points to raycast from</param>
    /// <param name="distance">Distance to the point</param>
    /// <returns>Array of ints witch describes segments of human body</returns>
    internal int[] ProcessRaycast(Vector2[] raycastPoint, out float distance)
    {
        distance = 0;

        if (!_sourceAR.enabled || _sourceAR.Texture == null) return new []{ 0 };
            
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
                    hitParticles[1].transform.position = _raycastHits[0].pose.position;
                    hitParticles[1].transform.rotation = _raycastHits[0].pose.rotation;
                    hitParticles[1].Play();
                    
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
    
    private static readonly int InputTexture = Shader.PropertyToID("InputTexture");
    private static readonly int Input = Shader.PropertyToID("Input");
    private static readonly int Result = Shader.PropertyToID("Result");
        
    private float ProcessKeypoints(Keypoint[] keypoints, IEnumerable<int> types, out Vector3 headPos)
    {
        var prevDistance = _prevDistance;
        var currentDistance = 0f;
        var validatedPoints = 0;
        headPos = Vector3.zero;

        var screen = new Vector2(Screen.width, Screen.height);
        
        // other
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
        
        // nose
        var noseMarkerPos = Vector2.zero;
        
        if (keypoints[0].Score > 0.87f)
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
        private readonly Transform _hp;
        private readonly TextMeshPro _text;
        private Vector3 _prevHeadPos = Vector3.zero;

        private int _HP = 100;
        internal int HP
        {
            get => _HP;
            set
            {
                _HP = value;
                
                if (_HP <= 0) ChangeEmotion(Emotions.Dead);
                else _hp.localScale = new Vector3(value * 6 / 100f, 0.2f, 1);
            }
        }
        
        internal RealEnemy(Transform speechText, Transform hp)
        {
            _speechText = speechText;
            _text = speechText.GetComponentInChildren<TextMeshPro>();
            _hp = hp;
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
                    _speechText.DOScale(0, 0.4f).OnComplete(() => _speechText.gameObject.SetActive(false)).SetId(TweenId);
                }
                else
                {
                    if (_speechText.gameObject.activeSelf)
                    {
                        var distance = Vector2.Distance(_prevHeadPos, value);
                        
                        if (distance < 17 * Time.deltaTime)
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
                        _speechText.DOScale(0.06f, 0.5f).SetEase(Ease.OutBack).OnStart(() => _speechText.gameObject.SetActive(true)).SetId(TweenId);
                    }
                }
            }
        }
    }

    private void OnRenderObject()
    {
        if (!_drawSkeleton) return;

        _material.SetBuffer(Keypoints, _detector.KeypointBuffer);
        _material.SetFloat(Aspect, (float)_resolution.x / _resolution.y);

        _material.SetPass(1);
        Graphics.DrawProceduralNow
          (MeshTopology.Triangles, 6, Body.KeypointCount);

        _material.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 12);
    }
}
