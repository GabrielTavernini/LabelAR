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
    static private MagicLeapMarkerUnderstandingFeature markerFeature;
    static public bool firstDetection = true;
    static public GameObject aprilTag;
    static private MarkerUnderstanding instance;

    private void Start()
    {   
        instance = this;
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

    public static void SetAprilCode(int code, Vector3 pos, Quaternion rot)
    {
        aprilTag.transform.position = pos;
        aprilTag.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);

        StringBuilder builder = new();
        builder.AppendLine("Marker:" + code);
        builder.AppendLine("Position: " + aprilTag.transform.position);
        builder.AppendLine("Rotation: " + aprilTag.transform.rotation);
        aprilTag.GetComponentInChildren<TextMeshPro>().text = builder.ToString();

        if(firstDetection) {
            firstDetection = false;
            instance.StartCoroutine(LabelLoader.Load(code, aprilTag));
        }
    }

    public static void FreezeAprilCode() {
        aprilTag.GetComponent<XRGrabInteractable>().interactionManager = null;
    }

    public static void UnfreezeAprilCode() {
        if(instance == null) return; // ignore calls before class is setup
        aprilTag.GetComponent<XRGrabInteractable>().interactionManager = instance.interactionManager;
    }


    private void Update()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
#else
        markerFeature.UpdateMarkerDetectors();
        if(markerFeature.MarkerDetectors.Count == 0) return;

        MarkerDetector detector = markerFeature.MarkerDetectors[0];
        MarkerData? markerData = detector.Data.Where(d => d.MarkerPose != null).FirstOrDefault();

        if (markerData.HasValue && markerData.Value.MarkerPose.HasValue 
            && markerData.Value.MarkerPose.Value.position.magnitude > 0)
        {
            SetAprilCode(
                (int) markerData.Value.MarkerNumber, 
                markerData.Value.MarkerPose.Value.position, 
                markerData.Value.MarkerPose.Value.rotation
            );
            MarkerUnderstanding.Stop();
        }
#endif
    }

    static public void Stop()
    {
        markerFeature.DestroyAllMarkerDetectors();
    }

    private void OnDestroy()
    {
        Destroy(aprilTag);
        markerFeature.DestroyAllMarkerDetectors();
    }
}
