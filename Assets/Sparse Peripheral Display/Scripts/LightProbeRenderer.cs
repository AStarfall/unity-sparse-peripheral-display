using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightProbeRenderer : MonoBehaviour
{
    public GameObject lightProbeParent; // Referenz auf das GameObject, das alle Light Probes enthält
    private LightProbe[] lightProbes; // Array der Light Probes

    // public ComputeShader computeShader; // Referenz auf den ComputeShader

    public Camera renderCamera; // Referenz auf die MainCamera
    private Texture2D cameraTexture; // Textur, um das gerenderte Bild der MainCamera zu speichern
    private int[] nearestProbeIndices; // Zuordnung der Pixel zu den Light Probes
    int[] probeCounts; // Anzahl der Pixel, die einem Light Probe zugeordnet sind

    void Start()
    {
        // Skaliere die RenderTexture der MainCamera auf 1/10 der Größe
        // renderCamera.targetTexture = new RenderTexture(renderCamera.pixelWidth / 10, renderCamera.pixelHeight / 10, 24);
        renderCamera.targetTexture = new RenderTexture(256, 144, 24);
        renderCamera.fieldOfView = 120;

        Debug.Log("Resolution for Light Probe Rendering: " + renderCamera.targetTexture.width + "x" + renderCamera.targetTexture.height + "px");

        // Erstelle die Textur, um das gerenderte Bild der MainCamera zu speichern
        cameraTexture = new Texture2D(renderCamera.pixelWidth, renderCamera.pixelHeight);

        // Erstelle das Array der Light Probes
        lightProbes = lightProbeParent.GetComponentsInChildren<LightProbe>();

        Debug.Log("Number of Light Probes: " + lightProbes.Length);

        // Berechne das Voronoi-Diagramm einmalig
        CalculateVoronoiDiagram();
    }

    void LateUpdate()
    {
        // Aktualisiere die Light Probes basierend auf dem vorberechneten Voronoi-Diagramm
        StartCoroutine(UpdateLightProbes());
    }

    void CalculateVoronoiDiagram()
    {
        // Erstelle das Voronoi-Diagramm und berechne Durchschnittsfarbwerte
        nearestProbeIndices = new int[cameraTexture.width * cameraTexture.height];
        Color[] averageColors = new Color[lightProbes.Length];
        probeCounts = new int[lightProbes.Length];

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

    // IEnumerator UpdateLightProbes()
    // {
    //     yield return new WaitForEndOfFrame();

    //     // Rendere das Bild der MainCamera in die Textur
    //     RenderTexture currentRT = RenderTexture.active;
    //     RenderTexture.active = renderCamera.targetTexture;
    //     cameraTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
    //     cameraTexture.Apply();
    //     RenderTexture.active = currentRT;

    //     // Erstelle ComputeBuffers für nearestProbeIndices und probeCounts
    //     ComputeBuffer nearestProbeIndicesBuffer = new ComputeBuffer(nearestProbeIndices.Length, sizeof(int));
    //     ComputeBuffer probeCountsBuffer = new ComputeBuffer(this.probeCounts.Length, sizeof(int));

    //     // Setze die Daten in die ComputeBuffers
    //     nearestProbeIndicesBuffer.SetData(nearestProbeIndices);
    //     probeCountsBuffer.SetData(probeCounts);

    //     // Erhalte den Index des Kernels im ComputeShader
    //     int kernelIndex = this.computeShader.FindKernel("CalculateAverageColors");

    //     // Setze die ComputeBuffers im ComputeShader
    //     computeShader.SetBuffer(kernelIndex, "nearestProbeIndices", nearestProbeIndicesBuffer);
    //     computeShader.SetBuffer(kernelIndex, "probeCounts", probeCountsBuffer);

    //     // Dispatch den ComputeShader
    //     computeShader.Dispatch(kernelIndex, cameraTexture.width, cameraTexture.height, 1);

    //     // Erhalte die aktualisierten Daten aus den ComputeBuffers
    //     nearestProbeIndicesBuffer.GetData(nearestProbeIndices);
    //     probeCountsBuffer.GetData(probeCounts);

    //     // Gib die ComputeBuffers frei
    //     nearestProbeIndicesBuffer.Release();
    //     probeCountsBuffer.Release();

    //     // Aktualisiere die Light Probes mit den Durchschnittsfarbwerten
    //     Color[] averageColors = new Color[lightProbes.Length];

    //     for (int i = 0; i < cameraTexture.width * cameraTexture.height; i++)
    //     {
    //         int nearestProbeIndex = nearestProbeIndices[i];

    //         averageColors[nearestProbeIndex] += cameraTexture.GetPixel(i % cameraTexture.width, i / cameraTexture.width);
    //     }

    //     for (int i = 0; i < lightProbes.Length; i++)
    //     {
    //         if (probeCounts[i] > 0)
    //         {
    //             averageColors[i] /= probeCounts[i];
    //             lightProbes[i].UpdateProbeColor(averageColors[i]);
    //         }
    //     }
    // }

    IEnumerator UpdateLightProbes()
    {
        yield return new WaitForEndOfFrame();

        // Rendere das Bild der MainCamera in die Textur
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderCamera.targetTexture;
        cameraTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        cameraTexture.Apply();
        RenderTexture.active = currentRT;

        // Weise den Light Probes die Farben basierend auf dem Voronoi-Diagramm zu
        Color[] averageColors = new Color[lightProbes.Length];
        probeCounts = new int[lightProbes.Length];

        // Erhalte die Pixelwerte der Kameratextur
        Color[] pixels = cameraTexture.GetPixels();

        // ---- Berchnung der Durchschnittsfarbwerte in Batches ----
        // int batchSize = 200;
        // int numBatches = Mathf.CeilToInt((float)pixels.Length / batchSize);

        // for (int batchIndex = 0; batchIndex < numBatches; batchIndex++)
        // {
        //     int startIndex = batchIndex * batchSize;
        //     int endIndex = Mathf.Min(startIndex + batchSize, pixels.Length);

        //     for (int i = startIndex; i < endIndex; i++)
        //     {
        //         int nearestProbeIndex = nearestProbeIndices[i];

        //         averageColors[nearestProbeIndex] += pixels[i];
        //         probeCounts[nearestProbeIndex]++;
        //     }
        // }
        // ----


        // ---- Berchnung der Durchschnittsfarbwerte ohne Batches ----
        // Iteriere über die Pixel und aktualisiere die Durchschnittsfarbwerte
        for (int i = 0; i < pixels.Length; i++)
        {
            int nearestProbeIndex = nearestProbeIndices[i];

            averageColors[nearestProbeIndex] += pixels[i];
            probeCounts[nearestProbeIndex]++;
        }
        // ----

        // Debug.Log("Width: " + cameraTexture.width + ", Height: " + cameraTexture.height + ", Total Pixels:" + pixels.Length);

        // Aktualisiere die Light Probes mit den Durchschnittsfarbwerten
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
