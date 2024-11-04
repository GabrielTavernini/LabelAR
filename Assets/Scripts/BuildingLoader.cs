using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using netDxf;
using netDxf.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class BuildingLoader : MonoBehaviour
{
    [SerializeField]
    private Shader transparentShader;

    [SerializeField]
    private Material material;
    private Shader materialShader;

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
        materialShader = material.shader;
        instance = this;
    }

    public static void ChangeShader(bool transparent) {
        if(instance == null) return; // ignore calls before class is setup

        if(transparent)
            instance.material.shader = instance.transparentShader;
        else
            instance.material.shader = instance.materialShader;
    }
  
    public static IEnumerator GenerateBuildings(GameObject marker)
    {
        Coordinates markerCoordinates = LabelLoader.response.coordinates;
        Debug.Log("Marker swiss coords: " + markerCoordinates);
        instance.buildings.transform.parent = marker.transform;
        instance.buildings.transform.localPosition = new UnityEngine.Vector3(
            -(markerCoordinates.east - 2600000),
            -markerCoordinates.altitude,
            -(markerCoordinates.north - 1200000) 
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
        meshRenderer.material = material;
        if(LabelLoader.response.buildings.Contains(Handle))
            meshRenderer.material = highlightMaterial;

        return polyfaceMeshObj;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
