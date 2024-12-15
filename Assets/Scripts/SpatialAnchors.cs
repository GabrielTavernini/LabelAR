using System.Collections;
using System.Collections.Generic;
using MagicLeap.OpenXR.Features.LocalizationMaps;
using MagicLeap.OpenXR.Features.SpatialAnchors;
using MagicLeap.OpenXR.Subsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;

public class SpatialAnchors
{
    public ARAnchorManager anchorManager { get; }
    private MagicLeapSpatialAnchorsStorageFeature anchorStorageFeature;
    private MagicLeapLocalizationMapFeature mapFeature;
    private MLXrAnchorSubsystem activeSubsystem;
    private readonly List<ARAnchor> activeAnchorsStored = new();
    private bool queryResponseReceived = false;
    public ARAnchor anchor { get; private set; }
    public bool anchorFound { get; private set; } = false;

    public SpatialAnchors(ARAnchorManager anchorManager)
    {
        this.anchorManager = anchorManager;
    }

    public IEnumerator Start(string spaceName)
    {
        Debug.Log("Starting spatial anchor subsystem!");
        yield return new WaitUntil(AreSubsystemsLoaded);
        anchorStorageFeature = OpenXRSettings.Instance.GetFeature<MagicLeapSpatialAnchorsStorageFeature>();
        mapFeature = OpenXRSettings.Instance.GetFeature<MagicLeapLocalizationMapFeature>();
        mapFeature.EnableLocalizationEvents(true);

        anchorStorageFeature.OnCreationCompleteFromStorage += OnAnchorCompletedCreationFromStorage;
        anchorStorageFeature.OnPublishComplete += OnAnchorPublishComplete;
        anchorStorageFeature.OnQueryComplete += OnAnchorQueryComplete;
        anchorStorageFeature.OnDeletedComplete += OnAnchorDeleteComplete;
        anchorManager.anchorsChanged += OnAnchorsChanged;

        Debug.Log("Query spatial anchor subsystem!");
        anchorStorageFeature.QueryStoredSpatialAnchors(new Vector3(), 100);
        while (!queryResponseReceived || (anchorFound && anchor == null))
            yield return null;
    }

