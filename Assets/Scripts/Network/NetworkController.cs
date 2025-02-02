﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;



public class NetworkController : MonoBehaviour
{
    public static NetworkController networkController;
    List<string[]> blocksToReplace;
    List<string[]> entitiesToShow;
    float updateTime = 0f;

    void Awake()
    {
        networkController = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Create(string ip, int port, string username)
    {
        Server.Main(ip, port, username);
        DontDestroyOnLoad(gameObject);
        //NATUPnP(true);
    }

    public bool ConnectTo(string ip, int port, string message)
    {
        print("client reached connect1");
        if (Client.Main(ip, port, message))
        {
            DontDestroyOnLoad(gameObject);
            print("client exits connect");
            return true;
        }
        else
        {
            return false;
        }

    }

    //public void NATUPnP(bool boolean)
    //{
    //    Debug.Log("Mapping");
    //    UPnPNAT upnpnat = new UPnPNAT();
    //    Debug.Log("Added mapping x");

    //    IDynamicPortMappingCollection mappings = upnpnat.DynamicPortMappingCollection;
    //    Debug.Log("Added mapping xx");
    //    // Check if the device supports UPnP and has a mapping capability
    //    if (mappings != null)
    //    {
    //        // Add a new port mapping
    //        if (boolean)
    //        {
    //            mappings.Add(GetPublicIP(),7777, "TCP", 7777, GetLocalIP(), true, "Leager Multiplayer Server", 4);
    //            Debug.Log("Added mapping");
    //        }
    //        else
    //            mappings.Remove(GetPublicIP(),7777, "TCP");
    //    }
    //    else
    //    {
    //        Debug.LogError("UPnP is not supported on this device.");
    //    }

    //    Debug.Log("Added mapping out");
    //}


    public void UpdateBlock(int chunkIdx, int idx, int tile)
    {
        if (GameManager.gameManagerReference.isNetworkClient)
        {
            Client.Write(string.Join(";", new string[] { "chunkReplace", chunkIdx.ToString(), idx.ToString(), tile.ToString() + "/" }));
        }
        else if (GameManager.gameManagerReference.isNetworkHost)
        {
            blocksToReplace.Add(new string[] { Server.hostUsername, "chunkReplace", chunkIdx.ToString(), idx.ToString(), tile.ToString() });
        }
    }
    
    public void UpdateBackgroundBlock(int chunkIdx, int idx, int tile)
    {
        if (GameManager.gameManagerReference.isNetworkClient)
        {
            Client.Write(string.Join(";", new string[] { "chunkBgReplace", chunkIdx.ToString(), idx.ToString(), tile.ToString() + "/" }));
        }
        else if (GameManager.gameManagerReference.isNetworkHost)
        {
            blocksToReplace.Add(new string[] { Server.hostUsername, "chunkBgReplace", chunkIdx.ToString(), idx.ToString(), tile.ToString() });
        }
    }

    public void UpdateProperties(int chunkIdx, int idx, string properties)
    {
        if (GameManager.gameManagerReference.isNetworkClient)
        {
            Client.Write(string.Join(";", new string[] { "chunkReplace", chunkIdx.ToString(), idx.ToString(), properties.ToString() + "/" }));
        }
        else if (GameManager.gameManagerReference.isNetworkHost)
        {
            blocksToReplace.Add(new string[] { Server.hostUsername, "chunkReplace", chunkIdx.ToString(), idx.ToString(), properties.ToString() });
        }
    }

    public void DropItem(int item, int amount, float imunityGrab, Vector2 position, Vector2 velocity, string name = "")
    {
        if (GameManager.gameManagerReference.isNetworkClient)
        {
            Client.Write(string.Join(";", new string[] { "dropItem", item.ToString(), amount.ToString(), imunityGrab.ToString(), position.x + "#" + position.y, velocity.x + "#" + velocity.y + "/" }));
        }
        else if (GameManager.gameManagerReference.isNetworkHost)
        {
            foreach (TcpUser user in Server.clients)
            {
                Server.Write(user.tcpClient, string.Join(";", new string[] { "dropItem", item.ToString(), amount.ToString(), imunityGrab.ToString(), position.x + "#" + position.y, velocity.x + "#" + velocity.y, name + "/" }));
            }
        }
    }

    public void UndropItem(string name)
    {
        if (GameManager.gameManagerReference.isNetworkHost)
        {
            foreach (TcpUser user in Server.clients)
            {
                Server.Write(user.tcpClient, string.Join(";", new string[] { "undropItem", name + "/" }));
            }
        }
    }

    public void MoveItem(string name, Vector2 newPosition)
    {
        if (GameManager.gameManagerReference.isNetworkHost)
        {
            foreach (TcpUser user in Server.clients)
            {
                Server.Write(user.tcpClient, string.Join(";", new string[] { "moveItem", name, newPosition.x + "#" + newPosition.y + "/" }));
            }
        }
    }

    public void DropRequest(int item, int amount, string dropName, string userName)
    {
        if (GameManager.gameManagerReference.isNetworkHost)
        {
            foreach (TcpUser user in Server.clients)
            {
                if (user.username == userName)
                {
                    Server.Write(user.tcpClient, string.Join(";", new string[] { "dropRequest", item.ToString(), amount.ToString(), dropName + "/" }));
                }
            }
        }
    }

    public void DropCallback(int item, int amount, string dropName)
    {
        if (GameManager.gameManagerReference.isNetworkClient)
        {
            Client.Write(string.Join(";", new string[] { "dropCallback", item.ToString(), amount.ToString(), dropName + "/" }));
        }
    }

    void Update()
    {
        if (Server.isOpen)
        {
            if (Server.listener.Pending() && GameManager.gameManagerReference != null)
            {
                string newClientUsername = Server.AceptClient(1024);
                if (newClientUsername != "null")
                    Debug.Log(newClientUsername + " has joined!");
            }
        }

        if (GameManager.gameManagerReference != null)
        {
            if (updateTime > 0.05f)
            {
                updateTime = 0f;

                if (GameManager.gameManagerReference.isNetworkHost)
                {
                    List<TcpUser> disconnectUsers = new List<TcpUser>();

                    foreach (TcpUser user in Server.clients)
                    {
                        if (user.tcpClient.GetStream().DataAvailable)
                        {
                            string entry = "";
                            do
                            {
                                entry += Server.Read(user.tcpClient, 16384);
                            } while (entry[entry.Length - 1] != '/');

                            string[] messages = entry.Split('/');

                            foreach (string message in messages)
                            {
                                string[] input = message.Split(';');

                                if (input[0] == "disconnect")
                                {
                                    disconnectUsers.Add(user);
                                }

                                if (input[0] == "playerPos")
                                {
                                    user.position = new Vector4(System.Convert.ToSingle(input[1]), System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]), System.Convert.ToSingle(input[4]));
                                    user.flipX = System.Convert.ToBoolean(input[5]);
                                }

                                if (input[0] == "dropItem")
                                {
                                    int item = int.Parse(input[1]);
                                    int amount = int.Parse(input[2]);
                                    float imunityGrab = float.Parse(input[3]);

                                    string[] position = input[4].Split('#');
                                    Vector2 vPosition = new Vector2(float.Parse(position[0]), float.Parse(position[1]));
                                    string[] velocity = input[5].Split('#');
                                    Vector2 vVelocity = new Vector2(float.Parse(velocity[0]), float.Parse(velocity[1]));


                                    ManagingFunctions.DropItem(item, vPosition, vVelocity, amount, imunityGrab);
                                }

                                if (input[0] == "dropCallback")
                                {
                                    int item = int.Parse(input[1]);
                                    int amount = int.Parse(input[2]);
                                    string dropName = input[3];
                                    Transform drop = ManagingFunctions.dropContainer.transform.Find(dropName);
                                    Debug.Log("Drop: " + dropName + " amm: " + amount);

                                    if (amount == 0)
                                    {
                                        UndropItem(dropName);
                                        if (drop != null)
                                        {
                                            drop.GetComponent<DroppedItemController>().gettingEnabled = false;
                                            Destroy(drop.gameObject);
                                        }
                                    }
                                    else
                                    {
                                        if (drop != null)
                                        {
                                            DroppedItemController droppedItem = drop.GetComponent<DroppedItemController>();
                                            droppedItem.amount = amount;
                                            droppedItem.gettingEnabled = true;
                                        }
                                    }
                                }

                                if (input[0] == "chunkReplace")
                                {
                                    ChunkController chunk = GameManager.gameManagerReference.chunkContainer.transform.GetChild(System.Convert.ToInt32(input[1])).GetComponent<ChunkController>();
                                    chunk.TileGrid[System.Convert.ToInt32(input[2])] = System.Convert.ToInt32(input[3]);
                                    chunk.UpdateChunk();
                                    blocksToReplace.Add(new string[] { user.username, input[0], input[1], input[2], input[3] });
                                    LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
                                }
                            }
                        }

                        Transform dummyPlayer = GameManager.gameManagerReference.dummyObjects.Find(user.username);

                        if (dummyPlayer == null)
                        {
                            GameObject newDummy = Instantiate(GameManager.gameManagerReference.lecterSprite, Vector3.zero, Quaternion.identity, GameManager.gameManagerReference.dummyObjects);
                            newDummy.name = user.username;
                            newDummy.transform.GetChild(0).GetComponent<TextMesh>().text = user.username;
                            float[] input = new float[] { user.position.x, user.position.y, user.position.z, user.position.w };

                            newDummy.transform.position = new Vector2(System.Convert.ToSingle(input[0]), System.Convert.ToSingle(input[1]));
                            newDummy.GetComponent<Rigidbody2D>().velocity = new Vector2(System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]));
                        }
                        else
                        {
                            float[] input = new float[] { user.position.x, user.position.y, user.position.z, user.position.w};

                            dummyPlayer.transform.position = new Vector2(System.Convert.ToSingle(input[0]), System.Convert.ToSingle(input[1]));
                            dummyPlayer.GetComponent<Rigidbody2D>().velocity = new Vector2(System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]));
                            dummyPlayer.GetComponent<SpriteRenderer>().flipX = user.flipX;
                        }
                    }

                    foreach (TcpUser removedUser in disconnectUsers)
                    {
                        Server.clients.Remove(removedUser);

                        Transform dummyPlayer = GameManager.gameManagerReference.dummyObjects.Find(removedUser.username);

                        if (dummyPlayer != null)
                        {
                            Destroy(dummyPlayer.gameObject);
                        }

                        Debug.Log(removedUser.username + " has left!");
                    }

                    foreach (TcpUser userData in Server.clients)
                    {
                        foreach (TcpUser user in Server.clients)
                        {
                            if (user != userData)
                            {
                                Server.Write(user.tcpClient, string.Join(";", new string[] { "playerPos", userData.username, userData.position.x.ToString(), userData.position.y.ToString(), userData.position.z.ToString(), userData.position.w + "/" }));
                            }
                        }

                        foreach (TcpUser removedUser in disconnectUsers)
                        {
                            Server.Write(userData.tcpClient, string.Join(";", new string[] { "removePlayer", removedUser.username + "/" }));
                        }

                        if (GameManager.gameManagerReference.player.alive)
                            Server.Write(userData.tcpClient, string.Join(";", new string[] { "playerPos", Server.hostUsername, GameManager.gameManagerReference.player.transform.position.x.ToString(), GameManager.gameManagerReference.player.transform.position.y.ToString(), GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.x.ToString(), GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.y.ToString(), GameManager.gameManagerReference.player.GetComponent<SpriteRenderer>().flipX.ToString() + "/" }));
                        else
                            Server.Write(userData.tcpClient, string.Join(";", new string[] { "playerPos", Server.hostUsername, "0", "-100", "0", "0", "False/" }));

                        Server.Write(userData.tcpClient, string.Join(";", new string[] { "setTime", GameManager.gameManagerReference.internationalTime + "/" }));

                        if (blocksToReplace.Count > 0)
                        {
                            foreach (string[] blockData in blocksToReplace)
                            {
                                if (blockData[0] != userData.username)
                                    Server.Write(userData.tcpClient, string.Join(";", new string[] { "chunkReplace", blockData[2], blockData[3], blockData[4] + "/" }));
                            }
                        }
                    }

                    blocksToReplace = new List<string[]>();
                }
                else if (GameManager.gameManagerReference.isNetworkClient)
                {
                    if (Client.stream.DataAvailable)
                    {
                        string entry = "";
                        do
                        {
                            entry += Client.Read(16384);
                        } while (entry[entry.Length - 1] != '/');

                        string[] messages = entry.Split('/');


                        foreach (string message in messages)
                        {
                            string[] input = message.Split(';');

                            if (input[0] == "disconnect")
                            {
                                Application.Quit();
                            }

                            if (input[0] == "playerPos")
                            {
                                Transform dummyPlayer = GameManager.gameManagerReference.dummyObjects.Find(input[1]);

                                if (dummyPlayer == null)
                                {
                                    GameObject newDummy = Instantiate(GameManager.gameManagerReference.lecterSprite, Vector3.zero, Quaternion.identity, GameManager.gameManagerReference.dummyObjects);
                                    newDummy.name = input[1];
                                    newDummy.transform.GetChild(0).GetComponent<TextMesh>().text = input[1];
                                    newDummy.transform.position = new Vector2(System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]));
                                    newDummy.GetComponent<Rigidbody2D>().velocity = new Vector2(System.Convert.ToSingle(input[4]), System.Convert.ToSingle(input[5]));
                                }
                                else
                                {
                                    dummyPlayer.transform.position = new Vector2(System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]));
                                    dummyPlayer.GetComponent<Rigidbody2D>().velocity = new Vector2(System.Convert.ToSingle(input[4]), System.Convert.ToSingle(input[5]));
                                    dummyPlayer.GetComponent<SpriteRenderer>().flipX = System.Convert.ToBoolean(input[6]);
                                }
                            }

                            if (input[0] == "removePlayer")
                            {
                                Transform dummyPlayer = GameManager.gameManagerReference.dummyObjects.Find(input[1]);

                                if (dummyPlayer != null)
                                {
                                    Destroy(dummyPlayer.gameObject);
                                }
                            }

                            if (input[0] == "dropItem")
                            {
                                int item = int.Parse(input[1]);
                                int amount = int.Parse(input[2]);
                                float imunityGrab = float.Parse(input[3]);

                                string[] position = input[4].Split('#');
                                Vector2 vPosition = new Vector2(float.Parse(position[0]), float.Parse(position[1]));
                                string[] velocity = input[5].Split('#');
                                Vector2 vVelocity = new Vector2(float.Parse(velocity[0]), float.Parse(velocity[1]));

                                string name = input[6];

                                ManagingFunctions.DropItem(item, vPosition, vVelocity, amount, imunityGrab, true, name);
                            }

                            if (input[0] == "undropItem")
                            {
                                string dropName = input[1];
                                Transform drop = ManagingFunctions.dropContainer.transform.Find(dropName);

                                if (drop != null)
                                {
                                    Destroy(drop.gameObject);
                                }
                            }

                            if (input[0] == "moveItem")
                            {
                                string dropName = input[1];
                                string[] position = input[2].Split('#');
                                Vector2 vPosition = new Vector2(float.Parse(position[0]), float.Parse(position[1]));
                                Transform drop = ManagingFunctions.dropContainer.transform.Find(dropName);

                                if (drop != null)
                                {
                                    drop.position = vPosition;
                                }
                            }

                            if (input[0] == "dropRequest")
                            {
                                int item = int.Parse(input[1]);
                                int amount = int.Parse(input[2]);
                                string dropName = input[3];
                                int itemReturn = 0;

                                if (ManagingFunctions.dropContainer.transform.Find(dropName) != null)
                                {
                                    for (int i = 0; i < amount; i++)
                                    {
                                        if (!StackBar.AddItemInv(item)) itemReturn++;
                                    }

                                    DropCallback(item, itemReturn, dropName);
                                }
                            }

                            if (input[0] == "chunkReplace")
                            {
                                ChunkController chunk = GameManager.gameManagerReference.chunkContainer.transform.GetChild(System.Convert.ToInt32(input[1])).GetComponent<ChunkController>();
                                chunk.TileGrid[System.Convert.ToInt32(input[2])] = System.Convert.ToInt32(input[3]);
                                chunk.UpdateChunk();
                                LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
                            }

                            if (input[0] == "setTime")
                            {
                                GameManager.gameManagerReference.internationalTime = System.Convert.ToSingle(input[1]);
                            }
                        }
                    }

                    if (GameManager.gameManagerReference.player.alive)
                        Client.Write(string.Join(";", new string[] { "playerPos", GameManager.gameManagerReference.player.transform.position.x.ToString(), GameManager.gameManagerReference.player.transform.position.y + "", GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.x + "", GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.y.ToString(), GameManager.gameManagerReference.player.GetComponent<SpriteRenderer>().flipX.ToString() + "/" }));
                    else
                        Client.Write(string.Join(";", new string[] { "playerPos", "0", "-100", "0", "0", "False/" }));
                }
            }
            else
            {
                updateTime += Time.deltaTime;
            }
        }
    }

    public void LateUpdate()
    {
        entitiesToShow = new List<string[]>();
    }

    public static string GetLocalIP()
    {
        try
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
        catch
        {
            return "";
        }
    }
}


