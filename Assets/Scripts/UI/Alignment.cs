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

    private List<Label> labels = new();
    private List<double> orientations = new();
    private int step = 0;

    void Start()
    {
        upButton.onClick.AddListener(upClick);
        downButton.onClick.AddListener(downClick);
        nextButton.onClick.AddListener(nextClick);

        labels.Add(Request.response.labels[1]);
        labels.Add(Request.response.labels[2]);
        labels.Add(Request.response.labels[4]);
        Debug.Log($"Triangulation Labels: {labels[0].name}, {labels[1].name}, {labels[2].name}");

        init();
    }

    public void init() {
        step = 0;
        highlightLabel(labels[step]);
        nextButton.gameObject.SetActive(true);
    }

    void upClick() {
        orchestrator.marker.transform.position += new Vector3(0, 0.1f, 0);
    }

    void downClick() {
        orchestrator.marker.transform.position -= new Vector3(0, 0.1f, 0);
    }
    
    double getAngle(string label) {
        GameObject l = GameObject.Find(label);
        Vector2 to = new Vector2(l.transform.position.x, l.transform.position.z);
        return Math.Atan2(to.y, to.x);
    }

    void highlightLabel(Label label) {
        foreach(string meshId in label.buildings)
            if(GameObject.Find(meshId) != null)
                GameObject.Find(meshId).GetComponent<MeshRenderer>().material =  orchestrator.editMaterial;
    }

    void restoreLabel(Label label) {
        foreach(string meshId in label.buildings)
            if(GameObject.Find(meshId) != null)
                GameObject.Find(meshId).GetComponent<MeshRenderer>().material =  orchestrator.highlightMaterial;
    }

    public void restoreLabels() {
        foreach(Label label in labels) 
            foreach(string meshId in label.buildings)
                if(GameObject.Find(meshId) != null)
                    GameObject.Find(meshId).GetComponent<MeshRenderer>().material =  orchestrator.highlightMaterial;
    }

    void nextClick() {
        if(step >= 3) return;

        restoreLabel(labels[step]);
        orientations.Add(getAngle(labels[step].name));
        Debug.Log(labels[step].name + " Pos: " + GameObject.Find(labels[step].name).transform.position);
        Debug.Log(labels[step].name + " Ang: " + getAngle(labels[step].name));

        step++;
        
        if(step < 3) {
            highlightLabel(labels[step]);
        } else {
            nextButton.gameObject.SetActive(false);
            triangulate();
        }
    }

    void triangulate() {
        Vector3[] points = new Vector3[3];
        double[] angles = new double[3];

        for(int i = 0; i < 3; i++) {
            points[i] =  GameObject.Find(labels[i].name).transform.position;
            angles[i] = orientations[i];
        }
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
