/// <summary>
/// This script renders a Voronoi Diagram of the Light Probes in the scene and updates their colors based on the average color of the pixels nearest to them.
/// </summary>
using System.Collections;
using UnityEngine;

public class LightProbeRenderer : MonoBehaviour
{
    // Public variables
    public GameObject lightProbeParent; // Reference to the GameObject that contains all the Light Probes
    private LightProbe[] lightProbes; // Array of Light Probes
    public Camera renderCamera; // Reference to the camera of the Light Probe Renderer
    // public ComputeShader computeShader; // Reference to the compute shader that calculates the Voronoi Diagram

    // Private variables
    private Texture2D cameraTexture; // Texture to store the rendered image of the MainCamera
    private int[] nearestProbeIndices; // Index of the nearest Light Probe for each pixel
    private Color[] averageColors; // Array to calculate the average color values
    private Color[] pixels; // Array to store the pixel values of the camera texture
    private int[] pixelsPerProbe; // Number of pixels assigned to each Light Probe

    void Start()
    {
        // Setup the camera of the Light Probe Renderer
        renderCamera.targetTexture = new RenderTexture(256, 144, 24);
        renderCamera.fieldOfView = 120;

        Debug.Log("Resolution for Light Probe Rendering: " + renderCamera.targetTexture.width + "x" + renderCamera.targetTexture.height + "px");

        // Create the texture to store the rendered image of the MainCamera
        cameraTexture = new Texture2D(renderCamera.pixelWidth, renderCamera.pixelHeight);

        // Create the array of Light Probes and get all Light Probes in the scene
        lightProbes = lightProbeParent.GetComponentsInChildren<LightProbe>();

        Debug.Log("Number of Light Probes found: " + lightProbes.Length);

        // Calculate the Voronoi Diagram once at the beginning
        CalculateVoronoiDiagram();
    }

    void LateUpdate()
    {
        // Start the coroutine to update the Light Probes
        StartCoroutine(UpdateLightProbes());
    }

    /// <summary>
    /// Calculates the Voronoi Diagram of the Light Probes.
    /// </summary>
    void CalculateVoronoiDiagram()
    {
        nearestProbeIndices = new int[cameraTexture.width * cameraTexture.height];
        averageColors = new Color[lightProbes.Length];
        pixelsPerProbe = new int[lightProbes.Length];

        pixels = cameraTexture.GetPixels();

        // Assign each pixel to its nearest light probe.
        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                Vector2 pixelPosition = new Vector2(i, j);
                nearestProbeIndices[j * cameraTexture.width + i] = FindNearestProbeIndex(pixelPosition);
            }
        }

        // Accumulate the color of each pixel in the light probe to which it was assigned.
        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                // calculate the index of the pixel in the array
                int pixelIndex = j * cameraTexture.width + i;

                // get the index of the nearest light probe for the pixel
                int nearestProbeIndex = nearestProbeIndices[pixelIndex];

                // add the color of the pixel to the average color of the light probe
                averageColors[nearestProbeIndex] += cameraTexture.GetPixel(i, j);

                // increment the number of pixels assigned to the light probe
                pixelsPerProbe[nearestProbeIndex]++;
            }
        }

        // Update each light probe with the average color of all pixels assigned to it.
        for (int i = 0; i < lightProbes.Length; i++)
        {
            if (pixelsPerProbe[i] > 0)
            {
                averageColors[i] /= pixelsPerProbe[i];
                lightProbes[i].UpdateProbeColor(averageColors[i]);
            }
        }
    }


    /// <summary>
    /// Updates the Light Probes with the calculated Color values.
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateLightProbes()
    {
        yield return new WaitForEndOfFrame();

        // Get the current render texture
        RenderTexture currentRT = RenderTexture.active;
        // Set the render texture to the one we are capturing to
        RenderTexture.active = renderCamera.targetTexture;

        // Read the pixels from the render texture into the camera texture
        cameraTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        cameraTexture.Apply();

        // Restore the original render texture
        RenderTexture.active = currentRT;

        // Create an array to store the average colors for each probe
        averageColors = new Color[lightProbes.Length];

        // Get the pixels from the camera texture
        pixels = cameraTexture.GetPixels();

        // For each pixel, add its color to the average color for the probe it is nearest to
        for (int i = 0; i < pixels.Length; i++)
        {
            averageColors[nearestProbeIndices[i]] += pixels[i];
        }

        // For each probe, update its color to the average color of the pixels nearest to it
        for (int i = 0; i < lightProbes.Length; i++)
        {
            if (pixelsPerProbe[i] > 0)
            {
                averageColors[i] /= pixelsPerProbe[i];
                lightProbes[i].UpdateProbeColor(averageColors[i]);
            }
        }
    }


    /// <summary>
    /// Finds the index of the nearest Light Probe for a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    int FindNearestProbeIndex(Vector2 position)
    {
        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < lightProbes.Length; i++)
        {
            float distance = Vector2.Distance(position, renderCamera.WorldToScreenPoint(lightProbes[i].GetProbePosition()));
            if (distance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = distance;
            }
        }

        return nearestIndex;
    }
}
