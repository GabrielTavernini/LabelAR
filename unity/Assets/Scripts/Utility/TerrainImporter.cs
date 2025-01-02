#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
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
    private int xIndex;
    private int yIndex;
    private int zIndex;
    private int stepSize;


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

    private Vector3 parseLine(string line) {
        string[] splitLine = line.Trim().Split(' ');

        float.TryParse(splitLine[xIndex], out float x);
        float.TryParse(splitLine[yIndex], out float y);
        float.TryParse(splitLine[zIndex], out float z);

        x -= WorldLoader.X_offset;
        z -= WorldLoader.Z_offset;

        return new Vector3(x, y, z);
    }

    private IEnumerator CreateMeshFromXYZ()
    {
        string[] order = lines[0].Trim().Split(' ');
        xIndex = Array.IndexOf(order, "X");
        yIndex = Array.IndexOf(order, "Y");
        zIndex = Array.IndexOf(order, "Z");
        
        stepSize = (int)(parseLine(lines[2]).x - parseLine(lines[1]).x);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
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
            Vector3 vertex = parseLine(line);
            int currentIndex = vertices.Count();
            vertices.Add(vertex);
            if(prevZ != vertex.z) {
                prevX_end = currX_end;
                prevX_start = currX_start;
                currX_start = vertex.x;

                prevI_end = currI_end;
                prevI_start = currI_start;
                currI_start = currentIndex;

                prevZ = vertex.z;
                gridLine++;
            }

            if(vertex.x >= prevX_start && vertex.x <= prevX_end) {
                int downIndex = prevI_start + (int)((vertex.x - prevX_start)/stepSize);

                if(vertex.x != prevX_start && currentIndex != currI_start) {
                    // first node above prev line
                    triangles.Add(currentIndex - 1);
                    triangles.Add(currentIndex);
                    triangles.Add(downIndex);
                }

                if(vertex.x != prevX_end) {
                    // last node above prev line
                    triangles.Add(downIndex + 1);
                    triangles.Add(downIndex);
                    triangles.Add(currentIndex);
                }
            }
            currX_end = vertex.x;
            currI_end = currentIndex;

            if(vertices.Count() % 500 == 0) yield return null;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var savePath = Path.Combine(saveFolderPath, "terrain.asset");
        AssetDatabase.CreateAsset(mesh, savePath);

        Log("Vertices: " + mesh.vertices.Count());
        Log("Grid Lines: " + gridLine);
        Log("Finished creating mesh");
    }
}
#endif