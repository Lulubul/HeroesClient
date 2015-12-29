using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.IO;
using Assets.Scripts.Model;

namespace Assets.Scripts.Networking
{
    public sealed class Network
    {
        public LobbyPlayer ClientPlayer { get; set; }
        public Socket Client;
        public Handler Handler;
        private const int Port = 11000;
        private const string Host = "192.168.0.102";
        public const int BufferSize = 2048;
        public byte[] Buffer = new byte[BufferSize];

        private static volatile Network _instance;
        private static readonly object SyncRoot = new object();

        public static Network Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new Network();
                }
                return _instance;
            }
        }

        private Network()
        {
            try
            {
                var remoteEp = new IPEndPoint(IPAddress.Parse(Host), Port);
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false };
                Client.BeginConnect(remoteEp, OnConnect, Client);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        public void OnConnect(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
            Debug.Log("OnConnect");
            try
            {
                sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    Debug.Log("Unable to connect to remote machine, Connect Failed!");
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + "Unusual error during Connect!");
            }
        }

        public void Close()
        {
            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
        }

        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = OnRecievedData;
                sock.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + "Setup Recieve Callback failed!");
            }
        }

        public void OnRecievedData(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
            try
            {
                var nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    var stream = new MemoryStream();
                    stream.Write(Buffer, 0, Buffer.Length);
                    object o = RemoteInvokeMethod.ReadFromStream(stream);
                    var remoteInvoke = (RemoteInvokeMethod)o;
                    var command = remoteInvoke.MethodName;
                    var parameters = remoteInvoke.Parameters;
                    var serviceName = remoteInvoke.ServiceClassName;
                    Handler.Execute(command, serviceName, parameters);
                    SetupRecieveCallback(sock);
                }
                else
                {
                    Debug.Log("Client , disconnected" + sock.RemoteEndPoint);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + "Unusual error druing Recieve!");
            }
        }
    }
}
