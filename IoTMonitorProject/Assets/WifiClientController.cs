using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class WifiClientController : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Set this to your server's IP address.
    public int serverPort = 31008;             // Set this to your server's port.
    private string firstMessageToSend = "Hello Server!"; // The message to send.

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    [SerializeField] private Text _clientMessage;
    [SerializeField] private Text _serverIP;
    [SerializeField] private Text _lastConnectedText;
    [SerializeField] private InputField _lastConnectedInput;
    Thread thread;
    [SerializeField] private string[] oldServers;
    [SerializeField] private GameObject _lastConnectedPanel;


    void Start()
    {
        // ConnectToServer();

        var lastSaved = PlayerPrefs.GetString("lastSaved");
        _lastConnectedPanel.SetActive(false);
        if (lastSaved != "")
        {
            SetLCPanel(lastSaved);
        }
        
    }

    void Update()
    {
        //disable this if you are sending from another script or a button
        if (Input.GetKeyDown(KeyCode.V))
        {
            SendMessageToServer(firstMessageToSend);
        }
    }


    #region PlayerPrefs

    public void SetString(string KeyName, string Value)
    {
        PlayerPrefs.SetString(KeyName, Value);
    }

    public void GetString(string key)
    {
        PlayerPrefs.GetString(key);
    }

    private void SetLCPanel(string lastSaved)
    {
        _lastConnectedPanel.SetActive(true);
        _lastConnectedText.text = lastSaved;
    }

    public void SetLCText()
    {
        //_serverIP.text = _lastConnectedText.text;
        //_serverIP.text = "Test";
        _lastConnectedInput.text = _lastConnectedText.text;
    }
    #endregion


    /// <summary>
    /// This code should be used as to fix the flow stopping
    /// </summary>
    public void StartConnection()
    {
        thread = new Thread(new ThreadStart(ConnectToServer));
        thread.Start();
    }

    public void ConnectToServer()
    {
        Debug.Log("Connecting to " + _serverIP.text);
        SetLCPanel(_serverIP.text);
        SetString("lastSaved", _serverIP.text);
        try
        {
            Debug.Log("Connecting to " + _serverIP.text);
            client = new TcpClient(_serverIP.text, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
            

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            if (Application.platform != RuntimePlatform.Android)
                SendMessageToServer(EssentialData());
            else
                SendMessageToServer("Android");
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (stream.DataAvailable)
                {
                    int length;
                    // Read incoming stream into byte array.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("Server message received: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendDataButton()
    {
        SendMessageToServer(_clientMessage.text);
    }



    /// <summary>
    /// Need to break the serverMessage for basic string manipulation
    /// First three letters decide what the server msg does
    /// Msg codes:
    /// Toast message for android   -> tst
    /// Check battery               -> bat
    /// Get device info             -> inf
    /// Control screen brightness   -> lum // Needs work
    /// Control volume              -> vup
    /// Volume down                 -> vdn
    /// </summary>
    /// <param name="serverMessage"></param>
    private void ControlClient(string serverMessage)
    {
       // regex for client control


        switch (serverMessage)
        {
            case "tst":
                BluetoothManager.Instance.Toast(serverMessage);
                break;

        }


    }

    private string EssentialData()
    {
        var deviceInfo =
            SystemInfo.deviceName +
            " Device model = " + SystemInfo.deviceModel
            +"\nBattery level / status :" + SystemInfo.batteryLevel + "/" + SystemInfo.batteryStatus
            +"\nOS Info :" + SystemInfo.operatingSystem + " " + SystemInfo.operatingSystemFamily + " family"
            +"\nCPU: " + SystemInfo.processorType + " freq:" + SystemInfo.processorFrequency.ToString()
            +"\nGPU: " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceType + " " + SystemInfo.graphicsDeviceVendor; 
        return deviceInfo;
    }

    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }
}
