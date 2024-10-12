using MagicLeap.OpenXR.Features.PhysicalOcclusion;
using UnityEngine;
using UnityEngine.XR.OpenXR;

public class Occlusion : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The sources to use for the Physical Occlusion Feature.")]
    private MagicLeapPhysicalOcclusionFeature.OcclusionSource  _occlusionSource = 
                        MagicLeapPhysicalOcclusionFeature.OcclusionSource.Controller |
                        MagicLeapPhysicalOcclusionFeature.OcclusionSource.DepthSensor |
                        MagicLeapPhysicalOcclusionFeature.OcclusionSource.Hands;
    
    [Tooltip("Depth sensor operates at 30fps when maximum range is 0.9 metters or lower. (min 0.3, max 7.5)")]
    [SerializeField]
    private float _nearRange = 0.3f;
    
    [Tooltip("Depth sensor operates at 30fps when maximum range is 0.9 metters or lower. (min 0.3, max 7.5)")]
    [SerializeField]
    private float _farRange = 0.9f;

    private MagicLeapPhysicalOcclusionFeature occlusionFeature;

    void Start()
    {
        // Initialize the Physical Occlusion feature
        occlusionFeature = OpenXRSettings.Instance.GetFeature<MagicLeapPhysicalOcclusionFeature>();

        if (occlusionFeature == null || !occlusionFeature.enabled)
        {
            Debug.LogError("Physical Occlusion feature must be enabled.");
            enabled = false;
            return;
        }

        // Enable occlusion and configure sources
        occlusionFeature.EnableOcclusion = true;
        occlusionFeature.EnabledOcclusionSource = _occlusionSource;

        // Configure depth sensor properties to the range specified in the inspector,
        // This will limit environment occlusion to this range as well
        occlusionFeature.DepthSensorNearRange = _nearRange;
        occlusionFeature.DepthSensorFarRange = _farRange;
    }

    void OnDestroy()
    {
        if (occlusionFeature != null && occlusionFeature.enabled)
        {
            // Disable occlusion when the script is destroyed
            occlusionFeature.EnableOcclusion = false;
            occlusionFeature.EnabledOcclusionSource = 0;
        }
    }
}