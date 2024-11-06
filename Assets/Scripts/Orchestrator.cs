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
    [SerializeField] private GameObject markerVisualPrefab;

    [SerializeField] private XRInteractionManager interactionManager;

    [SerializeField] private Shader viewShader;

    [SerializeField] private Material editMaterial;

    [SerializeField] private Material labelMaterial;

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
        worldLoader = new WorldLoader(buildings, labelMaterial, editMaterial);
        labelLoader = new LabelLoader(labels, textMaterial);
    }

    public IEnumerator Open(string mapName)
    {
        yield return spatialAnchors.Start();

        marker = Instantiate(markerVisualPrefab);
        marker.name = "Marker";

        if (spatialAnchors.anchorFound)
        {
            Debug.Log("Anchor found. Loading aligned map and labels.");
            marker.transform.parent = spatialAnchors.anchor.transform;
            marker.transform.localPosition = new Vector3();
            marker.transform.localRotation = Quaternion.identity;

            marker.GetComponent<XRGrabInteractable>().interactionManager = null;
            editMaterial.shader = viewShader;
        }
        else
        {
            Debug.Log("Anchor NOT found. Entering alignment mode.");
            marker.transform.position = new Vector3(0, 0, 1);
            marker.transform.rotation = Quaternion.Euler(new Vector3(0, 120, 0));

            marker.AddComponent<Adjustment>();
            marker.GetComponent<XRGrabInteractable>().interactionManager = interactionManager;
        }

        yield return Load(0);
    }

    private IEnumerator Load(int code)
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

    void OnDestroy()
    {
    }
}