using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class LabelLoader
{
    private GameObject labels;
    private List<(GameObject, float)> labelObjects = new();
    private Material textMaterial;
    private float visibility;

    public LabelLoader(GameObject labels, Material textMaterial)
    {
        this.labels = labels;
        this.textMaterial = textMaterial;
    }

    public IEnumerator SpawnLabels(Response response) {
        visibility = response.visibility;

        int counter = 0;
        foreach (Label l in response.labels) {
            GameObject obj = SpawnLabel(l);
            labelObjects.Add((obj, l.distance));

            if(counter++ % 500 == 0) yield return null;
        }
    }

    public void UpdateVisibility(bool useVisibility)
    {
        foreach ((GameObject obj, float distance) in labelObjects)
            obj.SetActive(!useVisibility || distance <= visibility);
    }

    public GameObject SpawnLabel(Label label)
    {
        Debug.Log("Spawning label " + label.name + " at " + label.x + " " + label.y + " " + label.z);
        GameObject obj = new GameObject(); // Instantiate(instance.prefab);
        obj.transform.parent = labels.transform;
        obj.transform.localPosition = new Vector3(label.x, label.y, label.z);
        obj.name = label.name;

        float scale = Math.Max(Math.Min((float)(1 + 12*label.distance/1000), 200), 7);
        obj.transform.localScale = new Vector3(scale, scale, scale);

        if(label.distance > Orchestrator.farClippingBound) {
            Vector3 direction = obj.transform.localPosition;
            direction.Normalize();
            direction *= Orchestrator.farClippingBound - 250;
            float heightGap = (label.distance/1000 - 10)*10;
            obj.transform.localPosition = direction + new Vector3(0,heightGap,0);

            GameObject beam = new GameObject();
            beam.name = "bream";
            beam.transform.parent = obj.transform;
            LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor =  Color.gray;
            lineRenderer.endColor =  Color.gray;

            lineRenderer.startWidth = 20f;
            lineRenderer.endWidth = 20f;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, obj.transform.position - new Vector3(0,heightGap,0));
            lineRenderer.SetPosition(1, obj.transform.position - new Vector3(0,scale,0));
            lineRenderer.useWorldSpace = false;
        }

        // Add a TextMesh component to display the text
        TextMeshPro textMesh = obj.AddComponent<TextMeshPro>();
        textMesh.text = label.name;
        textMesh.color = Color.gray;   // Set the text color
        textMesh.fontSize = 18;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.material = textMaterial;

        obj.AddComponent<FaceCamera>();
        Debug.Log("Spawned label " + label.name + " at " + obj.transform.position);
        
        return obj;
    }
}
