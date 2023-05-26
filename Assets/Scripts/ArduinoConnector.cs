using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO.Ports;

public class ArduinoConnector : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM5", 250000); //only works with coroutine
    // SerialPort serialPort = new SerialPort("COM5", 19200);   //working
    // SerialPort serialPort = new SerialPort("COM5", 23000);   //working

    public Color[] ledColors; // array for the colours of the LEDs
    public int ledCount = 2; // number of LEDs connected to Arduino

    void Start()
    {
        // Start serial connection
        serialPort.DtrEnable = true;    // necessary for my Arduino Nano every
        serialPort.RtsEnable = true;    // necessary for my Arduino Nano every
        serialPort.WriteBufferSize = ledCount * 3; // set the buffer size to the number of LEDs * 3, Don't know if this is necessary
        serialPort.Open();

        // Array for the colours of the LEDs
        ledColors = new Color[ledCount];

        ledColors[0] = new Color(1, 0, 0); // red
        ledColors[1] = new Color(0, 1, 0); // green

        // send data to Arduino
        StartCoroutine(SendData());
    }

    // Update is called once per frame
    void Update()
    {
        // Change the colour of the LEDs
        for (int i = 0; i < ledCount; i++)
        {
            Color temp = ledColors[i];
            ledColors[i].r = temp.b;
            ledColors[i].g = temp.r;
            ledColors[i].b = temp.g;

            // Debug.Log("LED " + i + " is now " + ledColors[i]);
        }
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

            // Wait for 0.01 seconds (100Hz)
            yield return new WaitForSeconds(0.01f);
        }
    }

    void OnApplicationQuit()
    {
        // Close serial connection
        serialPort.Close();
    }
}