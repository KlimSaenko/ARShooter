using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class SourceAR : MonoBehaviour
{
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private Shader shader;

    internal Texture Texture => _cameraTexture;
    
    private void Awake()
    {
        _textureMat = new Material(shader);
    }

    private void OnEnable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived += OnCameraFrameReceived;
    }
    
    private void OnDestroy()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnCameraFrameReceived;
    }
    
    private RenderTexture _cameraTexture;
    private Material _textureMat;
    
    private Texture2D _camTexture;
    
    private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
            return;
            
        // print(cpuImage.dimensions); (1920, 1440)
            
        const TextureFormat format = TextureFormat.RGBA32;
        
        if (_camTexture == null || _camTexture.width != cpuImage.width || _camTexture.height != cpuImage.height)
        {
            _camTexture = new Texture2D(cpuImage.width, cpuImage.height, format, false);
            _cameraTexture = new RenderTexture(cpuImage.width, cpuImage.height, 0);
        }
        
        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, format, XRCpuImage.Transformation.MirrorY);
        var rawTextureData = _camTexture.GetRawTextureData<byte>();
        
        try
        {
            cpuImage.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            cpuImage.Dispose();
        }
        
        _camTexture.Apply();
            
        Graphics.Blit(_camTexture, _cameraTexture, _textureMat);
    }
}
