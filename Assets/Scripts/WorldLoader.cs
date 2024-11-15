using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class WorldLoader
{   
    private Material highlightMaterial;
    private Material material;

    private Orchestrator orchestrator;
    private XRInteractionManager interactionManager;

    private GameObject buildings;

    private bool enableColliders = false;

    public static readonly int X_offset = 2600000;
    public static readonly int Z_offset = 1200000;
    public static readonly int radius = 2500;

    public WorldLoader(GameObject buildings, Material highlightMaterial, Material material, Orchestrator orchestrator, XRInteractionManager interactionManager)
    {
        this.buildings = buildings;
        this.highlightMaterial = highlightMaterial;
        this.material = material;
        this.orchestrator = orchestrator;
        this.interactionManager = interactionManager;
    }

    public void EnableColliders() {
        enableColliders = true;
        foreach(MeshCollider collider in buildings.GetComponentsInChildren<MeshCollider>(true))
            collider.enabled = true;
    }

    public void DisableColliders() {
        foreach(MeshCollider collider in buildings.GetComponentsInChildren<MeshCollider>(true))
            collider.enabled = false;
    }
  
    public IEnumerator GenerateWorld(HashSet<string> highlightedBuildings)
    {
        int counter = 0;
        foreach (var mesh in Resources.LoadAll<Mesh>("Buildings/")) {
            bool highlight = highlightedBuildings.Contains(mesh.name);
            SpawnMesh(mesh, highlight, !highlight);
            if(counter++ % 500 == 0) yield return null;
        }

        yield return null;
        foreach (var mesh in Resources.LoadAll<Mesh>("Terrain/")) {
            yield return SpawnMesh(mesh, ignoreRadius:true);
        }
    }

    GameObject SpawnMesh(Mesh mesh, bool highlight = false, bool collider = false, bool ignoreRadius = false)
    {
        double distance = Vector3.Distance(mesh.bounds.center, -buildings.transform.localPosition);
        if(!ignoreRadius && distance > radius)
            return null;

        GameObject polyfaceMeshObj = new GameObject(mesh.name);
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

        if(collider) {
            var colliderComponent = polyfaceMeshObj.AddComponent<MeshCollider>();
            colliderComponent.enabled = enableColliders;
            
            var interactable = polyfaceMeshObj.AddComponent<XRSimpleInteractable>();
            interactable.interactionManager = this.interactionManager;
            interactable.hoverEntered.AddListener(_ => meshRenderer.material = highlightMaterial);
            interactable.hoverExited.AddListener(_ => meshRenderer.material = material);

            interactable.selectExited.AddListener(_ => SelectedMesh(polyfaceMeshObj, mesh));
        }

        return polyfaceMeshObj;
    }

    private void SelectedMesh(GameObject building, Mesh mesh) {
        Payload payload = new Payload();
        payload.name = mesh.name;
        payload.east = mesh.bounds.center.x + X_offset;
        payload.north = mesh.bounds.center.z + Z_offset;
        payload.height = mesh.bounds.center.y + 20;
        payload.buildings = new List<string>(){mesh.name};        

        orchestrator.CreateLabel(payload, building);
    }
}
