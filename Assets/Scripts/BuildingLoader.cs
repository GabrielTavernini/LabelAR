using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using netDxf;
using netDxf.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class BuildingLoader : MonoBehaviour
{

    [SerializeField]
    private Material material;

    [SerializeField]
    private Material highlightMaterial;

    [SerializeField]
    private Material highlightMaterial2;

    private GameObject buildings;

    private static BuildingLoader instance;

    // Start is called before the first frame update
    void Start()
    {
        buildings = new GameObject("Buildings");
        instance = this;
    }

    public static IEnumerator GenerateBuildings(Coordinates markerCoordinates, GameObject marker)
    {
        Debug.Log("Marker swiss coords: " + markerCoordinates);
        instance.buildings.transform.parent = marker.transform;
        instance.buildings.transform.localPosition = new UnityEngine.Vector3(
            -markerCoordinates.east,
            -markerCoordinates.altitude,
            -markerCoordinates.north
        );
        instance.buildings.transform.localRotation = Quaternion.identity;
        
        int counter = 0;
        foreach (var mesh in Resources.LoadAll<UnityEngine.Mesh>("Buildings/")) {
            instance.SpawnMesh(mesh);
            if(counter++ % 500 == 0) yield return null;
        }
    }

    GameObject SpawnMesh(UnityEngine.Mesh mesh)
    {
        string Handle = mesh.name;
        GameObject polyfaceMeshObj = new GameObject(Handle);
        polyfaceMeshObj.transform.parent = buildings.transform;
        polyfaceMeshObj.transform.localPosition = new UnityEngine.Vector3();
        polyfaceMeshObj.transform.localRotation = Quaternion.identity;

        // Add MeshFilter and MeshRenderer components
        MeshFilter meshFilter = polyfaceMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = polyfaceMeshObj.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        meshFilter.mesh = mesh;
        if (Handle == "5BEA1")
            meshRenderer.material = highlightMaterial;
        else if (Handle == "1CD66")
            meshRenderer.material = highlightMaterial2;
        else
            meshRenderer.material = material;

        return polyfaceMeshObj;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
