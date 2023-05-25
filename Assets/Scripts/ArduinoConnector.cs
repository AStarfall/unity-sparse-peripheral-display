using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO.Ports;

public class ArduinoConnector : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM5", 250000);
    public Color[] ledColors;
    public int ledCount = 2;

    void Start()
    {
        // Starte die serielle Verbindung auf dem COM-Port 5
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;
        serialPort.Open();

        // Array für die Farben der LEDs
        ledColors = new Color[ledCount];
    }

    // Update is called once per frame
    void Update()
    {
        // // zufällige Farben für die LEDs
        // for (int i = 0; i < ledCount; i++)
        // {
        //     ledColors[i] = new Color(Random.value, Random.value, Random.value);
        // }

        ledColors[0] = new Color(1, 0, 0); // Rot
        ledColors[1] = new Color(0, 1, 0); // Grün

        // Sende die Farben an den Arduino im binären Format
        byte[] data = new byte[ledCount * 3];
        for (int i = 0; i < ledCount; i++)
        {
            // Konvertiere den Farbwert in den RGB-Bereich von 0-255
            int r = Mathf.RoundToInt(ledColors[i].r * 255);
            int g = Mathf.RoundToInt(ledColors[i].g * 255);
            int b = Mathf.RoundToInt(ledColors[i].b * 255);

            // Speichere die RGB-Werte im Datenarray
            data[i * 3] = (byte)r;
            data[i * 3 + 1] = (byte)g;
            data[i * 3 + 2] = (byte)b;
        }

        // Sende die Daten an den Arduino
        serialPort.Write(data, 0, data.Length);


        if (serialPort.IsOpen)
        {
            Debug.Log("Serial Port is open");
            Debug.Log("Sent data to Arduino");
        }
    }

    void OnApplicationQuit()
    {
        // Schließe die serielle Verbindung
        serialPort.Close();
    }
}