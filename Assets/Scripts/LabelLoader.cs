using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Label
{
    public float x;
    public float y;
    public float z;
    public float distance;
    public string name;
}

public class LabelLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    
    [SerializeField]
    private Material textMaterial;

    static private LabelLoader instance;

    void Start()
    {
        instance = this;
    }

    static public IEnumerator Load(int code, GameObject marker) {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spawning labels starting from");
        builder.AppendLine("Code: " + code);
        builder.AppendLine("Position: " + marker.transform.position);
        builder.AppendLine("Rotation: " + marker.transform.rotation);
        Debug.Log(builder.ToString());

        string url = "labelar.ilbrigante.me/get_labels?number=" + code;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Labels received from server: " + jsonResponse);
                List<Label> response = JsonConvert.DeserializeObject<List<Label>>(jsonResponse);

                Debug.Log("Spawning " + response.Count + " labels!");
                // Spawn game objects with text at each position
                foreach (Label l in response)
                    SpawnObjectAtPosition(l, marker);
            }
        }
    }

    static void SpawnObjectAtPosition(Label label, GameObject marker)
    {   
        GameObject obj = Instantiate(instance.prefab, new Vector3(label.x, label.y, label.z), Quaternion.identity);
        obj.name = label.name;
        
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        // int scale = 3*3;
        // obj.transform.localScale = new Vector3(scale, scale, scale);
        obj.transform.parent = marker.transform;

        // Add a TextMesh component to display the text
        TextMeshPro textMesh = obj.AddComponent<TextMeshPro>();
        textMesh.text = label.name;
        textMesh.color = Color.gray;   // Set the text color
        textMesh.fontSize = 18;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.material = instance.textMaterial;

        Debug.Log("Spawned label " + label.name + " at " + obj.transform.position);
    }
}
