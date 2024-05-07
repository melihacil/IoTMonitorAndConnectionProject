using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.ComponentModel;
using System.Linq;

public class WifiServerController : MonoBehaviour
{


    public static WifiServerController instance;

    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    [SerializeField] private string _serverIP = "172.33.133.57";

    private void Start()
    {
        _serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
        Debug.Log("Local IP address = " + _serverIP);
        if (instance != null)
        {
            Debug.Log("Server instance found! destroying = " + gameObject.name);
            Destroy(this.gameObject);
        }
        instance = this;
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }


    //private void OnEnable()
    //{
    //    thread = new Thread(new ThreadStart(SetupServer));
    //    thread.Start();
    //}

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    SendMessageToClient("Hello");
        //}
    }

    public void StartServer()
    {
        



        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(_serverIP);
            server = new TcpListener(localAddr, 3008);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);

                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(message: response);
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

    public void CloseServer()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    private void OnApplicationQuit()
    {
        if (thread == null)
            return;
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }
}
