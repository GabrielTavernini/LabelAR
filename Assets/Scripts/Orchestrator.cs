using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Timeline;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR;

public class Orchestrator : MonoBehaviour
{
    [SerializeField] private GameObject sceneSelection;
    [SerializeField] private GameObject viewSettings;


    [SerializeField] private GameObject markerVisualPrefab;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private Shader transparentShader;
    public Material material; // material for non-labeled buildings
    public Material highlightMaterial; // material for labeled buildings
    [SerializeField] private Material textMaterial;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private bool UseVisibility;

    private SpatialAnchors spatialAnchors;
    private WorldLoader worldLoader;
    private LabelLoader labelLoader;

    private GameObject marker;
    private GameObject labels;
    private GameObject buildings;


    void Start()
    {
        labels = new GameObject("labels");
        buildings = new GameObject("buildings");

        spatialAnchors = new SpatialAnchors(anchorManager);
        worldLoader = new WorldLoader(buildings, highlightMaterial, material);
        labelLoader = new LabelLoader(labels, textMaterial);
    }

    public void SetAdjustmentMode(bool value)
    {
        Debug.Log("Adjustment mode: " + value);
        if (value)
        {
            // Put the Origin Marker in a default position
            // TODO: More principled way
            marker.transform.parent = null;
            marker.transform.position = new Vector3(0, 0, 1);
            marker.transform.rotation = Quaternion.Euler(new Vector3(0, 120, 0));

            // Enable Adjustment interaction on the marker
            Adjustment adjustment = marker.AddComponent<Adjustment>();
            adjustment.orchestrator = this;
            marker.GetComponent<XRGrabInteractable>().interactionManager = interactionManager;

            // Disable the ViewSettings UI and set the material to opaque
            viewSettings.SetActive(false);
            MaterialHelper.SetSolid(material);
        }
        else
        {
            // Align the Origin Marker with the spatial anchor
            marker.transform.parent = spatialAnchors.anchor.transform;
            marker.transform.localPosition = new Vector3();
            marker.transform.localRotation = Quaternion.identity;

            // Disable Adjustment interaction on the marker
            Destroy(marker.GetComponent<Adjustment>());
            marker.GetComponent<XRGrabInteractable>().interactionManager = null;

            // Enable the ViewSettings UI
            viewSettings.SetActive(true);
            MaterialHelper.SetFullyTransparent(material, transparentShader);
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

        yield return LoadAssets(0);
    }

    private IEnumerator LoadAssets(int code)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spawning labels starting from");
        builder.AppendLine("Code: " + code);
        builder.AppendLine("Position: " + marker.transform.position);
        builder.AppendLine("Rotation: " + marker.transform.rotation);
        Debug.Log(builder.ToString());

        yield return Request.Load(code);
        SpawnWorld();
        SpawnLabels();

        GameObject.Find("SceneSelection").SetActive(false);
        GameObject.Find("ViewSettings").SetActive(true);
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

    public IEnumerator SaveSpatialAnchor(Pose pose)
    {
        // Instantiate the new anchor object here, then pass it to SpatialAnchors
        GameObject newAnchor = Instantiate(
            spatialAnchors.anchorManager.anchorPrefab, pose.position, pose.rotation);
        yield return spatialAnchors.CreateAnchor(newAnchor);

        // Once the anchor is saved, disable the Adjustment mode
        SetAdjustmentMode(false);
    }

    void OnDestroy()
    {
    }
}