using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using netDxf;
using System;
using netDxf.Entities;
using System.Linq;

public class BuildingImporter : EditorWindow
{
  private string importFolderPath = "Assets/StreamingAssets";  // Default source folder
  private string saveFolderPath = "Assets/Resources/Buildings";       // Folder to save Unity assets

  [MenuItem("Tools/Import and Save Buildings")]
  public static void ShowWindow()
  {
    GetWindow<BuildingImporter>("Mesh Importer");
  }

  private void OnGUI()
  {
    GUILayout.Label("Mesh Import Settings", EditorStyles.boldLabel);

    importFolderPath = EditorGUILayout.TextField("Import Folder Path", importFolderPath);
    saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);

    if (GUILayout.Button("Import and Save Meshes"))
      ImportAndSaveMeshes();
  }

  private void ImportAndSaveMeshes()
  {
    List<DxfDocument> docs = LoadDocuments();
    foreach (DxfDocument d in docs)
      GenerateBuildings(d);

    // Refresh the AssetDatabase to make saved assets available
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Debug.Log("Mesh import and save completed!");
  }

  private List<DxfDocument> LoadDocuments()
  {
    List<DxfDocument> documents = new();
    string[] filePaths = Directory.GetFiles(importFolderPath, "*.dxf");

    foreach (string filePath in filePaths)
    {
      try
      {
        documents.Add(DxfDocument.Load(filePath));
        Debug.Log("Finished loading " + filePath);
      }
      catch (Exception e)
      {
        Debug.Log("Error loading " + filePath);
        Debug.Log(e.StackTrace);
      }
    }
    return documents;
  }

  protected void GenerateBuildings(DxfDocument doc)
  {
    Debug.Log("Loading " + doc.Entities.PolyfaceMeshes.Count() + " meshes.");
    IEnumerator<PolyfaceMesh> e = doc.Entities.PolyfaceMeshes.GetEnumerator();
    while (e.MoveNext())
      SpawnPolyfaceMesh(e.Current);
    Debug.Log("Done loading");
  }

  void SpawnPolyfaceMesh(PolyfaceMesh polyfaceMesh)
  {
    UnityEngine.Mesh mesh = new UnityEngine.Mesh();

    // Extract vertices from the PolyfaceMesh
    List<UnityEngine.Vector3> vertices = new List<UnityEngine.Vector3>();
    foreach (var vertex in polyfaceMesh.Vertexes)
    {
      vertices.Add(new UnityEngine.Vector3((float)vertex.X, (float)vertex.Z, (float)vertex.Y));
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
  }

}