    private bool AreSubsystemsLoaded()
    {
        if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null ||
            XRGeneralSettings.Instance.Manager.activeLoader == null) return false;
        activeSubsystem =
            XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRAnchorSubsystem>() as
                MLXrAnchorSubsystem;
        return activeSubsystem != null;
    }

    public IEnumerator CreateAnchor(GameObject newAnchor)
    {
        ARAnchor toPublish = newAnchor.AddComponent<ARAnchor>();
        while (toPublish.trackingState != TrackingState.Tracking)
            yield return null;

        Debug.Log(
            "Anchors Storage count: " + activeAnchorsStored.Count + "\n"
        );
        if (activeAnchorsStored.Count > 0)
        {
            Debug.Log("DeleteStoredSpatialAnchors call response: " + anchorStorageFeature.DeleteStoredSpatialAnchors(activeAnchorsStored));
            while (activeAnchorsStored.Count > 0) yield return null;
            Debug.Log("Delete Done!");
        }
        Debug.Log("PublishSpatialAnchorsToStorage call response: " + anchorStorageFeature.PublishSpatialAnchorsToStorage(new List<ARAnchor> { toPublish }, 0));
    }

    #region persistence callbacks
    // All the anchor storage features are asynchronous and have callbacks for when the call is complete
    private void OnAnchorPublishComplete(ulong anchorId, string anchorMapPositionId)
    {
        Pose newAnchorPose = activeSubsystem.GetAnchorPoseFromId(anchorId);

        Debug.Log($"SpatialAnchorsStorageTest: Anchor Publish Complete hit for location: {newAnchorPose.ToString()}");
    }

    // This is where all persisted anchors found during a query will be returned.
    private void OnAnchorQueryComplete(List<string> anchorMapPositionIds)
    {
        // Find Anchors already known to Subsystem
        List<string> alreadyCreated = new List<string>();

        foreach (var storedAnchor in activeAnchorsStored)
        {
            string anchorMapPositionId = activeSubsystem.GetAnchorMapPositionId(storedAnchor);
            if (anchorMapPositionIds.Contains(anchorMapPositionId))
            {
                alreadyCreated.Add(anchorMapPositionId);
            }
        }

        // Create New Anchors
        List<string> createStoredAnchors = new List<string>();

        foreach (string storedAnchor in anchorMapPositionIds)
        {
            TrackableId subsystemId = activeSubsystem.GetTrackableIdFromMapPositionId(storedAnchor);
            ARAnchor foundAnchor = anchorManager.GetAnchor(subsystemId);
            if (!alreadyCreated.Contains(storedAnchor) && foundAnchor == null)
            {
                createStoredAnchors.Add(storedAnchor);
            }
        }

        Debug.Log("OnAnchorQueryComplete: \n" +
            "anchorMapPositionIds count: " + anchorMapPositionIds.Count + "\n" +
            "activeAnchorsStored count: " + activeAnchorsStored.Count + "\n" +
            "alreadyCreated count: " + alreadyCreated.Count + "\n" +
            "createStoredAnchors count: " + createStoredAnchors.Count + "\n"
        );

        queryResponseReceived = true;
        if (createStoredAnchors.Count > 0)
        {
            bool result = anchorStorageFeature.CreateSpatialAnchorsFromStorage(createStoredAnchors);
            if (!result)
                Debug.LogError("SpatialAnchorsStorageTest: Error creating Anchors from storage Id.");
            else
                anchorFound = true;
        }
    }

    // This is where anchors found by a query, that were not already in the scene, and were subsequently created from 
    // storage are instantiated into the unity scene.
    private void OnAnchorCompletedCreationFromStorage(Pose pose, ulong anchorId, string anchorMapPositionId,
        XrResult result)
    {
        Debug.Log($"SpatialAnchorsStorageTest: Anchor Creation from Storage Complete hit for location: {pose.ToString()} With result: {result.ToString()}");
    }

    // This is where a confirmation of deletion will happen and the current scene representation of the anchor
    // can be removed
    private void OnAnchorDeleteComplete(List<string> anchorMapPositionIds)
    {
        Debug.Log($"SpatialAnchorsStorageTest: Anchor Delete Complete hit with {anchorMapPositionIds.Count} results.");
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs anchorsChanged)
    {
        Debug.Log("OnAnchorsChanged: \n" +
            "Anchors Added count: " + anchorsChanged.added.Count + "\n" +
            "Anchors Updated count: " + anchorsChanged.updated.Count + "\n" +
            "Anchors Removed count: " + anchorsChanged.removed.Count + "\n"+
            "Active anchor: " + anchor + "\n"+
            "Anchors Stored count: " + activeAnchorsStored.Count + "\n"
        );
        // Check for newly added Stored Anchors this Script may not yet know about.
        if (anchorsChanged.added.Count > 0)
            this.anchor = anchorsChanged.added[0];
        if (anchorsChanged.updated.Count > 0)
            this.anchor = anchorsChanged.updated[0];

        foreach (ARAnchor anchor in anchorsChanged.added)
        {
            if (activeSubsystem.IsStoredAnchor(anchor))
            {
                Debug.Log("store added anchor");
                activeAnchorsStored.Add(anchor);
            }
        }

        foreach (ARAnchor anchor in anchorsChanged.updated)
        {
            if (activeSubsystem.IsStoredAnchor(anchor) && !activeAnchorsStored.Contains(anchor))
            {
                Debug.Log("store updated anchor");
                activeAnchorsStored.Add(anchor);
            }
        }

        // Check if we are still tracking a deleted anchor.
        foreach (ARAnchor anchor in anchorsChanged.removed)
        {
            if (activeAnchorsStored.Contains(anchor))
            {
                Debug.Log("remove storage anchor");
                activeAnchorsStored.Remove(anchor);
            }
        }

        Debug.Log("Active anchor: " + anchor + "\n"+
            "Anchors Stored count: " + activeAnchorsStored.Count + "\n"
        );
    }
    #endregion
}