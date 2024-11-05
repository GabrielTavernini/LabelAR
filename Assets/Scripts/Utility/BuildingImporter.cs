#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using netDxf;
using System;
using netDxf.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class BuildingImporter : EditorWindow
{
    private string importFolderPath = "Assets/StreamingAssets";  // Default source folder
    private string saveFolderPath = "Assets/Resources/Buildings";       // Folder to save Unity assets
    private List<DxfDocument> documents = new();
    private string log;
    private UnityEngine.Vector2 scrollPosition;


    [MenuItem("Tools/Import and Save Buildings")]
    public static void ShowWindow()
    {
        var window = GetWindow<BuildingImporter>("Buildings Mesh Importer");
        window.minSize = new UnityEngine.Vector2(600, 400);
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Import Settings", EditorStyles.boldLabel);

        importFolderPath = EditorGUILayout.TextField("Import Folder Path", importFolderPath);
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

        if (GUILayout.Button("Import and Save Meshes"))
            EditorCoroutineUtility.StartCoroutine(ImportAndSaveMeshes(), this);

        // Log output area
        GUILayout.Label("Log:");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        var logStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
        EditorGUILayout.LabelField(log, logStyle);
        EditorGUILayout.EndScrollView();
    }

    private void Log(string msg)
    {
        log += msg + "\n";
    }

    private IEnumerator ImportAndSaveMeshes()
    {
        Task t = Task.Run(() => LoadDocuments());
        while (!t.IsCompleted)
            yield return null;

        foreach (DxfDocument d in documents)
            EditorCoroutineUtility.StartCoroutine(GenerateBuildings(d), this);
    }

    private void LoadDocuments()
    {
        string[] filePaths = Directory.GetFiles(importFolderPath, "*.dxf");
        foreach (string filePath in filePaths)
        {
            try
            {
                Log("Loading " + filePath);
                documents.Add(DxfDocument.Load(filePath));
                Log("Finished loading " + filePath);
            }
            catch (Exception e)
            {
                Log("Error loading " + filePath);
                Log(e.Message);
            }
        }
    }

    protected IEnumerator GenerateBuildings(DxfDocument doc)
    {
        Log("Loading " + doc.Entities.PolyfaceMeshes.Count() + " meshes.");
        IEnumerator<PolyfaceMesh> e = doc.Entities.PolyfaceMeshes.GetEnumerator();
        int count = 0;
        while (e.MoveNext()) {
            SpawnPolyfaceMesh(e.Current);
            if(count++ % 20 == 0) yield return null;
        }
        Log("Done loading " + doc.Entities.PolyfaceMeshes.Count() + " meshes.");
    }

    UnityEngine.Mesh SpawnPolyfaceMesh(PolyfaceMesh polyfaceMesh)
    {
        UnityEngine.Mesh mesh = new UnityEngine.Mesh();

        // Extract vertices from the PolyfaceMesh
        List<UnityEngine.Vector3> vertices = new List<UnityEngine.Vector3>();
        foreach (var vertex in polyfaceMesh.Vertexes)
        {
            vertices.Add(new UnityEngine.Vector3(
                (float)vertex.X - WorldLoader.X_offset, 
                (float)vertex.Z, 
                (float)vertex.Y - WorldLoader.Z_offset
            ));
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

        var savePath = Path.Combine(saveFolderPath, polyfaceMesh.Handle + ".asset");
        AssetDatabase.CreateAsset(mesh, savePath);
        return mesh;
    }

}
#endif