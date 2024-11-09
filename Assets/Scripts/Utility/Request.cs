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
}

public class Response
{
    public Coordinates coordinates;
    public List<Label> labels;
    public HashSet<string> buildings;
    public float visibility;
}

public class Request {
  public static Response response;
  private static readonly string baseUrl = "labelar.ilbrigante.me/get_labels?mapName=";
  
  private Request() {}

  public static IEnumerator Load(string mapName) {
    string url = baseUrl + mapName;
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
            response = JsonConvert.DeserializeObject<Response>(jsonResponse);
        }
    }
  }
}