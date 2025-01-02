using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicLeap.OpenXR.Features.LocalizationMaps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using System.Text.RegularExpressions;
using UnityEngine.SearchService;
using UnityEngine.Timeline;

public class Alignment : MonoBehaviour
{
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Orchestrator orchestrator;
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text title;

    private List<string> meshIds = new();
    private List<double> orientations = new();
    Vector3[] points = new Vector3[3];
    double[] angles = new double[3];
    private int step = 0;
    private readonly string alignmentSphereName = "AlignmentSphere";

    void Start()
    {
        upButton.onClick.AddListener(upClick);
        downButton.onClick.AddListener(downClick);
        nextButton.onClick.AddListener(nextClick);

        meshIds = new(Request.response.alignment_labels); 
        while (meshIds.Count() < 3)
            meshIds.Add("");
        Debug.Log($"Triangulation Meshes: {meshIds[0]}, {meshIds[1]}, {meshIds[2]}");
        init();
    }

    public void init() {
        if(meshIds.Count() > 0) {
            step = 0;
            highlightLabel(meshIds[step]);
            nextButton.gameObject.SetActive(true);
        } else {
            text.text = "Waiting for the meshes to finish loading...";
            title.text = "Loading Meshes";
        }
    }

    void upClick() {
        orchestrator.marker.transform.position += new Vector3(0, 0.1f, 0);
    }

    void downClick() {
        orchestrator.marker.transform.position -= new Vector3(0, 0.1f, 0);
    }