public class Server
{
    public static string targetIp;
    public static int targetPort;
    public static string externalIp;
    public static bool isOpen = false;
    public static string hostUsername;

    public static TcpListener listener;
    public static List<TcpUser> clients = new List<TcpUser>();
    public static Thread hostBroadcastsThread;

    public static void Main(string ip, int portParam, string username)
    {
        hostUsername = username;
        clients = new List<TcpUser>();
        IPAddress ipAddress = IPAddress.Parse(ip);
        int port = portParam;

        listener = new TcpListener(ipAddress, port);
        listener.Start();

        targetIp = ip;
        targetPort = portParam;

        ThreadStart task = new ThreadStart(HostBroadcasts);
        Thread hostBroadcaster = new Thread(task);
        hostBroadcaster.Start();
        hostBroadcastsThread = hostBroadcaster;

        Debug.Log("Server started.");
        isOpen = true;
    }

    public static string AceptClient(int messageBuffer)
    {
        if (listener.Pending())
        {
            Debug.Log("Someone is entering...");
            TcpClient client = listener.AcceptTcpClient();

            NetworkStream stream = client.GetStream();


            byte[] data = new byte[messageBuffer]; //creates data with X buffer
            int bytesRead = stream.Read(data, 0, data.Length); //reads the stream
            string message = Encoding.ASCII.GetString(data, 0, bytesRead);

            //TODO Check if username in use, if true then refuse the connect, else let client connect
            Debug.Log("client user:" + message);
            List<string> usernamesInUse = new List<string> { hostUsername, "null", "Lecter" };
            foreach (TcpUser c in clients)
                usernamesInUse.Add(c.username);

            if (usernamesInUse.Contains(message))
            {
                client.GetStream().Close();
                Debug.Log("rejected client for repeated username");
                return "null";
            }

            Debug.Log("sending client version" + Application.version);
            Write(client, Application.version);


            Debug.Log("get ready");
            string a = Read(client, 1024);
            Debug.Log("client response" + a);
            if (a != "d")
            {
                Debug.Log("got here so far");
                Write(client, string.Join(";", new string[] { GameManager.gameManagerReference.WorldHeight + "", GameManager.gameManagerReference.WorldWidth + "" }));
                Debug.Log(1);
                Read(client, 1024);

                Debug.Log(2);

                int[][] tilemaps = new int[GameManager.gameManagerReference.WorldWidth][];
                string[][] tileprops = new string[GameManager.gameManagerReference.WorldWidth][];

                for (int i = 0; i < tilemaps.Length; i++)
                {
                    tilemaps[i] = (int[])GameManager.gameManagerReference.chunkContainer.transform.GetChild(i).GetComponent<ChunkController>().TileGrid.Clone();
                }
                for (int i = 0; i < tileprops.Length; i++)
                {
                    tileprops[i] = (string[])GameManager.gameManagerReference.chunkContainer.transform.GetChild(i).GetComponent<ChunkController>().TilePropertiesArr.Clone();
                }

                ThreadStart lastStuff = new ThreadStart(() => TheStuff(client, tilemaps, tileprops, message));
                Thread stuff = new Thread(lastStuff);
                stuff.Start();

                return message;
            }
            else
            {
                client.GetStream().Close();
                client.Close();
                return "null";
            }
        }
        else
        {
            return "null";
        }
    }

