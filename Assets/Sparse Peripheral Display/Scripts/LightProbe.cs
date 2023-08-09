using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class representing a light probe in a scene.
/// </summary>
public class LightProbe : MonoBehaviour
{
    /// <summary>
    /// The index of the light probe.
    /// </summary>
    // public int probeIndex;

    /// <summary>
    /// The color of the light probe.
    /// </summary>
    public Color ProbeColor { protected set; get; }

    void Start()
    {

    }

    /// <summary>
    /// Updates the color of the sphere representig the light probe.
    /// </summary>
    /// <param name="color">The new color of the sphere representig the light probe.</param>
    public void UpdateProbeColor(Color color)
    {
        ProbeColor = color;

        // Get the sphere of the light probe
        GameObject sphere = transform.GetChild(0).gameObject;

        // Set the color of the sphere
        sphere.GetComponent<Renderer>().material.color = ProbeColor;
    }

    /// <summary>
    /// Gets the world space position of the light probe.
    /// </summary>
    /// <returns>The world space position of the light probe.</returns>
    public Vector3 GetProbePosition()
    {
        return transform.position;
    }
}