    Vector3 getClosestVertex(GameObject obj) {
        Vector3 closestVertex = Vector3.zero;
        float minDistance = Mathf.Infinity;
        foreach(Vector3 vec in obj.GetComponent<MeshFilter>().mesh.vertices) {
            if(vec.y < obj.GetComponent<MeshFilter>().mesh.bounds.center.y) continue;
            Vector3 worldVertex = obj.transform.TransformPoint(vec);
            float distance = Vector3.Distance(worldVertex, Vector3.zero);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestVertex = worldVertex;
            }
        }
        return closestVertex;
    }
    
    double getAngle(string meshId) {
        return getAngle(GameObject.Find(meshId).transform.position);
    }

    double getAngle(Vector3 position) {
        Vector2 to = new Vector2(position.x, position.z);
        return Math.Atan2(to.y, to.x);
    }

    void highlightLabel(string meshId) {
        if(GameObject.Find(meshId) != null) {            
            text.text = "Look around to find the building higlighted in <color=#00FF00><b>green</b></color>. Align the <b>white sphere</b> with the corresponding corner using the <color=#775BDD><b>bumper</b></color> to rotate.\n\n";
            title.text = "Align Building " + (step+1);

            Vector3 closestVertex = getClosestVertex(GameObject.Find(meshIds[step]));
            float scale = 8*(closestVertex.magnitude/1000);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = orchestrator.marker.transform;
            sphere.transform.localScale = new Vector3(scale, scale, scale);
            sphere.transform.position = closestVertex;
            sphere.name = alignmentSphereName;
            GameObject.Find(meshId).GetComponent<MeshRenderer>().material =  orchestrator.editMaterial;
        }
    }

    void restoreLabel(string meshId) {
        if(GameObject.Find(alignmentSphereName) != null)
            Destroy(GameObject.Find(alignmentSphereName));

        if(GameObject.Find(meshId) != null) 
            GameObject.Find(meshId).GetComponent<MeshRenderer>().material =  orchestrator.highlightMaterial;
    }

    public void restoreLabels() {
        foreach(string meshId in meshIds)
            restoreLabel(meshId);
    }

    void nextClick() {
        if(step >= 3 || meshIds.Count() == 0) return;

        restoreLabel(meshIds[step]);
        if(GameObject.Find(meshIds[step]) != null) {
            Vector3 closestVertex = GameObject.Find(alignmentSphereName).transform.position;
            points[step] = closestVertex;
            angles[step] = getAngle(closestVertex);
            // orientations.Add(getAngle(meshIds[step]));
            Debug.Log(meshIds[step] + " Pos: " + GameObject.Find(meshIds[step]).transform.position);
            Debug.Log(meshIds[step] + " Ang: " + getAngle(meshIds[step]));
        } else {
            orientations.Add(0);
            Debug.Log("GameObject doesn't exist");
        }

        step++;
        
        if(step < 3) {
            highlightLabel(meshIds[step]);
        } else {
            text.text = "Press <color=#775BDD><b>up</b></color> or <color=#775BDD><b>down</b></color> to correct the world height and use the controller's <color=#775BDD><b>bumper</b></color> button to fine-tune the orientation.\n\nOptionally grab and move the cube.";
            title.text = "Fine-tune alignment";
            nextButton.gameObject.SetActive(false);
            triangulate();
        }
    }

    void triangulate() {
        // Vector3[] points = new Vector3[3];
        // double[] angles = new double[3];

        // for(int i = 0; i < 3; i++) {
        //     points[i] =  GameObject.Find(meshIds[i]) != null ? GameObject.Find(meshIds[i]).transform.position : new Vector3();
        //     angles[i] = orientations[i];
        // }
        Vector2 newPos = FindPosition(points, angles);
        Debug.Log("Optimized Pos: " + newPos);
        Debug.Log("Error: " + CalculateTotalError(points, angles, newPos.x, newPos.y));
        orchestrator.marker.transform.position += new Vector3(newPos.x, 0, newPos.y);
    }

    Vector2 FindPosition(Vector3[] points, double[] radians)
    {


        (double A, double B, double C)[] lines = new (double A, double B, double C)[3];
        Vector3[] intersections = new Vector3[3];

        //Get equation of lines passing by the alignment points
        for (int i=0; i < 3; i++){
            lines[i] = GetLineEquation((points[i].x, points[i].z), radians[i]);
        }
        //Get intersections between lines
        for (int i=0; i < 3; i++){
            intersections[i] = FindIntersection(lines[i], lines[(i+1)%3]);
        }
        //Find barycenter of intersection points
        return FindBarycenter(intersections[0], intersections[1], intersections[2]);
    }

    (double A, double B, double C) GetLineEquation((double x, double y) point, double radians)
    {
        double dx = Math.Cos(radians);
        double dy = Math.Sin(radians);

        double A = -dy;
        double B = dx;
        double C = -(A * point.x + B * point.y);

        return (A, B, C);
    }
    Vector2 FindIntersection((double A, double B, double C) line1, (double A, double B, double C) line2){
        double det = line1.A * line2.B - line2.A * line1.B;
        if (Math.Abs(det) < 1e-9)
        {
            return Vector2.negativeInfinity;
        }
        double x = (line2.B * -line1.C - line1.B * -line2.C) / det;
        double z = (line1.A * -line2.C - line2.A * -line1.C) / det;
        return new Vector2((float)x, (float)z);
    }

    Vector2 FindBarycenter(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        return (v1 + v2 + v3) / 3.0f;
    }

    double CalculateTotalError(Vector3[] points, double[] angles, double x, double z)
    {
        double error = 0;

        for (int i = 0; i < points.Length; i++)
        {
            // Calculate expected angle to point (x, z)
            double dx = points[i].x - x;
            double dz = points[i].z - z;
            double expectedAngle = Math.Atan2(dz, dx);

            // Wrap angles to [-π, π]
            double angleDifference = WrapAngle(expectedAngle - angles[i]);

            // Accumulate squared error
            error += angleDifference * angleDifference;
        }

        return error;
    }

    double WrapAngle(double angle)
    {
        while (angle > Math.PI) angle -= 2 * Math.PI;
        while (angle < -Math.PI) angle += 2 * Math.PI;
        return angle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
