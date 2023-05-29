using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightProbe : MonoBehaviour
{
    public int probeIndex; // Index der Light Probe (entspricht der LED-Nummer)

    private Color probeColor; // Aktueller Farbwert der Light Probe

    public void UpdateProbeColor(Color color)
    {
        probeColor = color;

        // get Sphere in the Light Probe
        GameObject sphere = transform.GetChild(0).gameObject;

        // set the color of the sphere
        sphere.GetComponent<Renderer>().material.color = probeColor;
    }

    public Color GetProbeColor()
    {
        return probeColor;
    }
}