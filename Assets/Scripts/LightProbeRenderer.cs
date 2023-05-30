using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightProbeRenderer : MonoBehaviour
{
    public LightProbe[] lightProbes; // Array der Light Probes

    public Camera mainCamera; // Referenz auf die MainCamera
    public Texture2D cameraTexture; // Textur, um das gerenderte Bild der MainCamera zu speichern

    void Start()
    {
        // Erstelle die Textur, um das gerenderte Bild der MainCamera zu speichern
        cameraTexture = new Texture2D(mainCamera.pixelWidth, mainCamera.pixelHeight);
    }
    void Update()
    {
        CalculateVoronoiDiagram();
    }

    void CalculateVoronoiDiagram()
    {
        // Rendere das Bild der MainCamera in die Textur
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = mainCamera.targetTexture;
        mainCamera.Render();
        cameraTexture.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        cameraTexture.Apply();
        RenderTexture.active = currentRT;

        // Erstelle das Voronoi-Diagramm
        for (int i = 0; i < cameraTexture.width; i++)
        {
            for (int j = 0; j < cameraTexture.height; j++)
            {
                Vector2 pixelPosition = new Vector2(i, j);
                int nearestProbeIndex = FindNearestProbeIndex(pixelPosition);
                lightProbes[nearestProbeIndex].UpdateProbeColor(cameraTexture.GetPixel(i, j));
            }
        }
    }

    int FindNearestProbeIndex(Vector2 position)
    {
        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;

        for (int i = 0; i < lightProbes.Length; i++)
        {
            float distance = Vector2.Distance(position, lightProbes[i].GetProbePosition());
            if (distance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = distance;
            }
        }

        return nearestIndex;
    }
}