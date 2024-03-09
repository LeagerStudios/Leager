using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using NATUPNPLib;



public class NetworkController : MonoBehaviour
{
    public static NetworkController networkController;
    List<string[]> blocksToReplace;
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
        if(Client.Main(ip, port, message))
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

    public void NATUPnP(bool boolean)
    {
        UPnPNAT upnpnat = new UPnPNAT();

        IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;

        // Check if the device supports UPnP and has a mapping capability
        if (mappings != null)
        {
            // Add a new port mapping
            if (boolean)
                mappings.Add(7777, "TCP", Server.targetPort, "ip", true, "Leager Multiplayer Server");
            else
                mappings.Remove(7777, "TCP");
        }
        else
        {
            Debug.LogError("UPnP is not supported on this device.");
        }
    }

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

    void Update()
    {
        if (Server.isOpen)
        {
            if (Server.listener.Pending() && GameManager.gameManagerReference != null)
            {
                string newClientUsername = Server.AceptClient(1024);
                if(newClientUsername != "null")
                Debug.Log(newClientUsername + " has joined!");
            }
        }

        if (GameManager.gameManagerReference != null)
        {
            if (updateTime > 0.1f)
            {
                updateTime = 0f;

                if (GameManager.gameManagerReference.isNetworkHost)
                {
                    List<TcpUser> disconnectUsers = new List<TcpUser>();

                    foreach (TcpUser user in Server.clients)
                    {
                        if (user.tcpClient.GetStream().DataAvailable)
                        {
                            string entry = Server.Read(user.tcpClient, 16384);
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
                            newDummy.transform.position = user.position;
                        }
                        else
                        {
                            if (dummyPlayer.transform.position.x != user.position.x)
                                dummyPlayer.GetComponent<SpriteRenderer>().flipX = dummyPlayer.transform.position.x > user.position.x;
                            dummyPlayer.transform.position = user.position;
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
                            Server.Write(userData.tcpClient, string.Join(";", new string[] { "playerPos", Server.hostUsername, GameManager.gameManagerReference.player.transform.position.x.ToString(), GameManager.gameManagerReference.player.transform.position.y.ToString(), GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.x.ToString(), GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.y + "/" }));
                        else
                            Server.Write(userData.tcpClient, string.Join(";", new string[] { "playerPos", Server.hostUsername, "0", "-100", "0", "0/" }));

                        Server.Write(userData.tcpClient, string.Join(";", new string[] { "setTime", GameManager.gameManagerReference.dayTime + "/" }));

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
                else if(GameManager.gameManagerReference.isNetworkClient)
                {
                    if (Client.stream.DataAvailable)
                    {
                        string entry = Client.Read(16384);
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
                                    if (dummyPlayer.transform.position.x != System.Convert.ToSingle(input[2]))
                                        dummyPlayer.GetComponent<SpriteRenderer>().flipX = dummyPlayer.transform.position.x > System.Convert.ToSingle(input[2]);
                                    dummyPlayer.transform.position = new Vector2(System.Convert.ToSingle(input[2]), System.Convert.ToSingle(input[3]));
                                    dummyPlayer.GetComponent<Rigidbody2D>().velocity = new Vector2(System.Convert.ToSingle(input[4]), System.Convert.ToSingle(input[5]));
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

                            if (input[0] == "chunkReplace")
                            {
                                ChunkController chunk = GameManager.gameManagerReference.chunkContainer.transform.GetChild(System.Convert.ToInt32(input[1])).GetComponent<ChunkController>();
                                chunk.TileGrid[System.Convert.ToInt32(input[2])] = System.Convert.ToInt32(input[3]);
                                chunk.UpdateChunk();
                                LightController.lightController.AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
                            }

                            if (input[0] == "setTime")
                            {
                                GameManager.gameManagerReference.dayTime = System.Convert.ToInt32(input[1]);
                            }
                        }
                    }

                    if (GameManager.gameManagerReference.player.alive)
                        Client.Write(string.Join(";", new string[] { "playerPos", GameManager.gameManagerReference.player.transform.position.x.ToString(), GameManager.gameManagerReference.player.transform.position.y + "", GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.x +"", GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().velocity.y + "/" }));
                    else
                        Client.Write(string.Join(";", new string[] { "playerPos", "0", "-100", "0", "0/" }));
                }
            }
            else
            {
                updateTime += Time.deltaTime;
            }
        }
    }

    public static string GetLocalIP()
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
            List<string> usernamesInUse = new List<string> { hostUsername, "null", "Lecter" };
            foreach (TcpUser c in clients)
                usernamesInUse.Add(c.username);

            if (usernamesInUse.Contains(message))
            {
                client.GetStream().Close();
                return "null";
            }
            Write(client, Application.version);
            
            Debug.Log("get ready");
            string a = Read(client, 1024);
            Debug.Log(a);
            if (a != "d")
            {
                Debug.Log("got here so far");
                Write(client, string.Join(";", new string[] { GameManager.gameManagerReference.WorldHeight + "", GameManager.gameManagerReference.WorldWidth + "" }));
                Debug.Log(1);
                Read(client, 1024);

                Debug.Log(2);
                SendChunks(client);

                Debug.Log(3);
                Write(client, string.Join(";", GameManager.gameManagerReference.GetBiomes()).Length + "");
                Read(client, 1024);

                Debug.Log(4);
                Write(client, string.Join(";", GameManager.gameManagerReference.GetBiomes()));
                Read(client, 1024);

                clients.Add(new TcpUser(client, message));
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

    public static void SendChunks(TcpClient client)
    {
        string readMain = "heugefiefbrgbwghwgruenrggv";

        while (readMain != "exit")
        {
            readMain = Read(client, 1024);
            Write(client, "next");
            Debug.Log("main" + readMain);

            if (readMain.Contains("requestchunk"))
            {
                string subRead = "HOLA WENAS";

                while (subRead != "exit")
                {

                    int chunkid = System.Convert.ToInt32(readMain.Split(':')[1]);
                    subRead = Read(client, 2048);
                    Debug.Log("sub" + readMain);

                    if (subRead == "mapgrid")
                    {
                        int[] tilemap = (int[])GameManager.gameManagerReference.chunkContainer.transform.GetChild(chunkid).GetComponent<ChunkController>().TileGrid.Clone();
                        string[] result = ManagingFunctions.ConvertIntToStringArray(tilemap);
                        string export = string.Join(";", result);

                        Write(client, export.Length + "");
                        Read(client, 1024);
                        Write(client, export);
                    }
                }

                Write(client, "exiting");
            }

        }
        //wait to exit
        Read(client, 1024);
    }

    public static string Read(TcpClient client, int buffer)
    {
        NetworkStream stream = client.GetStream();



        byte[] data = new byte[buffer]; //creates data with X buffer
        int bytesRead = stream.Read(data, 0, data.Length); //reads the stream
        string message = Encoding.ASCII.GetString(data, 0, bytesRead);

        return message;
    }

    public static void CloseServer()
    {
        foreach(TcpUser user in clients)
        {
            Write(user.tcpClient, "disconnect");
        }
        clients = null;
        listener.Stop();
        listener = null;
        targetIp = "";
        externalIp = "";
        targetPort = 0;
        isOpen = false;
    }
}

public class Client
{
    static IPAddress serverIpAddress;
    public static TcpClient client;
    public static NetworkStream stream;

    public static int[] worldProportionsLoad;
    public static int[] worldMapLoad;
    public static string[] worldMapPropLoad;
    public static string[] worldBiomesLoad;

    public static bool Main(string ip, int portParam, string message)
    {
        
        serverIpAddress = IPAddress.Parse(ip); // replace with the IP address of the server
        int serverPort = portParam; // replace with the port number used by the server

        Debug.Log("check");

        client = new TcpClient(serverIpAddress.ToString(), serverPort);
        Debug.Log("check");
        stream = client.GetStream();
        Debug.Log("check");
        byte[] data = Encoding.ASCII.GetBytes(message);

        Debug.Log("sending username");
        stream.Write(data, 0, data.Length);

        Debug.Log("getting version");
        string version = Read(1024);
        if(version != Application.version)
        {
            Write("d");
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
            Debug.Log(i);
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
        string mapprop = "";

        for (int i = 0; i < worldProportionsLoad[1]; i++)
        {
            Write("requestchunk:" + i);
            Read(1024);
            Write("mapgrid");
            int buffer = System.Convert.ToInt32(Read(8192));
            Write("x");
            string mapGrid = Read(buffer);
            if (map == "")
                map = mapGrid;
            else
                map = string.Join(";", new string[] { map, mapGrid });


            Write("exit");
            Read(1024);
            //Write("tileprop");
            //buffer = System.Convert.ToInt32(Read(8192));
            //Write("x");
            //string mapTileProp = Read(buffer);
            //mapprop = string.Join(";", new string[] { mapprop, mapTileProp });


        }

        Write("exit");
        Read(1024);
        Write("wenas");
        worldMapLoad = ManagingFunctions.ConvertStringToIntArray(map.Split(';'));
        worldMapPropLoad = new string[worldMapLoad.Length];
        for (int i = 0; i < worldMapPropLoad.Length; i++)
        {
            worldMapPropLoad[i] = "null";
        }
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
        worldBiomesLoad = null;
    }
}

public class TcpUser
{
    public string username;
    public Vector4 position;
    public TcpClient tcpClient;

    public TcpUser(TcpClient client, string username)
    {
        tcpClient = client;
        this.username = username;
    }
}