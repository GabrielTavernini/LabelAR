using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
            SpawnBuilding(mesh, highlight);
            if(counter++ % 500 == 0) yield return null;
        }

        yield return null;
        foreach (var mesh in Resources.LoadAll<Mesh>("Terrain/")) {
            yield return SpawnTerrain(mesh);
        }
    }

    GameObject SpawnBuilding(Mesh mesh, bool highlight) {
        double distance = Vector3.Distance(mesh.bounds.center, -buildings.transform.localPosition);
        if(distance > radius)
            return null;
        
        Material mat = highlight ? highlightMaterial : material;
        GameObject building = SpawnMesh(mesh, mat);

        if(!highlight) {
            var colliderComponent = building.AddComponent<MeshCollider>();
            colliderComponent.enabled = enableColliders;
            
            var meshRenderer = building.GetComponent<MeshRenderer>();
            var interactable = building.AddComponent<XRSimpleInteractable>();
            interactable.interactionManager = this.interactionManager;
            interactable.hoverEntered.AddListener(_ => meshRenderer.material = highlightMaterial);
            interactable.hoverExited.AddListener(_ => meshRenderer.material = material);

            interactable.selectExited.AddListener(_ => SelectedBuilding(building, mesh));
        }

        return building;
    }

    GameObject SpawnTerrain(Mesh mesh) {
        GameObject terrain = SpawnMesh(mesh, material);

        var colliderComponent = terrain.AddComponent<MeshCollider>();
        colliderComponent.enabled = enableColliders;
        
        var interactable = terrain.AddComponent<XRSimpleInteractable>();
        interactable.interactionManager = this.interactionManager;
        interactable.selectExited.AddListener(_ => SelectedTerrain(terrain));

        return terrain;
    }

    GameObject SpawnMesh(Mesh mesh, Material material)
    {
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
        meshRenderer.material = material;

        return polyfaceMeshObj;
    }

    private void SelectedBuilding(GameObject building, Mesh mesh) {
        Debug.Log($"Building hit at: {mesh.bounds.center}");
        Payload payload = new Payload();
        payload.east = mesh.bounds.center.x + X_offset;
        payload.north = mesh.bounds.center.z + Z_offset;
        payload.height = mesh.bounds.center.y + 20;
        payload.buildings = new List<string>(){mesh.name};        

        orchestrator.CreateLabel(payload, building);
    }

    private void SelectedTerrain(GameObject terrain) {
        GameObject.FindAnyObjectByType<XRRayInteractor>().TryGetCurrent3DRaycastHit(out RaycastHit raycastHit);
        Debug.Log($"Terrain hit at: {raycastHit.point}");

        Payload payload = new Payload();
        Vector3 rayHitPoint = Quaternion.Inverse(buildings.transform.rotation) * raycastHit.point;
        payload.east = rayHitPoint.x + Request.response.coordinates.east;
        payload.north = rayHitPoint.z + Request.response.coordinates.north;
        payload.height = rayHitPoint.y + Request.response.coordinates.altitude + 50;
        payload.buildings = new List<string>(){};        

        orchestrator.CreateLabel(payload);
    }
}
