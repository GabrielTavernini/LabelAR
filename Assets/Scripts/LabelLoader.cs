using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Coordinates
{
    public float east;
    public float north;
    public float altitude;
}

public class Label
{
    public float x;
    public float y;
    public float z;
    public float distance;
    public string name;
}

public class Response
{
    public Coordinates coordinates;
    public List<Label> labels;
    public HashSet<string> buildings;
}

public class FaceCamera : MonoBehaviour
{
    public Camera cameraToLookAt;  // Reference to the camera

    void Update()
    {
        if (cameraToLookAt != null)
        {
            // Rotate the object to face the camera
            transform.LookAt(cameraToLookAt.transform);
            // Optionally, flip the rotation if needed
            transform.Rotate(0, 180, 0);
        }
    }
}

public class LabelLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Material textMaterial;

    private GameObject labels;

    static private LabelLoader instance;

    static public Response response;

    void Start()
    {
        labels = new GameObject("Labels");
        instance = this;
    }

    static public IEnumerator Load(int code, GameObject marker)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spawning labels starting from");
        builder.AppendLine("Code: " + code);
        builder.AppendLine("Position: " + marker.transform.position);
        builder.AppendLine("Rotation: " + marker.transform.rotation);
        Debug.Log(builder.ToString());

        instance.labels.transform.parent = marker.transform;
        instance.labels.transform.localPosition = new Vector3();
        instance.labels.transform.localRotation = Quaternion.identity;

        string url = "labelar.ilbrigante.me/get_labels?number=" + code;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            // yield return null;
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
            }
            else
            {
                // Parse the JSON response
                // string jsonResponse = "{\"coordinates\":{\"east\":2683740.369,\"north\":1250632.114,\"altitude\":472.775},\"labels\":[{\"name\":\"WG_TEST\",\"distance\":10.26,\"x\":20,\"y\":0,\"z\":0},{\"name\":\"Lake Zurich\",\"distance\":1461.26,\"x\":-278.9903675355017,\"y\":-41.0,\"z\":-1434.4294453682378},{\"name\":\"Zurich Hauptbahnhof\",\"distance\":441.97,\"x\":-423.6082664085552,\"y\":-39.0,\"z\":126.13132119132206},{\"name\":\"HG\",\"distance\":102.88,\"x\":102.8826160794124,\"y\":48.0,\"z\":-0.7712020566686988},{\"name\":\"Grossm\u00FCnster\",\"distance\":733.62,\"x\":-207.2370368060656,\"y\":23.0,\"z\":-703.7734117538203},{\"name\":\"Fraumunster\",\"distance\":848.76,\"x\":-383.7784227631055,\"y\":23.0,\"z\":-757.0725818965584}]}";
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Labels received from server: " + jsonResponse);

                response = JsonConvert.DeserializeObject<Response>(jsonResponse);

                // Start coroutines to spawn buildings
                Debug.Log("Start spawning buildings!");
                instance.StartCoroutine(WorldLoader.GenerateWorld(marker));

                Debug.Log("Spawning " + response.labels.Count + " labels!");
                // Spawn game objects with text at each position
                foreach (Label l in response.labels)
                    SpawnObjectAtPosition(l, marker);
            }
        }
    }

    static void SpawnObjectAtPosition(Label label, GameObject marker)
    {
        Debug.Log("Spawning label " + label.name + " at " + label.x + " " + label.y + " " + label.z);
        GameObject obj = new GameObject(); // Instantiate(instance.prefab);
        obj.transform.parent = instance.labels.transform;
        obj.transform.localPosition = new Vector3(label.x, label.y, label.z);
        obj.name = label.name;

        float scale = Math.Max(Math.Min((float)(1 + 12*label.distance/1000), 50), 7);
        obj.transform.localScale = new Vector3(scale, scale, scale);

        // Add a TextMesh component to display the text
        TextMeshPro textMesh = obj.AddComponent<TextMeshPro>();
        textMesh.text = label.name;
        textMesh.color = Color.gray;   // Set the text color
        textMesh.fontSize = 18;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.material = instance.textMaterial;

        FaceCamera faceCameraScript = obj.AddComponent<FaceCamera>();
        faceCameraScript.cameraToLookAt = Camera.main;
        Debug.Log("Spawned label " + label.name + " at " + obj.transform.position);
    }
}
