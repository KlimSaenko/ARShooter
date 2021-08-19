using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BodyPix;
using TMPro;
using UnityEngine.XR.ARFoundation;

public sealed class Visualizer : MonoBehaviour
{
    [SerializeField] SourceAR _sourceAR = null;
    // [SerializeField] ImageSource _sourceImage = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Vector2Int _resolution = new(512, 384);
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] RawImage _maskUI = null;
    [SerializeField] bool _drawSkeleton = false;
    [SerializeField] Shader _shader = null;
    [SerializeField] private RectTransform marker;
    [SerializeField] private ARRaycastManager raycastManager;

    BodyDetector _detector;
    Material _material;
    RenderTexture _mask;
    
    private static readonly int Keypoints = Shader.PropertyToID("_Keypoints");
    private static readonly int Aspect = Shader.PropertyToID("_Aspect");
    
    void Start()
    {
        _detector = new BodyDetector(_resources, _resolution.x, _resolution.y);

        _material = new Material(_shader);

        var reso = new Vector2Int(Screen.width, Screen.height);
        _mask = new RenderTexture(reso.x, reso.y, 0);
        _maskUI.texture = _mask;
        // _material.SetFloat(GetPosOfCenter, 0.4f);
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

    private float _prevDistance = 0;
    
    private readonly List<ARRaycastHit> _raycastHits = new();
    
    [SerializeField] private ComputeShader processShader;
    private ComputeBuffer _resultsBuffer;
    private Data[] _data;
    private Vector4[] _points;
    
    private struct Data
    {
        public float type;
    }
    
    private void LateUpdate()
    {
        if (_sourceAR.enabled)
        {
            if (_sourceAR.Texture == null) return;

            _detector.ProcessImage(_sourceAR.Texture);
            
            var keypoints = _detector.UpdatePostReadCache();

            var distance = ProcessKeypoints(keypoints, new []{ 0, 5, 6, 11, 12 }, out var validatedPoints);
            
            tm.color = Color.Lerp(Color.red, Color.green, validatedPoints / 5f);
            tm.text = $"{distance:N2}m";
            
            Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
        }
        // else if (_sourceImage.enabled)
        // {
        //     if (_sourceImage.Texture == null) return;
        //
        //     _detector.ProcessImage(_sourceImage.Texture);
        //
        //     var keypoints = _detector.UpdatePostReadCache();
        //     
        //     var distance = ProcessKeypoints(keypoints, new []{ 0, 5, 6, 11, 12 }, out var validatedPoints);
        //     
        //     tm.color = Color.Lerp(Color.red, Color.green, validatedPoints / 5f);
        //     tm.text = $"{distance:N2}m";
        //     
        //     Graphics.Blit(_detector.MaskTexture, _mask, _material, 0);
        //     
        //     //
        //
        //     const int hitsCount = 1; // <= 16
        //     
        //     _resultsBuffer = new ComputeBuffer(hitsCount, sizeof(float));
        //     _data = new Data[hitsCount];
        //     _points = new[] { new Vector4(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, 1) }; // z - radius
        //
        //     var processShader = shader;
        //     
        //     processShader.SetTexture(0, InputTexture, _mask);
        //     processShader.SetVectorArray(Input, _points);
        //     processShader.SetBuffer(0, Result, _resultsBuffer);
        //     processShader.Dispatch(0, _mask.width / 8 , _mask.height / 8, hitsCount);
        //     
        //     _resultsBuffer.GetData(_data);
        //     
        //     _resultsBuffer.Release();
        //
        //     Debug.Log($"{_data[0].type}");
        //     // Debug.Log($"{UnityEngine.Input.mousePosition.x}  {UnityEngine.Input.mousePosition.y}");
        //
        //     // None  0.5019608  0.5019608  0.5019608  0
        //     // Crit  1  1  1  1
        //     // Standard 0.5019608  0.5019608  0.5019608  1
        //
        //     //
        // }
    }
    
    private static readonly int InputTexture = Shader.PropertyToID("InputTexture");
    private static readonly int Input = Shader.PropertyToID("Input");
    private static readonly int Result = Shader.PropertyToID("Result");
        
    private float ProcessKeypoints(Keypoint[] keypoints, int[] types, out int validatedPoints)
    {
        var prevDistance = _prevDistance;
        var currentDistance = 0f;
        var currentMarkerPos = Vector2.zero;
        validatedPoints = 0;

        foreach (var type in types)
        {
            if (keypoints[type].Score < 0.9f) continue;

            var thisMarkerPos = new Vector2(keypoints[type].Position.x * Screen.width, keypoints[type].Position.y * Screen.height);

            if (raycastManager.Raycast(thisMarkerPos, _raycastHits))
            {
                prevDistance = prevDistance == 0 ? _raycastHits[0].distance : 
                    Mathf.Lerp(prevDistance, _raycastHits[0].distance, 5 * Time.deltaTime);
            }
            // else continue;

            if (currentMarkerPos == Vector2.zero) currentMarkerPos = thisMarkerPos;
            currentDistance += prevDistance;
            validatedPoints++;
        }
        
        _prevDistance = prevDistance;
        
        if (validatedPoints == 0) return prevDistance;
        
        // currentMarkerPos /= validatedPoints;
        // currentMarkerPos = new Vector2(currentMarkerPos.x * Screen.width, currentMarkerPos.y * Screen.height);
        
        var prevMarkerPos = marker.anchoredPosition;
        // Debug.Log(markerOffset - Mathf.Pow(markerOffset / 100f, 3));
        marker.anchoredPosition = Vector2.Lerp(prevMarkerPos, currentMarkerPos, 
            0.09f * Vector2.Distance(prevMarkerPos, currentMarkerPos) * Time.deltaTime);
        
        currentDistance /= validatedPoints;
        return currentDistance;
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
    
    [SerializeField] private TextMeshProUGUI tm;
}
