using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.ComponentModel;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Android;
using TMPro;


public class WifiServerController : MonoBehaviour
{


    public static WifiServerController instance;

    // Declare a dictionary to store streams associated with client IDs
    private Dictionary<int, NetworkStream> clientStreams = new Dictionary<int, NetworkStream>();
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;
    private Dictionary<int, string> _connectedDevices = new Dictionary<int, string>();

    [SerializeField] private string _serverIP = "172.33.133.57";
    [SerializeField] private GameObject _androidDeviceControls;
    [SerializeField] private GameObject _otherDeviceControls;
    [SerializeField] private TextMeshProUGUI _logTexts;
    [SerializeField] private Text _serverMessage;
    [SerializeField] private Text _connectedClients;

    [SerializeField] private float maxWaitTime;

    Queue<Action> jobs = new Queue<Action>();

    /// <summary>
    /// Changes the server ip to be compliant with the device ip,
    /// it also creates threads to start the server
    /// </summary>
    private void Start()
    {
        // Singleton check
        if (instance != null)
        {
            Debug.Log("Server instance found! destroying = " + gameObject.name);
            Destroy(this.gameObject);
        }
        // Get device ip
        try
        {
            _serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            Debug.Log("Local IP address = " + _serverIP);

        }
        catch (Exception e) { 
            Debug.LogError(e);
        }
        instance = this;


#if UNITY_ANDROID
        Debug.Log("Starting Android Server Setup");
        StartCoroutine(nameof(CoroutineSetupServer));
        //thread = new Thread(() =>
        //{
        //    // ListenForData should be run on the thread
        //    SetupServer();
        //});
        //thread.Start();
#else
            Debug.Log("Other build, starting thread server");
            thread = new Thread(new ThreadStart(SetupServer));
            thread.Start();
#endif

        // Close client control panels
        _androidDeviceControls.SetActive(false);
        _otherDeviceControls.SetActive(false);

        //AndroidDevice.hardwareType.Equals(AndroidDevice.hardwareType);
        UnityEngine.Device.Screen.brightness = 10f;
    }




    //private void OnEnable()
    //{
    //    thread = new Thread(new ThreadStart(SetupServer));
    //    thread.Start();
    //}

    private void Update()
    {
        // Server main thread jobs from other threads will run
        while (jobs.Count > 0)
        {
            jobs.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// Can be used for event driven start
    /// </summary>
    public void StartServer()
    {
        if (server != null)
        {
            Debug.Log("Server Already Started!");
            return;
        }

        Debug.Log("Starting Android Server Setup");
        StartCoroutine(nameof(CoroutineSetupServer));
    }

    private IEnumerator CoroutineSetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(_serverIP);
            server = new TcpListener(localAddr, 31008);
            server.Start();
            Debug.Log("Starting WIFI Server");
            while (true)
            {
                if (server == null)
                {
                    break;
                }
                // Check for pending connection without blocking indefinitely
                if (server != null && server.Pending())
                {
                    client = server.AcceptTcpClient();
                    Debug.Log("Connected!");

                    // Handle the connection asynchronously
                    Task.Run(() => HandleClientConnection(client));
                }

                yield return null; // Allow other coroutines to run and avoid blocking indefinitely
            }
        }
        finally
        {
            Debug.Log("Closing Server");
            server.Stop();
        }
    }

    private async void HandleClientConnection(TcpClient client)
    {
        try
        {
            byte[] buffer = new byte[1024];
            string data = null;
            bool init = false;

            using (stream = client.GetStream())
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Received: " + data);
                    if (!init)
                    {
                        init = true;
                        InitConnection(data);
                    }
                    string response = "Server response: Data received, client data:" + data.ToString();
                    //SendMessageToClient(client.GetHashCode(), message: response);
                    SendMessageToClient(message: response);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error handling client connection: " + ex.Message);
        }
        finally
        {
            client.Close();
        }
    }

    private void AddDevicesText(string deviceInfo)
    {
        
        WifiServerController.instance.AddJob(() =>
        {
            // Will run on main thread, hence issue is solved
            _connectedClients.text +=  "Connected: " + deviceInfo + "Device" + "Poco X4";
        });
    }

    /// <summary>
    /// Sets the server up for windows devices
    /// Runs on thread for each device
    /// </summary>
    private void SetupServer()
    {
        bool isInit = false;
        try
        {
            IPAddress localAddr = IPAddress.Parse(_serverIP);
            server = new TcpListener(localAddr, 31008);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;
            bool init = false;
            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                // Store the client's stream in the dictionary using a unique identifier (e.g., client.GetHashCode())
                int hashCode = client.GetHashCode();
                clientStreams.Add(hashCode,stream);
                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);
                    if (!isInit)
                    {
                        isInit = true;
                    }
                    if (!init)
                    {
                        init = true;
                        AddDevicesText(data);
                        InitConnection(data);
                    }
                    string response = "Server response: " + data.ToString();

                    // Send the response to the client using the stored stream
                    SendMessageToClient(client.GetHashCode(), message: response);
                    WifiServerController.instance.AddJob(() =>
                    {
                        _logTexts.text += data.ToString();
                    });
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }
    private void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }

    /// <summary>
    /// Jobs that need to be run by the main thread
    /// are sent here from the other threads
    /// </summary>
    /// <param name="deviceInfo"></param>
    private void InitConnection(string deviceInfo)
    {
        WifiServerController.instance.AddJob(() =>
        {
            // Will run on main thread, hence issue is solved
            if (deviceInfo == "Android")
            {
                _androidDeviceControls.SetActive(true);
            }
            else
            {
                _otherDeviceControls.SetActive(true);
            }
        });
    }

    /// <summary>
    /// Client message codes similar to HTTP status
    /// X 100
    /// X 300
    /// 200 message control ok
    /// 400 cant found command
    /// 500 message not clear
    /// 
    /// </summary>
    /// <param name="clientMessage"></param>
    private void CheckClientData(string clientMessage)
    {
        switch (clientMessage)
        {
            case "200":
                Debug.Log("Message sent!- Ok");
                break;
            case "400":
                Debug.Log("Message cannot be send! No device found!");

                break;
            case "500":
                Debug.Log("Message did not send! Error connecting to the device");
                break;
        }
    }

    public void CloseServer()
    {
        Debug.Log("Closing Android Server Setup");

        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    private void OnApplicationQuit()
    {
        if (thread == null || stream == null || client == null || server == null)
            return;
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(int clientId, string message)
    {
        if (clientStreams.ContainsKey(clientId))
        {
            NetworkStream clientStream = clientStreams[clientId];
            byte[] msg = Encoding.UTF8.GetBytes(message);
            clientStream.Write(msg, 0, msg.Length);
            Debug.Log("Sent to client " + clientId + ": " + message);

        }
        else
        {
            Debug.LogError("Client with ID " + clientId + " not found.");
        }
    }

    public void SendMessageToClient()
    {
        byte[] msg = Encoding.UTF8.GetBytes(_serverMessage.text);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent to client: " + _serverMessage.text);
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent to client: " + message);
    }
}
