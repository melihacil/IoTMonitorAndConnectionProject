using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WifiClientController : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Set this to your server's IP address.
    public int serverPort = 31008;             // Set this to your server's port.
    private string firstMessageToSend = "Hello Server!"; // The message to send.

    private TcpClient client;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    private Thread clientReceiveThread;
    private AndroidNativeVolumeService volumeService;


    [SerializeField] private Text _clientMessage;
    [SerializeField] private Text _serverIP;
    [SerializeField] private Text _lastConnectedText;
    [SerializeField] private InputField _lastConnectedInput;
    Thread thread;
    [SerializeField] private string[] oldServers;
    [SerializeField] private GameObject _lastConnectedPanel;

    private bool isFinished = false;

    void Start()
    {
        // ConnectToServer();

        var lastSaved = PlayerPrefs.GetString("lastSaved");
        _lastConnectedPanel.SetActive(false);
        if (lastSaved != "")
        {
            SetLCPanel(lastSaved);
        }

        volumeService = new AndroidNativeVolumeService();
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
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            Debug.Log("Connected to server.");
            isFinished = false;
#if UNITY_ANDROID
            Debug.Log("Android build, starting coroutine listening method");
            StartCoroutine(nameof(ListenDataCoroutine));
            //clientReceiveThread = new Thread(() =>
            //{
            //    // ListenForData should be run on the thread
            //    ListenForData();
            //});
            //clientReceiveThread.Start();
#else
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
           // clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
#endif

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


    private IEnumerator ListenDataCoroutine()
    {
        Debug.Log("Starting to listen");
        byte[] bytes = new byte[1024];
        while (!isFinished)
        {
            yield return new WaitUntil(() => stream.DataAvailable);
            Debug.Log("Listening for data");
            // Check if there's any data available on the network stream
            if (stream.DataAvailable)
            {
                // Debug.Log("DATA AVAILABLE ON CLIENT");
                int length = stream.Read(bytes, 0, bytes.Length);
                if (length > 0)
                {
                    string serverMessage = Encoding.UTF8.GetString(bytes, 0, length);
                    Debug.Log("Server message received: " + serverMessage);
                    ControlClient(serverMessage);
                }
            }
        }
        Debug.Log("Finished listening, closing stream");
        yield return null;
    }

    private void ListenForData()
    {
        try
        {
            Debug.Log("Starting to listen");
            byte[] bytes = new byte[1024];
            while (true)
            {
                Debug.Log("Listening for data");
                // Check if there's any data available on the network stream
                if (stream.DataAvailable)
                {
                    Debug.Log("DATA AVAILABLE ON CLIENT");
                    int length = stream.Read(bytes, 0, bytes.Length);
                    if (length > 0)
                    {
                        string serverMessage = Encoding.UTF8.GetString(bytes, 0, length);
                        Debug.Log("Server message received: " + serverMessage);
                        ControlClient(serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Listening (Client) Socket exception: " + socketException);
        }
    }

    public void SendDataButton()
    {
        SendMessageToServer(_clientMessage.text);
    }

    public void CloseConnection()
    {
        SendMessageToServer("Closing connection, from client");
        BluetoothManager.Instance.Toast("Closed connection");
        isFinished = true;
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
                BluetoothManager.Instance.Toast(serverMessage.Remove(0,3) );
                //BluetoothManager.Instance.Toast(serverMessage.Substring(3));
                break;
            case "inf":
                SendMessageToServer(EssentialData());
                break;
            case "cls":
                SendMessageToServer("Closing connection, from client");
                BluetoothManager.Instance.Toast("Closed connection");
                isFinished = true;
                break;
            case "vdn":
                volumeService.SetSystemVolume(volumeService.GetSystemVolume() - 0.1f);
                SendMessageToServer("Volume down to = " +volumeService.GetSystemVolume() * 100 + "/100");
                break;
            case "vup":
                volumeService.SetSystemVolume(volumeService.GetSystemVolume() + 0.1f);
                SendMessageToServer("Volume up to = " + volumeService.GetSystemVolume() * 100 + "/100");
                break;
            case "bat":
                SendMessageToServer("\nBattery level / status :" + SystemInfo.batteryLevel + "/" + SystemInfo.batteryStatus);
                break;
            default:
                BluetoothManager.Instance.Toast(serverMessage.Remove(0, 3));
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