    public static void Write(TcpClient client, string message)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.ASCII.GetBytes(message);

        stream.Write(data, 0, data.Length);
    }

    public static void TheStuff(TcpClient client, int[][] tilemaps, string[][] tilepropmaps, string message)
    {
        SendChunks(client, tilemaps, tilepropmaps);
         
        Debug.Log(3);
        Write(client, string.Join(";", GameManager.gameManagerReference.GetBiomes()).Length + "");
        Read(client, 1024);

        Debug.Log(4);
        Write(client, string.Join(";", GameManager.gameManagerReference.GetBiomes()));
        Read(client, 1024);

        clients.Add(new TcpUser(client, message));
    }

    public static void SendChunks(TcpClient client,int[][] tilemaps, string[][] tilepropmaps)
    {
        for (int i = 0; i < GameManager.gameManagerReference.WorldWidth; i++)
        {
            int[] tilemap = tilemaps[i];
            string[] result = ManagingFunctions.ConvertIntToStringArray(tilemap);
            string export = string.Join(";", result) + "d";

            Write(client, export.Length + "");
            Read(client, 1024);
            Write(client, export);
            Read(client, 1024);


            string[] tileprops = tilepropmaps[i];
            string export2 = string.Join("$", tileprops);

            Write(client, export2.Length + "");
            Read(client, 1024);
            Write(client, export2);
            Read(client, 1024);
        }
        //wait to exit
    }

    public static string Read(TcpClient client, int buffer)
    {
        NetworkStream stream = client.GetStream();

        byte[] data = new byte[buffer]; //creates data with X buffer
        int bytesRead = stream.Read(data, 0, data.Length); //reads the stream
        string message = Encoding.ASCII.GetString(data, 0, bytesRead);

        return message;
    }

    public static void HostBroadcasts()
    {
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            // Set the port number
            int port = 7777;

            // Create a UDP socket
            udpSocket.EnableBroadcast = true;

            // Create an endpoint for receiving broadcast messages
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            udpSocket.Bind(localEndPoint);

            Debug.Log($"Listening for broadcast messages on port {port}...");

            // Receive broadcast messages
            byte[] receiveBuffer = new byte[1024];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                int bytesRead = udpSocket.ReceiveFrom(receiveBuffer, ref remoteEndPoint);
                string receivedMessage = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                Debug.Log($"Received broadcast message: {receivedMessage}");

                // Respond to the broadcast message
                string responseMessage = NetworkController.GetLocalIP() + "#" + hostUsername;
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                udpSocket.SendTo(responseBytes, remoteEndPoint);
                Debug.Log($"Sent response to {remoteEndPoint}");
            }
        }
        catch (ThreadAbortException)
        {
            udpSocket.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void CloseServer()
    {
        foreach (TcpUser user in clients)
        {
            Write(user.tcpClient, "disconnect");
        }
        clients = null;
        listener.Stop();
        listener = null;
        targetIp = "";
        externalIp = "";
        targetPort = 0;
        hostBroadcastsThread.Abort();
        hostBroadcastsThread = null;
        isOpen = false;
    }
}

