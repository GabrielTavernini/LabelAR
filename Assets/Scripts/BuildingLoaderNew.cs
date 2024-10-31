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

public class BuildingLoaderNew : MonoBehaviour
{

    [SerializeField]
    private Material material;

    [SerializeField]
    private Material highlightMaterial;

    [SerializeField]
    private Material highlightMaterial2;

    private Coordinates markerCoordinates;

    private GameObject buildings;

    private static BuildingLoaderNew instance;

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
        instance.buildings.transform.localPosition = new UnityEngine.Vector3();
        instance.buildings.transform.localRotation = Quaternion.identity;
        instance.markerCoordinates = markerCoordinates;

        string folderPath = "Assets/Buildings";
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        foreach (string guid in guids)
            yield return instance.SpawnPolyfaceMesh(AssetDatabase.GUIDToAssetPath(guid));
    }

    private UnityEngine.Vector3 ConvertCoordinates(netDxf.Vector3 vec)
    {
        float x = (float)(vec.X - this.markerCoordinates.east);
        float y = (float)(vec.Z - this.markerCoordinates.altitude);
        float z = (float)(vec.Y - this.markerCoordinates.north);
        return new UnityEngine.Vector3(x, y, z);
    }
    GameObject SpawnPolyfaceMesh(string path)
    {
        string Handle = path.Split("Assets/Resources/Buildings/")[1].Split(".")[0];
        
        GameObject polyfaceMeshObj = new GameObject(Handle);
        polyfaceMeshObj.transform.parent = buildings.transform;
        polyfaceMeshObj.transform.localPosition = new UnityEngine.Vector3();
        polyfaceMeshObj.transform.localRotation = Quaternion.identity;

        // Add MeshFilter and MeshRenderer components
        MeshFilter meshFilter = polyfaceMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = polyfaceMeshObj.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        // Create a new Unity Mesh

        UnityEngine.Mesh mesh = (UnityEngine.Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Mesh));
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
