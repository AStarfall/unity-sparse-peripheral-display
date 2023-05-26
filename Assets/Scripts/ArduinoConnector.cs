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
        // Start serial connection
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;
        serialPort.Open();

        // Array for the colours of the LEDs
        ledColors = new Color[ledCount];
    }

    // Update is called once per frame
    void Update()
    {
        ledColors[0] = new Color(1, 0, 0); // red
        ledColors[1] = new Color(0, 1, 0); // green


        byte[] data = new byte[ledCount * 3];
        for (int i = 0; i < ledCount; i++)
        {
            // Convert the colour value to the RGB range 0-255
            int r = Mathf.RoundToInt(ledColors[i].r * 255);
            int g = Mathf.RoundToInt(ledColors[i].g * 255);
            int b = Mathf.RoundToInt(ledColors[i].b * 255);

            // Store the RGB values in the data array
            data[i * 3] = (byte)r;
            data[i * 3 + 1] = (byte)g;
            data[i * 3 + 2] = (byte)b;
        }

        // Send data to Arduino
        serialPort.Write(data, 0, data.Length);

        if (serialPort.IsOpen)
        {
            Debug.Log("Serial Port is open");
            Debug.Log("Data sent to Arduino");
        }
    }

    void OnApplicationQuit()
    {
        // Close serial connection
        serialPort.Close();
    }
}