public class Client
{
    static IPAddress serverIpAddress;
    public static TcpClient client;
    public static NetworkStream stream;
    public static Difficulty difficulty = Difficulty.normal;

    public static int[] worldProportionsLoad;
    public static int[] worldMapLoad;
    public static int[] backgroundMapLoad;
    public static string[] worldMapPropLoad;
    public static string[] worldBiomesLoad;

    public static bool Main(string ip, int portParam, string message)
    {
        int serverPort = portParam;

        client = new TcpClient(ip, serverPort);
        Debug.Log("check");
        stream = client.GetStream();
        Debug.Log("check");
        byte[] data = Encoding.ASCII.GetBytes(message);

        Debug.Log("sending username");
        stream.Write(data, 0, data.Length);

        Debug.Log("getting version");
        string version = Read(1024);
        Debug.Log("client side version: " + Application.version + "server version:" + version + "thats it");
        Debug.Log(version);
        if (version != Application.version)
        {
            Write("d");
            MultiplayerPanelController.nameInUse = true;
            return false;
        }
        else
        {
            Write("OK");
            Debug.Log("getting data");

            Debug.Log("data1");
            int[] wp = ManagingFunctions.ConvertStringToIntArray(Read(1024).Split(';'));
            Write("Received");
            worldProportionsLoad = wp;

            Debug.Log("data2");
            GetMap();
            string i = Read(8196);
            int biomesLenght = System.Convert.ToInt32(i);
            Write("Received");
            string mapBiomes = Read(biomesLenght + 8196);
            Write("Received");

            Debug.Log("converting data");

            //Debug.Log(mapData);
            //worldMapLoad = ManagingFunctions.ConvertStringToIntArray(mapData.Split(';'));
            worldBiomesLoad = mapBiomes.Split(';');

            return true;
        }
    }

