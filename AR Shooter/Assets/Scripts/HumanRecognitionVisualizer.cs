using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BodyPix;
using DG.Tweening;
using Klak.TestTools;
using Mobs;
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
    
    private void Start()
    {
        _detector = new BodyDetector(_resources, _resolution.x, _resolution.y);

        _material = new Material(_shader);

        var reso = new Vector2Int(Screen.width, Screen.height);
        _mask = new RenderTexture(reso.x, reso.y, 0);
        _maskUI.texture = _mask;
        // _material.SetFloat(GetPosOfCenter, 0.4f);

        if (!_sourceAR.enabled) return;
        
        Instance = this;
        Config.CurrentGameplayMode = Config.GameplayMode.Real;
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
    
    private void LateUpdate()
    {
        if (!_sourceAR.enabled || _sourceAR.Texture == null) return;
        
        _detector.ProcessImage(_sourceAR.Texture);

        // if (_sourceImage.Texture == null) return;
        //
        // _detector.ProcessImage(_sourceImage.Texture);
            
        var keypoints = _detector.UpdatePostReadCache();

        _prevDistance = ProcessKeypoints(keypoints, new []{ 5, 6, 11, 12 }, out var validatedPoints);
            
        // tm.color = Color.Lerp(Color.red, Color.green, validatedPoints / 5f);
        // tm.text = $"{distance:N2}m";
            
        Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
    }

    private readonly List<ARRaycastHit> _raycastHits = new();
    
    [SerializeField] private ComputeShader processShader;
    private ComputeBuffer _resultsBuffer;
    private Data[] _data;
    private Vector4[] _points;
    
    private float _prevDistance = 0;
    
    private struct Data
    {
        public int type;
    }
    
    internal HitZone.ZoneType[] ProcessRaycast(Vector2[] raycastPoint, out float distance)
    {
        distance = 0;

        if (!_sourceAR.enabled || _sourceAR.Texture == null) return new []{ HitZone.ZoneType.None };
            
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

        var zoneTypes = new HitZone.ZoneType[hitsCount];
        for (var i = 0; i < hitsCount; i++)
        {
            zoneTypes[i] = (HitZone.ZoneType)_data[i].type;
        }

        return zoneTypes;
    }
    
    private static readonly int InputTexture = Shader.PropertyToID("InputTexture");
    private static readonly int Input = Shader.PropertyToID("Input");
    private static readonly int Result = Shader.PropertyToID("Result");
        
    private float ProcessKeypoints(Keypoint[] keypoints, int[] types, out int validatedPoints)
    {
        var prevDistance = _prevDistance;
        var currentDistance = 0f;
        validatedPoints = 0;

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
        
        if (keypoints[0].Score > 0.85f)
        {
            noseMarkerPos = new Vector2(keypoints[0].Position.x * screen.x, keypoints[0].Position.y * screen.y);

            if (raycastManager.Raycast(noseMarkerPos, _raycastHits))
            {
                prevDistance = prevDistance == 0 ? _raycastHits[0].distance : 
                    Mathf.Lerp(prevDistance, _raycastHits[0].distance, 5 * Time.deltaTime);
                
                currentDistance += prevDistance;
                validatedPoints++;
            }
        }
        
        currentDistance /= validatedPoints;

        if (noseMarkerPos == Vector2.zero)
        {
            if (!speechText.gameObject.activeSelf) return validatedPoints == 0 ? prevDistance : currentDistance;
            
            DOTween.Kill(TweenId);
            speechText.DOScale(0, 0.4f).OnComplete(() => speechText.gameObject.SetActive(false)).SetId(TweenId);

            return validatedPoints == 0 ? prevDistance : currentDistance;
        }

        var rayToNose = Camera.main.ScreenPointToRay(noseMarkerPos);
        var currentMarkerPos = rayToNose.GetPoint(3) + (Vector3.Cross(rayToNose.direction.normalized, Vector3.up) + Vector3.up) * 0.25f;
                                                              
        var prevMarkerPos = speechText.position;
        speechText.position = Vector3.Lerp(prevMarkerPos, currentMarkerPos, 
            12 * Mathf.Pow(Vector2.Distance(prevMarkerPos, currentMarkerPos), 0.5f) * Time.deltaTime);

        speechText.rotation = Quaternion.Lerp(speechText.rotation, Camera.main.transform.rotation, 4 * Time.deltaTime);

        if (speechText.gameObject.activeSelf) return validatedPoints == 0 ? prevDistance : currentDistance;
        
        DOTween.Kill(TweenId);
        speechText.DOScale(0.06f, 0.5f).SetEase(Ease.OutBack).OnStart(() => speechText.gameObject.SetActive(true)).SetId(TweenId);

        return validatedPoints == 0 ? prevDistance : currentDistance;
    }

    private const int TweenId = 102;

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
