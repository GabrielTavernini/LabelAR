// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using MagicLeap.OpenXR.Features.MarkerUnderstanding;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VisualScripting;

public class MarkerUnderstanding : MonoBehaviour
{
    [SerializeField]
    private GameObject markerVisualPrefab;
    [SerializeField]
    private XRInteractionManager interactionManager;
    private MagicLeapMarkerUnderstandingFeature markerFeature;
    static public bool firstDetection = true;
    static public GameObject aprilTag;

    private void Start()
    {
        aprilTag = Instantiate(markerVisualPrefab);
        aprilTag.name = "Marker";
        aprilTag.GetComponent<XRGrabInteractable>().interactionManager = interactionManager;

#if !UNITY_ANDROID || UNITY_EDITOR
        aprilTag.transform.position = new Vector3(0, 0, 0);
        aprilTag.transform.rotation = Quaternion.Euler(new Vector3(0, 120, 0));

        StringBuilder builder = new();
        builder.AppendLine("Position: " + aprilTag.transform.position);
        builder.AppendLine("Rotation: " + aprilTag.transform.rotation);
        aprilTag.GetComponentInChildren<TextMeshPro>().text = builder.ToString();

        StartCoroutine(LabelLoader.Load((int)0, aprilTag));
#else
        markerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapMarkerUnderstandingFeature>();
        CreateDetector();
#endif
    }

    private void CreateDetector()
    {
        //Create a detector based on a subset of settings. For fully customizable detector
        //creation, see the Marker Tracking scene in our Examples project. 
        MarkerDetectorSettings markerDetectorSettings = new();
        markerDetectorSettings.MarkerDetectorProfile = MarkerDetectorProfile.Default;

        markerDetectorSettings.MarkerType = MarkerType.AprilTag;
        markerDetectorSettings.AprilTagSettings.AprilTagType = AprilTagType.Dictionary_16H5;
        markerDetectorSettings.AprilTagSettings.AprilTagLength = 115 / 1000f;
        markerDetectorSettings.AprilTagSettings.EstimateAprilTagLength = true;

        markerFeature.CreateMarkerDetector(markerDetectorSettings);
    }


    private void Update()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
#else
        markerFeature.UpdateMarkerDetectors();
        MarkerDetector detector = markerFeature.MarkerDetectors[0];
        MarkerData? markerData = detector.Data.Where(d => d.MarkerPose != null).FirstOrDefault();

        if (markerData.HasValue && markerData.Value.MarkerPose.HasValue 
            && markerData.Value.MarkerPose.Value.position.magnitude > 0)
        {
            aprilTag.transform.position = markerData.Value.MarkerPose.Value.position;
            if (firstDetection)
                aprilTag.transform.rotation = Quaternion.Euler(0, markerData.Value.MarkerPose.Value.rotation.eulerAngles.y, 0);

            StringBuilder builder = new();
            builder.AppendLine("Marker:" + markerData.Value.MarkerString);
            builder.AppendLine("Position: " + aprilTag.transform.position);
            builder.AppendLine("Rotation: " + aprilTag.transform.rotation);
            aprilTag.GetComponentInChildren<TextMeshPro>().text = builder.ToString();

            if (firstDetection) {
                firstDetection = false;
                StartCoroutine(LabelLoader.Load((int)markerData.Value.MarkerNumber, aprilTag));
            }
        }
#endif
    }

    private void OnDestroy()
    {
        Destroy(aprilTag);
        markerFeature.DestroyAllMarkerDetectors();
    }
}
