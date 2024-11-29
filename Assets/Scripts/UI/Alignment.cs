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
    private List<Quaternion> orientations = new();
    private int step = 0;

    void Start()
    {
        upButton.onClick.AddListener(upClick);
        downButton.onClick.AddListener(downClick);
        nextButton.onClick.AddListener(nextClick);

        labels.Add(Request.response.labels[1]);
        labels.Add(Request.response.labels[2]);
        labels.Add(Request.response.labels[3]);
        
        GameObject.Find(labels[step].name).GetComponent<TextMeshPro>().color = Color.red;
    }

    void upClick() {
        orchestrator.marker.transform.position += new Vector3(0, 0.5f, 0);
    }

    void downClick() {
        orchestrator.marker.transform.position -= new Vector3(0, 0.5f, 0);
    }
    
    double getAngle(string label) {
        GameObject l = GameObject.Find(label);
        Vector2 to = new Vector2(l.transform.position.x, l.transform.position.z);
        return Math.Atan2(to.y, to.x);
    }

    void nextClick() {
        if(step >= 3) return;

        GameObject.Find(labels[step].name).GetComponent<TextMeshPro>().color = Color.grey;
        orientations.Add(orchestrator.marker.transform.rotation);
        Debug.Log(labels[step].name + " Pos: " + GameObject.Find(labels[step].name).transform.position);
        Debug.Log(labels[step].name + " Ang: " + getAngle(labels[step].name));

        step++;
        
        if(step < 3)
            GameObject.Find(labels[step].name).GetComponent<TextMeshPro>().color = Color.red;
        else
            triangulate();
    }

    void triangulate() {
        Vector3[] points = new Vector3[3];
        double[] angles = new double[3];

        for(int i = 0; i < 3; i++) {
            points[i] =  GameObject.Find(labels[i].name).transform.position;
            angles[i] = getAngle(labels[i].name);
        }
        Vector3 newPos = FindPosition(points, angles);
        Debug.Log("Optimized Pos: " + newPos);
        Debug.Log("Error: " + CalculateTotalError(points, angles, newPos.x, newPos.z));
    }

    Vector3 FindPosition(Vector3[] points, double[] radians)
    {


        (double A, double B, double C)[] lines = new (double A, double B, double C)[3];
        Vector3[] intersections = new Vector3[3];

        //Get equation of lines passing by the alignment points
        for (int i=0; i < 3; i++){
            lines[i] = GetLineEquation((points[i].x, points[i].z), radians[i]);
        }
        //Get intersections between lines
        for (int i=0; i < 3; i++){
            intersections[i] = FindIntersection(lines[i], lines[(i+1)%3], orchestrator.marker.transform.position.y);
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
    Vector3 FindIntersection((double A, double B, double C) line1, (double A, double B, double C) line2, double height){
        double det = line1.A * line2.B - line2.A * line1.B;
        if (Math.Abs(det) < 1e-9)
        {
            return Vector3.negativeInfinity;
        }
        double x = (line2.B * -line1.C - line1.B * -line2.C) / det;
        double z = (line1.A * -line2.C - line2.A * -line1.C) / det;
        return new Vector3((float)x, (float)height, (float)z);
    }

    Vector3 FindBarycenter(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return (v1 + v2 + v3) / 3.0f;
    }

    // TODO: THIS DOESN'T CONVERGE TO THE CORRECT POINT, IMPLEMENT AN ACTUAL OPTIMIZATION ALGO
    Vector3 OptimizePosition(Vector3[] points, double[] angles, double startX, double startZ)
    {
        const double tolerance = 10e-9;
        const double stepSize = 0.1;
        double currentX = startX, currentZ = startZ;
        double error = CalculateTotalError(points, angles, currentX, currentZ);

        while (true)
        {
            // Gradient descent: calculate partial derivatives
            double gradX = (CalculateTotalError(points, angles, currentX + stepSize, currentZ) - error) / stepSize;
            double gradZ = (CalculateTotalError(points, angles, currentX, currentZ + stepSize) - error) / stepSize;

            // Update position
            currentX -= stepSize * gradX;
            currentZ -= stepSize * gradZ;

            // Recalculate error
            double newError = CalculateTotalError(points, angles, currentX, currentZ);

            // Check for convergence
            if (Math.Abs(newError - error) < tolerance)
                break;

            error = newError;
        }

        return new Vector3((float)currentX, orchestrator.marker.transform.position.y, (float)currentZ);
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
