using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using netDxf;
using netDxf.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class BuildingLoader : MonoBehaviour
{

    [SerializeField]
    private Material material;

    [SerializeField]
    private Material highlightMaterial;

    [SerializeField]
    private List<String> documentNames;

    private List<DxfDocument> documents = new();

    protected Coordinates markerCoordinates;

    protected GameObject marker;

    protected static BuildingLoader instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartCoroutine(LoadDocuments());
    }

    private IEnumerator LoadDocuments()
    {
        foreach (string fileName in documentNames)
        {
            Debug.Log("Loading doc " + fileName);
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#if !UNITY_ANDROID || UNITY_EDITOR
            // For other platforms, just use a file path
            filePath = "file://" + filePath;
#endif

            Debug.Log(filePath);
            using (UnityWebRequest uwr = UnityWebRequest.Get(filePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.error != null)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    MemoryStream ms = new MemoryStream(uwr.downloadHandler.data);
                    Task loadTask = Task.Run(() => {
                        try {
                            documents.Add(DxfDocument.Load(ms));
                            Debug.Log("Finished loading " + fileName);
                        } catch(Exception e) {
                            Debug.Log("Error loading " + fileName);
                            Debug.Log(e.StackTrace);
                        }
                    });
                    
                    //StartCoroutine(GenerateBuildings(loadTask.Result));
                }
            }
        }
    }

    public static void GenerateBuildings(Coordinates markerCoordinates, GameObject marker) {
        Debug.Log("Marker swiss coords: " + markerCoordinates);
        instance.marker = marker;
        instance.markerCoordinates = markerCoordinates;
        foreach (DxfDocument doc in instance.documents) {
            instance.StartCoroutine(instance.GenerateBuildings(doc));
        }
    }

    protected IEnumerator GenerateBuildings(DxfDocument doc)
    {
        Debug.Log("Loading " + doc.Entities.PolyfaceMeshes.Count() + " meshes.");
        IEnumerator<PolyfaceMesh> e = doc.Entities.PolyfaceMeshes.GetEnumerator();
        while (e.MoveNext())
        {
            yield return SpawnPolyfaceMesh(e.Current);
        }
        Debug.Log("Done loading");
    }

    private UnityEngine.Vector3 ConvertCoordinates(netDxf.Vector3 vec)
    {
        float x = (float)(vec.X - this.markerCoordinates.east);
        float y = (float)(vec.Z - this.markerCoordinates.altitude);
        float z = (float)(vec.Y - this.markerCoordinates.north);
        return new UnityEngine.Vector3(x, y, z);
    }

    GameObject SpawnPolyfaceMesh(PolyfaceMesh polyfaceMesh)
    {
        // No clue why some ids are duplicate (they do not fall on the line between files)
        // Material mat = material;
        // if(GameObject.Find(polyfaceMesh.Handle) != null) {
        //     Debug.Log(polyfaceMesh.Handle);
        //     mat = highlightMaterial;
        // }

        // Create a new GameObject for the mesh
        GameObject polyfaceMeshObj = new GameObject(polyfaceMesh.Handle);
        polyfaceMeshObj.transform.parent = marker.transform;

        // Add MeshFilter and MeshRenderer components
        MeshFilter meshFilter = polyfaceMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = polyfaceMeshObj.AddComponent<MeshRenderer>();

        // Create a new Unity Mesh
        UnityEngine.Mesh mesh = new UnityEngine.Mesh();

        // Extract vertices from the PolyfaceMesh
        List<UnityEngine.Vector3> vertices = new List<UnityEngine.Vector3>();
        foreach (var vertex in polyfaceMesh.Vertexes)
        {
            vertices.Add(ConvertCoordinates(vertex));
        }

        // Extract faces (as triangles or quads) from the PolyfaceMesh
        List<int> triangles = new List<int>();
        foreach (var face in polyfaceMesh.Faces)
        {
            if (face.VertexIndexes.Count() == 3)
            {
                // If the face is a triangle
                triangles.Add(face.VertexIndexes[2] - 1);  // DXF is 1-indexed, Unity uses 0-indexing
                triangles.Add(face.VertexIndexes[1] - 1);
                triangles.Add(face.VertexIndexes[0] - 1);
            }
            else if (face.VertexIndexes.Count() == 4)
            {
                // If the face is a quad, split it into two triangles
                triangles.Add(face.VertexIndexes[2] - 1);
                triangles.Add(face.VertexIndexes[1] - 1);
                triangles.Add(face.VertexIndexes[0] - 1);

                triangles.Add(face.VertexIndexes[3] - 1);
                triangles.Add(face.VertexIndexes[2] - 1);
                triangles.Add(face.VertexIndexes[0] - 1);
            }
        }

        // Assign vertices and triangles to the Unity mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Optionally, calculate normals for proper lighting
        mesh.RecalculateNormals();

        // Assign the mesh to the MeshFilter
        meshFilter.mesh = mesh;

        // Assign the material to the MeshRenderer
        meshRenderer.material = material;
        if (polyfaceMesh.Handle == "5BEA1")
            meshRenderer.material = highlightMaterial;

        //polyfaceMeshObj.transform.localScale = new UnityEngine.Vector3(0.01f, 0.01f, 0.01f);
        return polyfaceMeshObj;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
