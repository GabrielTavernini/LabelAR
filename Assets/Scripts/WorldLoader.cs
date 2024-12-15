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
    private Orchestrator orchestrator;
    private XRInteractionManager interactionManager;

    private GameObject buildings;

    private bool enableColliders = false;

    public static readonly int X_offset = 2600000;
    public static readonly int Z_offset = 1200000;
    public static readonly int radius = 2000;
    public static readonly int colliderRadius = 1000;

    public WorldLoader(GameObject buildings, Orchestrator orchestrator, XRInteractionManager interactionManager)
    {
        this.buildings = buildings;
        this.orchestrator = orchestrator;
        this.interactionManager = interactionManager;
    }

    public void EnableColliders(bool fast = true) {
        enableColliders = true;
        if(!fast)
            foreach(MeshCollider collider in buildings.GetComponentsInChildren<MeshCollider>(true))
                collider.enabled = true;

    }

    public void DisableColliders(bool fast = true) {
        enableColliders = false;
        if(!fast)
            foreach(MeshCollider collider in buildings.GetComponentsInChildren<MeshCollider>(true))
                collider.enabled = false;
    }
  
    public IEnumerator GenerateWorld(HashSet<string> highlightedBuildings)
    {
        foreach (var mesh in Resources.LoadAll<Mesh>("Terrain/")) {
            yield return SpawnTerrain(mesh);
        }
        yield return null;

        int counter = 0;
        foreach (var mesh in Resources.LoadAll<Mesh>("Buildings/")) {
            bool highlight = highlightedBuildings.Contains(mesh.name);
            SpawnBuilding(mesh, highlight);
            if(counter++ % 400 == 0) yield return null;
        }

        orchestrator.TryStartAlignment();
    }

    public void AddInteractionListeners(GameObject building, bool highlight)
    {
        var interactable = building.GetComponent<XRSimpleInteractable>();
        var meshRenderer = building.GetComponent<MeshRenderer>();
        var mesh = building.GetComponent<MeshFilter>().sharedMesh;
    
        interactable.hoverEntered.AddListener(_ => HoverEnteredBuilding(meshRenderer, highlight));
        interactable.hoverExited.AddListener(_ => HoverExitedBuilding(meshRenderer, highlight));
        interactable.selectExited.AddListener(_ => SelectedBuilding(building, mesh, highlight));
    }

    GameObject SpawnBuilding(Mesh mesh, bool highlight) {
        double distance = Vector3.Distance(mesh.bounds.center, -buildings.transform.localPosition);
        if(distance > radius)
            return null;
        
        Material mat = highlight ? orchestrator.highlightMaterial : orchestrator.material;
        GameObject building = SpawnMesh(mesh, mat);

        var meshRenderer = building.GetComponent<MeshRenderer>();
        if(distance < colliderRadius) {
            var colliderComponent = building.AddComponent<MeshCollider>();
            colliderComponent.enabled = enableColliders;    
            var interactable = building.AddComponent<XRSimpleInteractable>();
            interactable.interactionManager = interactionManager;
    
            AddInteractionListeners(building, highlight);
        }

        return building;
    }

    GameObject SpawnTerrain(Mesh mesh) {
        GameObject terrain = SpawnMesh(mesh, orchestrator.material);

        var colliderComponent = terrain.AddComponent<MeshCollider>();
        colliderComponent.enabled = enableColliders;        
        var interactable = terrain.AddComponent<XRSimpleInteractable>();
        interactable.interactionManager = interactionManager;
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

    private void HoverEnteredBuilding(MeshRenderer meshRenderer, bool highlighted) {
        if(!enableColliders) return;

        if(highlighted)
            meshRenderer.material = orchestrator.editMaterial;
        
        if(!highlighted && !orchestrator.EditMode)
            meshRenderer.material = orchestrator.highlightMaterial;
    }
    
    private void HoverExitedBuilding(MeshRenderer meshRenderer, bool highlighted) {
        if(!enableColliders) return;

        if(highlighted)
            meshRenderer.material = orchestrator.highlightMaterial;
        else
            meshRenderer.material = orchestrator.material;
    }

    private void SelectedBuilding(GameObject building, Mesh mesh, bool highlighted) {
        if(!enableColliders) return;

        Debug.Log($"Building hit at: {mesh.bounds.center}");
        if(highlighted) {
            string labelName = Request.response.labels.Find(l => l.buildings.Contains(mesh.name)).name;
            orchestrator.SetEditMode(true);
            orchestrator.EditLabel(labelName, building);
        } else if (!orchestrator.EditMode) {
            AddLabelPayload payload = new AddLabelPayload();
            payload.east = mesh.bounds.center.x + X_offset;
            payload.north = mesh.bounds.center.z + Z_offset;
            payload.height = mesh.bounds.center.y + 20;
            payload.buildings = new List<string>(){mesh.name};        
            
            Debug.Log("Selected Building: " + building);
            orchestrator.CreateLabel(payload, building);
        }
    }

    private void SelectedTerrain(GameObject terrain) {
        if(!enableColliders) return;
        
        if(!orchestrator.EditMode) {
            GameObject.FindAnyObjectByType<XRRayInteractor>().TryGetCurrent3DRaycastHit(out RaycastHit raycastHit);
            Debug.Log($"Terrain hit at: {raycastHit.point}");

            AddLabelPayload payload = new AddLabelPayload();
            Vector3 rayHitPoint = Quaternion.Inverse(buildings.transform.rotation) * raycastHit.point;
            payload.east = rayHitPoint.x + Request.response.coordinates.east;
            payload.north = rayHitPoint.z + Request.response.coordinates.north;
            payload.height = rayHitPoint.y + Request.response.coordinates.altitude + 50;
            payload.buildings = new List<string>(){};        

            orchestrator.CreateLabel(payload);
        }
    }
}
