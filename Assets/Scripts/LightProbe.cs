using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightProbe : MonoBehaviour
{
    public int probeIndex; // Index der Light Probe (entspricht der LED-Nummer)

    public Color probeColor { protected set; get; } // Aktueller Farbwert der Light Probe

    void Start()
    {

    }

    public void UpdateProbeColor(Color color)
    {
        probeColor = color;

        // Hole die Sphere des Light Probe
        GameObject sphere = transform.GetChild(0).gameObject;

        // Setze die Farbe der Sphere
        sphere.GetComponent<Renderer>().material.color = probeColor;
    }


    public Vector3 GetProbePosition()
    {
        return transform.position;
    }

    public Vector3 GetProbeScreenPosition(Camera camera)
    {
        // return ProbePostion in relation to the camera
        return camera.WorldToScreenPoint(transform.position);
    }
}