using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLoader
{   
    private Material highlightMaterial;
    private Material material;

    private GameObject buildings;

    public static readonly int X_offset = 2600000;
    public static readonly int Z_offset = 1200000;

    public WorldLoader(GameObject buildings, Material highlightMaterial, Material material)
    {
        this.buildings = buildings;
        this.highlightMaterial = highlightMaterial;
        this.material = material;
    }

    // public void ChangeShader(bool transparent) {
    //     if(instance == null) return; // ignore calls before class is setup

    //     if(transparent)
    //         instance.material.shader = instance.transparentShader;
    //     else
    //         instance.material.shader = instance.materialShader;
    // }
  
    public IEnumerator GenerateWorld(HashSet<string> highlightedBuildings)
    {
        int counter = 0;
        foreach (var mesh in Resources.LoadAll<Mesh>("Buildings/")) {
            SpawnMesh(mesh, highlight:highlightedBuildings.Contains(mesh.name));
            if(counter++ % 500 == 0) yield return null;
        }

        yield return null;
        foreach (var mesh in Resources.LoadAll<Mesh>("Terrain/")) {
            yield return SpawnMesh(mesh, ignoreRadius:true);
        }
    }

    GameObject SpawnMesh(Mesh mesh, bool highlight = false, bool ignoreRadius = false)
    {
        // if(!ignoreRadius && Vector3.Distance(mesh.bounds.center + buildings.transform.localPosition, MarkerUnderstanding.aprilTag.transform.position) > 1000)
        //     return null;

        string Handle = mesh.name;
        GameObject polyfaceMeshObj = new GameObject(Handle);
        polyfaceMeshObj.transform.parent = buildings.transform;
        polyfaceMeshObj.transform.localPosition = new Vector3();
        polyfaceMeshObj.transform.localRotation = Quaternion.identity;

        // Add MeshFilter and MeshRenderer components
        MeshFilter meshFilter = polyfaceMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = polyfaceMeshObj.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        meshFilter.mesh = mesh;
        meshRenderer.material = highlight ? highlightMaterial : material;

        return polyfaceMeshObj;
    }
}
