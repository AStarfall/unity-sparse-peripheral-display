using System.Collections;
using UnityEngine;
using System.IO.Ports;


/// <summary>
/// This class handles the communication between Unity and an Arduino board to control a set of LEDs.
/// </summary>
public class LedConnector : MonoBehaviour
{
    // Public variables
    public string portName = "COM5"; // name of the serial port
    public int baudRate; // baud rate of the serial connection
    public int ledCount; // number of LEDs connected to Arduino
    public GameObject lightProbeParent; // Reference to the GameObject that contains all the Light Probes

    // Private variables
    private SerialPort serialPort;
    private LightProbe[] lightProbes; // Array of Light Probes

    /// <summary>
    /// Sets up the serial connection and starts sending data to the Arduino.
    /// </summary>
    void Start()
    {
        // Setup serial connection
        serialPort = new SerialPort(portName, baudRate)
        {
            DtrEnable = true,    // necessary for my Arduino Nano every
            RtsEnable = true,    // necessary for my Arduino Nano every
            WriteBufferSize = ledCount * 3 // important to prevent flickering
        };

        // Open serial connection
        Debug.Log("Opening serial port: " + portName + ", Baud rate: " + baudRate);
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

        // Create the array of Light Probes
        lightProbes = lightProbeParent.GetComponentsInChildren<LightProbe>();

        // send data to Arduino
        StartCoroutine(SendData(10));
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Sends data to the Arduino at a specified refresh rate.
    /// </summary>
    /// <param name="refreshRate">The refresh rate in Hz.</param>
    IEnumerator SendData(int refreshRate = 100)
    {
        while (true)
        {
            // Create a byte array for the data to send to Arduino
            byte[] data = new byte[ledCount * 3];

            for (int i = 0; i < ledCount; i++)
            {
                // Get the colour of the Light Probe
                Color probeColor = lightProbes[i].ProbeColor;
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

            // Wait for 1/X seconds (XHz)
            float waitTime = 1f / refreshRate;
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Stops sending data to the Arduino, turns off all LEDs and closes the serial connection when the application is quit.
    /// </summary>
    void OnApplicationQuit()
    {
        // stop sending data to Arduino
        StopCoroutine(SendData());

        // Turn off all LEDs
        TurnLedsOff();

        // Close serial connection
        serialPort.Close();

        // check if serial connection is closed
        if (serialPort.IsOpen)
        {
            Debug.Log("Serial port " + portName + " could not be closed");
        }
        else
        {
            Debug.Log("Serial port " + portName + " closed successfully");
        }
    }

    /// <summary>
    /// Turns off all LEDs by sending a byte array of zeros to the Arduino.
    /// </summary>
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
    }
}