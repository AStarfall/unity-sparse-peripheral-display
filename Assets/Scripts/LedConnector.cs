using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO.Ports;

public class LedConnector : MonoBehaviour
{
    // Public variables
    public string portName = "COM5"; // name of the serial port
    // public int baudRate = 250000; // baud rate of the serial connection
    public int baudRate; // baud rate of the serial connection
    public int ledCount; // number of LEDs connected to Arduino

    public GameObject lightProbeParent; // Referenz auf das GameObject, das alle Light Probes enthält


    // Private variables
    private SerialPort serialPort;
    // private Color[] ledColors; // array for the colours of the LEDs
    private LightProbe[] lightProbes; // Array der Light Probes

    void Start()
    {
        // Setup serial connection
        serialPort = new SerialPort(portName, baudRate);
        serialPort.DtrEnable = true;    // necessary for my Arduino Nano every
        serialPort.RtsEnable = true;    // necessary for my Arduino Nano every
        serialPort.WriteBufferSize = ledCount * 3; // important to prevent flickering

        // Open serial connection
        serialPort.Open();

        // check if serial connection is open
        if (serialPort.IsOpen)
        {
            Debug.Log("Serial port " + portName + " opened successfully");
        }
        else
        {
            Debug.Log("Could not open serial port " + portName);
        }

        // Erstelle das Array der Light Probes
        lightProbes = lightProbeParent.GetComponentsInChildren<LightProbe>();

        // send data to Arduino
        StartCoroutine(SendData());
    }

    // Update is called once per frame
    void Update()
    {

    }

    // send data to Arudino at 100Hz
    IEnumerator SendData()
    {
        while (true)
        {
            // Create a byte array for the data to send to Arduino
            byte[] data = new byte[ledCount * 3];

            for (int i = 0; i < ledCount; i++)
            {
                // Get the colour of the Light Probe
                Color probeColor = lightProbes[i].probeColor;
                // Convert the colour value to the RGB range 0-255
                int r = Mathf.RoundToInt(probeColor.r * 255);
                int g = Mathf.RoundToInt(probeColor.g * 255);
                int b = Mathf.RoundToInt(probeColor.b * 255);

                // Store the RGB values in the data array
                data[i * 3] = (byte)r;
                data[i * 3 + 1] = (byte)g;
                data[i * 3 + 2] = (byte)b;
            }

            // Send data to Arduino
            serialPort.Write(data, 0, data.Length);

            // Wait for 0.01 seconds (100Hz)
            // yield return new WaitForSeconds(0.01f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnApplicationQuit()
    {
        // stop sending data to Arduino
        StopCoroutine(SendData());

        // Turn off all LEDs
        TurnLedsOff();

        // Close serial connection
        serialPort.Close();
    }

    void TurnLedsOff()
    {
        // Create a byte array for the data to send to Arduino
        byte[] data = new byte[ledCount * 3];

        for (int i = 0; i < ledCount; i++)
        {
            // Store the RGB values in the data array
            data[i * 3] = (byte)0;
            data[i * 3 + 1] = (byte)0;
            data[i * 3 + 2] = (byte)0;
        }

        // Send data to Arduino
        serialPort.Write(data, 0, data.Length);

        // Log Connection closed
        Debug.Log("Serial port " + portName + " closed");
    }
}