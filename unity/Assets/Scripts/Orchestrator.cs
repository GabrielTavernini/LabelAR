using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR;
using UnityEngine.InputSystem;
using System;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class Orchestrator : MonoBehaviour
{
    public static readonly bool DEMO = true;
    [SerializeField] private GameObject sceneSelection;
    [SerializeField] private GameObject viewSettings;
    [SerializeField] private GameObject alignmentMenu;
    [SerializeField] private GameObject editLabels;
    [SerializeField] private GameObject newLabel;
    [SerializeField] private GameObject connectionError;
    [SerializeField] private ControllerHelper controllerHelper;
    public GameObject keyboard;
    


    [SerializeField] private GameObject markerVisualPrefab;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private GameObject occlusionManager;
    public Shader transparentShader;
    public Shader wireframeShader;
    public Material material; // material for non-labeled buildings
    public Material highlightMaterial; // material for labeled buildings
    public Material editMaterial; // material for hovered buildings in edit mode
    public Material markerMaterial; // material for the marker
    [SerializeField] private Material textMaterial;
    [SerializeField] private ARAnchorManager anchorManager;

    public static readonly int farClippingBound = 10000;

    private SpatialAnchors spatialAnchors;
    private WorldLoader worldLoader;
    private LabelLoader labelLoader;

    public GameObject marker {get; private set;}
    private GameObject labels;
    private GameObject buildings;

    // This is was autogenerated and allows developers to create a dynamic
    // instance of an InputActionAsset which includes predefined action maps
    // that correspond to all of the Magic Leap 2's input.
    private MagicLeapInput _magicLeapInputs;

    // This class is an Action Map and was autogenerated by the Unity Input
    // System and includes predefined bindings for the Magic Leap 2 Controller
    // Input Events.
    private MagicLeapInput.ControllerActions _controllerActions;

    private bool AdjustmentMode = false;
    public bool EditMode {get; private set;} = false;

    void Start()
    {
        labels = new GameObject("labels");
        buildings = new GameObject("buildings");

        spatialAnchors = new SpatialAnchors(anchorManager);
        worldLoader = new WorldLoader(buildings, this, interactionManager);
        labelLoader = new LabelLoader(labels, textMaterial);

        // Initialize the InputActionAsset
        _magicLeapInputs = new MagicLeapInput();
        _magicLeapInputs.Enable();

        //Initialize the ControllerActions using the Magic Leap Input
        _controllerActions = new MagicLeapInput.ControllerActions(_magicLeapInputs);
        _controllerActions.MenuButton.performed += OnMenuClick;
        _controllerActions.Trigger.canceled += OnTriggerClick;

#if UNITY_EDITOR
        marker = Instantiate(markerVisualPrefab);
        marker.name = "Marker";
        marker.transform.position = new Vector3(0, 0, 0);
        marker.transform.rotation = Quaternion.Euler(new Vector3(0, 150, 0));
        StartCoroutine(LoadAssets("Polyterrasse"));

        sceneSelection.SetActive(false);
        SetAdjustmentMode(false);
        SetFarClippingPlane(farClippingBound);
        viewSettings.SetActive(true);
#endif
    }

    void Update() {
#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
            OnTriggerClick(new InputAction.CallbackContext());
#endif
    }
    
    public void SetEditMode(bool value) {
        EditMode = value;
        Debug.Log("Edit mode: " + value);
        if (value)
        {
            // Disable the ViewSettings UI and set the material to opaque
            viewSettings.SetActive(false);
            editLabels.SetActive(true);
            controllerHelper.SetMode(ControllerMode.MenuScrolling);
        }
        else
        {
            CancelLabelEdit();

            // Enable the ViewSettings UI
            viewSettings.SetActive(true);
            editLabels.SetActive(false);
            controllerHelper.SetMode(ControllerMode.Menu);
        }
    }

    public void SetAdjustmentMode(bool value)
    {
        AdjustmentMode = value;
        Debug.Log("Adjustment mode: " + value);
        if (value)
        {
            // Enable Adjustment interaction on the marker
            Adjustment adjustment = marker.AddComponent<Adjustment>();
            adjustment.orchestrator = this;
            marker.GetComponent<XRGrabInteractable>().interactionManager = interactionManager;
            MaterialHelper.SetSolid(markerMaterial);
            marker.transform.Find("Text").gameObject.SetActive(true);

            // Disable building selection
            worldLoader.DisableColliders(false);

            // Disable the ViewSettings UI and set the material to opaque
            viewSettings.SetActive(false);
            controllerHelper.SetMode(ControllerMode.Adjustment);
            TryStartAlignment();
            MaterialHelper.SetTransparent(material);
        }
        else
        {
            // Align the Origin Marker with the spatial anchor
#if !UNITY_EDITOR
            marker.transform.parent = spatialAnchors.anchor.transform;
            marker.transform.localPosition = new Vector3();
            marker.transform.localRotation = Quaternion.identity;
#endif

            // Disable Adjustment interaction on the marker
            Destroy(marker.GetComponent<Adjustment>());
            marker.GetComponent<XRGrabInteractable>().interactionManager = null;
            MaterialHelper.SetTransparent(markerMaterial, alpha: 0.0f);
            marker.transform.Find("Text").gameObject.SetActive(false);

            // Enable building selection
            worldLoader.EnableColliders(false);

            // Disable the AlignmentMenu UI
            alignmentMenu.GetComponent<Alignment>().restoreLabels();
            alignmentMenu.SetActive(false);
            MaterialHelper.SetShader(material, transparentShader);

            controllerHelper.SetMode(ControllerMode.Labeling);
        }
    }

    public void TryStartAlignment() {
        if(Request.response != null && AdjustmentMode) {
            if(!alignmentMenu.activeSelf) 
                alignmentMenu.SetActive(true);
            else 
                alignmentMenu.GetComponent<Alignment>().init();
        }
    }

    public IEnumerator Open(string mapName)
    {
        // Disable the SceneSelection UI and instantiate the Origin Marker
        marker = Instantiate(markerVisualPrefab);
        Debug.Log("Marker instantiated: " + (marker != null));
        marker.name = "Marker";
        sceneSelection.SetActive(false);

        // Start the spatial anchors subsystem
        yield return spatialAnchors.Start(mapName);

        if (spatialAnchors.anchorFound)
        {
            Debug.Log("Anchor found. Loading aligned map and labels.");
            SetAdjustmentMode(false);
        }
        else
        {
            Debug.Log("Anchor NOT found. Entering alignment mode.");
            SetAdjustmentMode(true);
        }

        yield return LoadAssets(mapName);
    }

    private IEnumerator LoadAssets(string mapName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spawning labels starting from");
        builder.AppendLine("Map Name: " + mapName);
        builder.AppendLine("Position: " + marker.transform.position);
        builder.AppendLine("Rotation: " + marker.transform.rotation);
        Debug.Log(builder.ToString());

        yield return Request.Load(mapName);
        
        if (Request.response == null) 
            connectionError.SetActive(true);

        while (Request.response == null) 
        {
            yield return new WaitForSeconds(1);
            yield return Request.Load(mapName);
            
        }
        connectionError.SetActive(false);
        
        SetFarClippingPlane(Request.response.visibility);
        SpawnWorld();
        SpawnLabels();
    }

    private void SpawnWorld()
    {
        Debug.Log("Start spawning buildings!");

        Coordinates markerCoordinates = Request.response.coordinates;
        Debug.Log("Marker swiss coords: " + markerCoordinates);
        buildings.transform.parent = marker.transform;
        buildings.transform.localPosition = new Vector3(
            -(markerCoordinates.east - WorldLoader.X_offset),
            -markerCoordinates.altitude,
            -(markerCoordinates.north - WorldLoader.Z_offset)
        );
        buildings.transform.localRotation = Quaternion.identity;

        StartCoroutine(worldLoader.GenerateWorld(Request.response.buildings));
    }

    private void SpawnLabels()
    {
        labels.transform.parent = marker.transform;
        labels.transform.localPosition = new Vector3();
        labels.transform.localRotation = Quaternion.identity;
        
        StartCoroutine(labelLoader.SpawnLabels(Request.response));
        
    }

    public void EditLabel(string name, GameObject building = null) {
        worldLoader.DisableColliders();
        controllerHelper.SetMode(ControllerMode.Selecting);
        editLabels.GetComponent<EditLabels>().InitiateEdit(name, building);
    }

    public void CancelLabelEdit() {
        worldLoader.EnableColliders();
        controllerHelper.SetMode(ControllerMode.MenuScrolling);
    }

    public void CreateLabel(AddLabelPayload payload, GameObject building = null) {
        viewSettings.SetActive(false);
        worldLoader.DisableColliders();
        newLabel.SetActive(true);
        controllerHelper.SetMode(ControllerMode.Selecting);
        newLabel.GetComponent<NewLabel>().InitiateCreation(payload, building);
    }

    public void CancelLabelCreation() {
        newLabel.SetActive(false);
        controllerHelper.SetMode(ControllerMode.Labeling);
        worldLoader.EnableColliders();
    }

    public void SetHighlight(GameObject building, bool highlight) {
        building.GetComponent<MeshRenderer>().material = highlight ? highlightMaterial : material;

        // Remove old listeners
        var interactable = building.GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.RemoveAllListeners();
        interactable.hoverExited.RemoveAllListeners();
        interactable.selectExited.RemoveAllListeners();

        // Add new listeners depending on highlight state
        worldLoader.AddInteractionListeners(building, highlight);
    }

    public void CommitLabel(AddLabelPayload payload, GameObject building = null) {
        newLabel.SetActive(false);
        controllerHelper.SetMode(ControllerMode.Labeling);
        worldLoader.EnableColliders();

        Debug.Log("Sending post request: " + JsonConvert.SerializeObject(payload));
        StartCoroutine(Request.AddLabel(payload));

        // Position of the label to spawn
        Vector3 relativePosition = new Vector3(
            payload.east, 
            payload.height, 
            payload.north
        );

        // Highlight the building
        if(building) {
            relativePosition -= new Vector3(WorldLoader.X_offset, 0, WorldLoader.Z_offset);
            relativePosition += buildings.transform.localPosition;

            SetHighlight(building, true);
        } else {
            relativePosition -= new Vector3(
                Request.response.coordinates.east, 
                Request.response.coordinates.altitude, 
                Request.response.coordinates.north
            );
            relativePosition -= labels.transform.localPosition;
        }

        // Create label and spawn it
        Label label = new Label();
        label.name = payload.name;
        label.x = relativePosition.x;
        label.y = relativePosition.y;
        label.z = relativePosition.z;
        label.distance = relativePosition.magnitude;
        label.buildings = payload.buildings;

        labelLoader.SpawnLabel(label);
        Request.response.labels.Add(label);
    }

    public IEnumerator SaveSpatialAnchor(Pose pose)
    {
        // Instantiate the new anchor object here, then pass it to SpatialAnchors
        GameObject newAnchor = Instantiate(
            spatialAnchors.anchorManager.anchorPrefab, pose.position, pose.rotation);
        yield return spatialAnchors.CreateAnchor(newAnchor);

        // Once the anchor is saved, disable the Adjustment mode
        SetAdjustmentMode(false);
    }

    public void SetFarClippingPlane(float distance)
    {
        if(distance < 0) distance = farClippingBound;
        else distance = Math.Min(distance, farClippingBound);

        Camera.main.farClipPlane = distance;
        GameObject.Find("Game Controller").GetComponent<XRRayInteractor>().maxRaycastDistance = distance;
    }

    public void SetOcclusion(bool value) {
        occlusionManager.GetComponent<Occlusion>().SetEnableOcclusion(value);
    }

    private void OnMenuClick(InputAction.CallbackContext obj)
    {
        BackToViewSettings();
    }

    public void BackToViewSettings()
    {
        if (!sceneSelection.activeSelf && !AdjustmentMode && !connectionError.activeSelf)
        {
            if(EditMode) SetEditMode(false);
            if(newLabel.activeSelf) CancelLabelCreation();
            controllerHelper.SetMode(viewSettings.activeSelf ? ControllerMode.Labeling : ControllerMode.Menu);
            viewSettings.SetActive(!viewSettings.activeSelf);
        }
    }

    private void OnTriggerClick(InputAction.CallbackContext obj)
    {
        if (!sceneSelection.activeSelf
        && !AdjustmentMode 
        && !EditMode
        && Request.response != null 
        && !viewSettings.activeSelf
        && !newLabel.activeSelf
        ){   
            Vector3 direction = GameObject.Find("Game Controller").GetComponent<XRRayInteractor>().rayEndPoint;
            if(direction.magnitude <= WorldLoader.colliderRadius) return;

            direction.Normalize();
            direction *= farClippingBound*5;

            Debug.Log("Controller direction: " + direction);
            AddLabelPayload payload = new AddLabelPayload();
            Vector3 rayHitPoint = Quaternion.Inverse(buildings.transform.rotation) * direction;
            payload.east = rayHitPoint.x + Request.response.coordinates.east;
            payload.north = rayHitPoint.z + Request.response.coordinates.north;
            payload.height = rayHitPoint.y + Request.response.coordinates.altitude;
            payload.buildings = new List<string>(){};

            CreateLabel(payload);
        }
    }

    void OnDestroy()
    {
    }

}