    public static string Read(int buffer)
    {
        byte[] data = new byte[buffer]; //creates data with X buffer
        int bytesRead = stream.Read(data, 0, data.Length); //reads the stream
        string message = Encoding.ASCII.GetString(data, 0, bytesRead);


        return message;
    }

    public static void GetMap()
    {
        string map = "";
        string bgMap = "";
        string mapprop = "";

        for (int i = 0; i < worldProportionsLoad[1]; i++)
        {
            string ab = Read(8192);
            int buffer = System.Convert.ToInt32(ab);
            Write("a weno");


            string mapGrid = "";
            do
            {
                mapGrid += Read(buffer);
            } while (mapGrid[mapGrid.Length - 1] != 'd');
            mapGrid = mapGrid.Remove(mapGrid.Length - 1);

            if (map == "")
                map = mapGrid;
            else
                map = string.Join(";", new string[] { map, mapGrid });
            Write("a weno");


            //tileproperties
            string ac = Read(8192);
            int buffer2 = System.Convert.ToInt32(ac);
            Write("a weno");


            string mapProp = "";
            do
            {
                mapProp += Read(buffer2);
            } while (mapProp.Length < buffer2);

            if (mapprop == "")
                mapprop = mapProp;
            else
                mapprop = string.Join("$", new string[] { mapprop, mapProp });
            Write("a weno");

            string ad = Read(8192);
            int buffer3 = System.Convert.ToInt32(ad);
            Write("a weno");


            string bgMapGrid = "";
            do
            {
                bgMapGrid += Read(buffer3);
            } while (bgMapGrid[bgMapGrid.Length - 1] != 'd');
            bgMapGrid = bgMapGrid.Remove(bgMapGrid.Length - 1);

            if (bgMap == "")
                bgMap = bgMapGrid;
            else
                bgMap = string.Join(";", new string[] { bgMap, bgMapGrid });
            Write("a weno");
        }

        worldMapLoad = ManagingFunctions.ConvertStringToIntArray(map.Split(';'));
        backgroundMapLoad = ManagingFunctions.ConvertStringToIntArray(bgMap.Split(';'));
        worldMapPropLoad = mapprop.Split('$');
        //leaves with server write turn
    }

    public static void Write(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        stream.Write(data, 0, data.Length);
    }

    public static void Disconnect()
    {
        Write("disconnect/");
        stream = null;
        client = null;
        serverIpAddress = null;
        worldProportionsLoad = null;
        worldMapLoad = null;
        backgroundMapLoad = null;
        worldBiomesLoad = null;
    }
}

public class TcpUser
{
    public string username;
    public Vector4 position;
    public bool flipX;
    public TcpClient tcpClient;

    public TcpUser(TcpClient client, string username)
    {
        tcpClient = client;
        this.username = username;
    }
}