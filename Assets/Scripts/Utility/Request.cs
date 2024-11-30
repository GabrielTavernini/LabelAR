using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
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
    public List<string> buildings;
}

public class Response
{
    public Coordinates coordinates;
    public List<Label> labels;
    public HashSet<string> buildings;
    public float visibility;
}

public class AddLabelPayload
{
    public string name;
    public float north;
    public float east;
    public float height;
    public List<string> buildings;
}

public class EditLabelPayload {
    public string oldName;
    public string newName;
}

public class DeleteLabelPayload {
    public string name;
}

public class Request
{
    public static Response response;
    private static readonly string baseUrl = "labelar.ilbrigante.me";

    private Request() { }

    public static IEnumerator Load(string mapName)
    {
        string url = $"{baseUrl}/get_labels?mapName={mapName}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
                response = null;
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Labels received from server: " + jsonResponse);
                response = JsonConvert.DeserializeObject<Response>(jsonResponse);
            }
        }
    }



    public static IEnumerator AddLabel(AddLabelPayload payload)
    {
        string url = $"{baseUrl}/add_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
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
                Debug.Log("Add label response: " + jsonResponse);
            }
        }
    }

    public static IEnumerator EditLabel(EditLabelPayload payload)
    {
        Debug.Log("Edit: " + payload.oldName);
        string url = $"{baseUrl}/edit_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
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
                Debug.Log("Edit label response: " + jsonResponse);
            }
        }
    }

    public static IEnumerator DeleteLabel(DeleteLabelPayload payload)
    {
        Debug.Log("Delete: " + payload.name);
        string url = $"{baseUrl}/delete_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
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
                Debug.Log("Delete label response: " + jsonResponse);
            }
        }
    }
}