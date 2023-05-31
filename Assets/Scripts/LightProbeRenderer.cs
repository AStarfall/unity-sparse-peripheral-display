using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightProbeRenderer : MonoBehaviour
{
    public LightProbe[] lightProbes; // Array der Light Probes

    public Camera mainCamera; // Referenz auf die MainCamera
    private Texture2D cameraTexture; // Textur, um das gerenderte Bild der MainCamera zu speichern
    private int[] nearestProbeIndices; // Zuordnung der Pixel zu den Light Probes

    void Start()
    {
        // Erstelle die Textur, um das gerenderte Bild der MainCamera zu speichern
        cameraTexture = new Texture2D(mainCamera.pixelWidth, mainCamera.pixelHeight);

        // Berechne das Voronoi-Diagramm einmalig
        CalculateVoronoiDiagram();
    }

    void CalculateVoronoiDiagram()
    {
        // Erstelle das Voronoi-Diagramm und berechne Durchschnittsfarbwerte
        nearestProbeIndices = new int[cameraTexture.width * cameraTexture.height];
        Color[] averageColors = new Color[lightProbes.Length];
        int[] probeCounts = new int[lightProbes.Length];

        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                Vector2 pixelPosition = new Vector2(i, j);
                nearestProbeIndices[j * cameraTexture.width + i] = FindNearestProbeIndex(pixelPosition);
            }
        }

        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                int pixelIndex = j * cameraTexture.width + i;
                int nearestProbeIndex = nearestProbeIndices[pixelIndex];

                averageColors[nearestProbeIndex] += cameraTexture.GetPixel(i, j);
                probeCounts[nearestProbeIndex]++;
            }
        }

        for (int i = 0; i < lightProbes.Length; i++)
        {
            if (probeCounts[i] > 0)
            {
                averageColors[i] /= probeCounts[i];
                lightProbes[i].UpdateProbeColor(averageColors[i]);
            }
        }
    }

    void LateUpdate()
    {
        // Aktualisiere die Light Probes basierend auf dem vorberechneten Voronoi-Diagramm
        StartCoroutine(UpdateLightProbes());
    }

    IEnumerator UpdateLightProbes()
    {
        yield return new WaitForEndOfFrame();

        // Rendere das Bild der MainCamera in die Textur
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = mainCamera.targetTexture;
        cameraTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        cameraTexture.Apply();
        RenderTexture.active = currentRT;

        // Weise den Light Probes die Farben basierend auf dem Voronoi-Diagramm zu
        Color[] averageColors = new Color[lightProbes.Length];
        int[] probeCounts = new int[lightProbes.Length];

        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                int pixelIndex = j * cameraTexture.width + i;
                int nearestProbeIndex = nearestProbeIndices[pixelIndex];

                averageColors[nearestProbeIndex] += cameraTexture.GetPixel(i, j);
                probeCounts[nearestProbeIndex]++;
            }
        }

        for (int i = 0; i < lightProbes.Length; i++)
        {
            if (probeCounts[i] > 0)
            {
                averageColors[i] /= probeCounts[i];
                lightProbes[i].UpdateProbeColor(averageColors[i]);
            }
        }
    }

    int FindNearestProbeIndex(Vector2 position)
    {
        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < lightProbes.Length; i++)
        {
            float distance = Vector2.Distance(position, mainCamera.WorldToScreenPoint(lightProbes[i].GetProbePosition()));
            if (distance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = distance;
            }
        }

        return nearestIndex;
    }
}
