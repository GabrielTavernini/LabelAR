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
        List<List<Vector3>> point_grid = new();
        List<int> triangles = new();
        int vertex_count = 0;

        // Read the first line to determine the order of x, y, z
        string[] order = lines[0].Trim().Split(' ');
        int xIndex = Array.IndexOf(order, "x");
        int yIndex = Array.IndexOf(order, "y");
        int zIndex = Array.IndexOf(order, "z");

        foreach (string line in lines.Skip(1))
        {
            string[] splitLine = line.Trim().Split(' ');
            float.TryParse(splitLine[xIndex], out float x);
            float.TryParse(splitLine[yIndex], out float y);
            float.TryParse(splitLine[zIndex], out float z);

            if (point_grid.Count() == 0 || point_grid.Last()[0].z != z)
            {
                point_grid.Add(new List<Vector3>{new Vector3(x, y, z)});
            }
            else
            {
                point_grid.Last().Add(new Vector3(x, y, z));
            }

            if(++vertex_count % 500 == 0) yield return null;
        }
        Log(point_grid.ToString());

        for (int i = 0, currOffset = 0; i < point_grid.Count() - 1; i++)
        {
            var curr = point_grid[i];
            var next = point_grid[i + 1];
            int currJ = 0, nextJ = 0;
            int nextOffset = currOffset + point_grid[i].Count();

            // ... * * ...
            //       * ...
            // connect all start curr to first next
            for (; curr[currJ].x < next[0].x; currJ++)
            {
                int a = currOffset + currJ, b = currOffset + currJ + 1, c = nextOffset;
                triangles.AddRange(new int[] { a, b, c });
            }

            //       * ...
            // ... * * ...
            // connect all start next to first curr
            for (; curr[0].x > next[nextJ].x; nextJ++)
            {
                int a = currOffset, b = nextOffset + nextJ, c = nextOffset + nextJ + 1;
                triangles.AddRange(new int[] { a, b, c });
            }

            // ... * * ...
            // ... * * ...
            // connect if currJ and nextJ both have a next point
            for (; currJ < curr.Count() - 1 & nextJ < next.Count() - 1; currJ++, nextJ++)
            {
                int a1 = currOffset + currJ, b1 = currOffset + currJ + 1, c1 = nextOffset + nextJ;
                triangles.AddRange(new int[] { a1, b1, c1 });

                int a2 = currOffset + currJ + 1, b2 = nextOffset + nextJ, c2 = nextOffset + nextJ + 1;
                triangles.AddRange(new int[] { a2, b2, c2 });

                if(currOffset + currJ % 500 == 0) yield return null;
            }

            //   ... * * ...
            //   ... * 
            // connect all end curr to last next
            for (; currJ < curr.Count() - 1; currJ++)
            {
                int a = currOffset + currJ, b = currOffset + currJ + 1, c = nextOffset + nextJ;
                triangles.AddRange(new int[] { a, b, c });
            }
    
            //   ... * 
            //   ... * * ...
            // connect all end next to last curr
            for (; nextJ < next.Count() - 1; nextJ++)
            {
                int a = currOffset + currJ, b = nextOffset + nextJ, c = nextOffset + nextJ + 1;
                triangles.AddRange(new int[] { a, b, c });
            }

            currOffset = nextOffset;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = point_grid.SelectMany(x => x).ToArray();
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