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

public class MarkerUnderstanding : MonoBehaviour
{
    [SerializeField]
    private GameObject markerVisualPrefab;
    private MagicLeapMarkerUnderstandingFeature markerFeature;
    private bool firstDetection = true;
    static public Quaternion adjustment = Quaternion.Euler(-90, 0, 0);
    static private GameObject aprilTag;

    private void Start()
    {
        markerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapMarkerUnderstandingFeature>();
        // detectorVisuals = new HashSet<GameObject>();
        CreateDetector();
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
        aprilTag = Instantiate(markerVisualPrefab);
    }


    private void Update()
    {
        markerFeature.UpdateMarkerDetectors();
        MarkerDetector detector = markerFeature.MarkerDetectors[0];
        MarkerData? markerData = detector.Data.Where(d => d.MarkerPose != null).FirstOrDefault();

        if (markerData.HasValue && markerData.Value.MarkerPose.HasValue)
        { 
            float magnitude = (aprilTag.transform.position - markerData.Value.MarkerPose.Value.position).magnitude;
            float angle = Quaternion.Angle(aprilTag.transform.rotation, markerData.Value.MarkerPose.Value.rotation);
            if(!firstDetection && (magnitude < 0.1 || angle < 0.1))
                return; // no need to update on small movements

            aprilTag.transform.position = markerData.Value.MarkerPose.Value.position;
            aprilTag.transform.rotation = markerData.Value.MarkerPose.Value.rotation;
            // aprilTag.transform.rotation *= adjustment;

            StringBuilder builder = new();
            builder.AppendLine("Marker:" + markerData.Value.MarkerString);
            builder.AppendLine("Position: " + aprilTag.transform.position);
            builder.AppendLine("Rotation: " + aprilTag.transform.rotation);
            aprilTag.GetComponentInChildren<TextMeshPro>().text = builder.ToString();
            
            if(firstDetection) 
            {
                firstDetection = false;
                StartCoroutine(LabelLoader.Load((int) markerData.Value.MarkerNumber, aprilTag));
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(aprilTag);
        markerFeature.DestroyAllMarkerDetectors();
    }
}
