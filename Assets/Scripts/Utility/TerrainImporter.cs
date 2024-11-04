#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using netDxf.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using Mesh = UnityEngine.Mesh;

public class TerrainImporter : EditorWindow
{
    private string importFolderPath = "Assets/StreamingAssets";  // Default source folder
    private string saveFolderPath = "Assets/Resources/Terrain";       // Folder to save Unity assets
    private string[] lines;
    private string log;
    private UnityEngine.Vector2 scrollPosition;


    [MenuItem("Tools/Import and Save Terrains")]
    public static void ShowWindow()
    {
        var window = GetWindow<TerrainImporter>("Terrain Mesh Importer");
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

        Log("Start creating mesh");
        EditorCoroutineUtility.StartCoroutine(CreateMeshFromXYZ(), this);
    }

    private void LoadDocuments()
    {
        try{
            string filePath = Directory.GetFiles(importFolderPath, "*.xyz")[0];
            Log("Loading file " + filePath);
            lines = File.ReadAllLines(filePath);
            Log($"Done reading file! Read {lines.Count()} lines");
        } catch (System.Exception e)
        {
            Log("Error reading .xyz file: " + e.Message);
        }
    }

    private IEnumerator CreateMeshFromXYZ()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        int stepSize = 10;
        int gridLine = -1;
        float prevZ = -1;

        float currX_start = -1;
        float currX_end = -1;
        int currI_start = -1;
        int currI_end = -1;

        float prevX_start = -1;
        float prevX_end = -1;
        int prevI_start = -1;
        int prevI_end = -1;

        foreach (string line in lines.Skip(1))
        {
            string[] splitLine = line.Trim().Split(' ');
            float.TryParse(splitLine[0], out float x);
            float.TryParse(splitLine[1], out float y);
            float.TryParse(splitLine[2], out float z);

            int currentIndex = vertices.Count();
            if(prevZ != z) {
                prevX_end = currX_end;
                prevX_start = currX_start;
                currX_start = x;

                prevI_end = currI_end;
                prevI_start = currI_start;
                currI_start = currentIndex;

                prevZ = z;
                gridLine++;
                // Log($"V{currentIndex}: {currX_start.ToString()} - {currX_end.ToString()}");
            }

            if(x >= prevX_start && x <= prevX_end) {
                // Log($"V{currentIndex}: {prevI_start.ToString()}");
                int downIndex = prevI_start + (currentIndex - currI_start);

                if(x != prevX_start && currentIndex != currI_start) {
                    // first node above prev line
                    triangles.Add(currentIndex - 1);
                    triangles.Add(currentIndex);
                    triangles.Add(downIndex);
                    // Log($"A: {currentIndex} - {currentIndex-1} - {downIndex}");
                }

                if(x != prevX_end) {
                    // last node above prev line
                    triangles.Add(downIndex + 1);
                    triangles.Add(downIndex);
                    triangles.Add(currentIndex);
                    // Log($"B: {currentIndex} - {downIndex} - {downIndex+1}");
                }
            }
            currX_end = x;
            currI_end = currentIndex;
            vertices.Add(new Vector3(x, y, z));

            if(vertices.Count() % 500 == 0) yield return null;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var savePath = Path.Combine(saveFolderPath, "test.asset");
        AssetDatabase.CreateAsset(mesh, savePath);

        Log("Vertices: " + mesh.vertices.Count());
        Log("Finished creating mesh");
    }

    // var savePath = Path.Combine(saveFolderPath, polyfaceMesh.Handle + ".asset");
    // AssetDatabase.CreateAsset(mesh, savePath);
    // return mesh;

}
#endif