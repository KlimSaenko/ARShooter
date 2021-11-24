using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// A component that can be used to access the most
/// recently received light estimation information
/// for the physical environment as observed by an
/// AR device.
/// </summary>
[RequireComponent(typeof(Light))]
public class BasicLightEstimation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events containing light estimation information.")]
    private ARCameraManager m_CameraManager;

    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    internal ARCameraManager cameraManager
    {
        get { return m_CameraManager; }
        set
        {
            if (m_CameraManager == value)
                return;

            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;

            m_CameraManager = value;

            if (m_CameraManager != null & enabled)
                m_CameraManager.frameReceived += FrameChanged;
        }
    }

    /// <summary>
    /// The estimated brightness of the physical environment, if available.
    /// </summary>
    internal float? brightness { get; private set; }

    /// <summary>
    /// The estimated color temperature of the physical environment, if available.
    /// </summary>
    internal float? colorTemperature { get; private set; }

    /// <summary>
    /// The estimated color correction value of the physical environment, if available.
    /// </summary>
    internal Color? colorCorrection { get; private set; }

    /// <summary>
    /// The estimated direction of the main light of the physical environment, if available.
    /// </summary>
    internal Vector3? mainLightDirection { get; private set; }

    /// <summary>
    /// The estimated color of the main light of the physical environment, if available.
    /// </summary>
    internal Color? mainLightColor { get; private set; }

    /// <summary>
    /// The estimated intensity in lumens of main light of the physical environment, if available.
    /// </summary>
    internal float? mainLightIntensityLumens { get; private set; }

    /// <summary>
    /// The estimated spherical harmonics coefficients of the physical environment, if available.
    /// </summary>
    internal SphericalHarmonicsL2? sphericalHarmonics { get; private set; }

    [SerializeField]
    private float m_BrightnessMod = 2.0f;

    private void Awake()
    {
        m_Light = GetComponent<Light>();
    }

    private void OnEnable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived += FrameChanged;
    }

    private void OnDisable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived -= FrameChanged;
    }

    private void FrameChanged(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            brightness = args.lightEstimation.averageBrightness.Value;
            m_Light.intensity = brightness.Value * m_BrightnessMod;
        }

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            colorTemperature = args.lightEstimation.averageColorTemperature.Value;
            m_Light.colorTemperature = colorTemperature.Value;
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            colorCorrection = args.lightEstimation.colorCorrection.Value;
            m_Light.color = colorCorrection.Value;
        }

        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            mainLightDirection = args.lightEstimation.mainLightDirection;
            m_Light.transform.rotation = Quaternion.LookRotation(mainLightDirection.Value);
        }

        if (args.lightEstimation.mainLightColor.HasValue)
        {
            mainLightColor = args.lightEstimation.mainLightColor;
            m_Light.color = mainLightColor.Value;
        }

        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            mainLightIntensityLumens = args.lightEstimation.mainLightIntensityLumens;
            m_Light.intensity = args.lightEstimation.averageMainLightBrightness.Value;
        }

        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            sphericalHarmonics = args.lightEstimation.ambientSphericalHarmonics;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = sphericalHarmonics.Value;
        }
    }

    private Light m_Light;
}
