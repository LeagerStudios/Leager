using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class MultiplayerPanelController : MonoBehaviour
{
    private const int ServerDiscoveryPort = 7777;
    [SerializeField] CanvasGroup panel;
    [SerializeField] RectTransform hostedPanel;
    [SerializeField] GameObject gamePrefab;
    [SerializeField] InputField ip;
    [SerializeField] InputField port;
    [SerializeField] InputField user;
    [SerializeField] Button connectButton;
    [SerializeField] GameObject warn1;
    [SerializeField] GameObject warn2;

    public static bool nameInUse = false;

    public List<string> hostedWorlds = new List<string>();
    public List<string> hostedWorldsName = new List<string>();
    public bool updateList = false;
    public Thread currentHosting;


    void Start()
    {
        Debug.Log("LocalIP: " + NetworkController.GetLocalIP());
        RefreshMultiplayerHosts();
        nameInUse = false;
    }

    void Update()
    {
        if (Camera.main.GetComponent<MainCameraController>().Focus == "multiplayer")
        {
            panel.alpha += 3 * Time.deltaTime;

            warn1.SetActive(nameInUse);
            connectButton.interactable = ip.text != "" && port.text != "" && user.text != "";
            warn2.SetActive(!connectButton.interactable);
        }
        else
        {
            panel.alpha -= 3 * Time.deltaTime;
        }

        if (updateList)
        {
            updateList = false;
            foreach(string server in hostedWorlds)
            {
                for(int i = 2; i < hostedPanel.childCount; i++)
                {
                    Destroy(hostedPanel.GetChild(i).gameObject);
                }

                for(int i = 0; i < hostedWorlds.Count; i++)
                {
                    GameObject panel = Instantiate(gamePrefab, hostedPanel);
                    panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15 + i * -30);
                    int val = i;
                    panel.GetComponent<Button>().onClick.AddListener(() => FocusElement(val));
                    panel.transform.GetChild(0).GetComponent<Text>().text = hostedWorldsName[i];
                }
            }
        }
    }

    public void FocusElement(int i)
    {
        Debug.Log(i);
        ip.text = hostedWorlds[i];
        port.text = 7777 + "";
    }

    public void RefreshMultiplayerHosts()
    {
        if (currentHosting != null)
        {
            currentHosting.Abort();
            currentHosting = null;
        }

        updateList = true;

        hostedWorlds = new List<string>();
        hostedWorldsName = new List<string>();
        ThreadStart task = new ThreadStart(SearchForMultiplayerHosts);
        Thread hostSearcher = new Thread(task);
        hostSearcher.Start();
        currentHosting = hostSearcher;
    }


    public void SearchForMultiplayerHosts()
    {
        try
        {
            IPAddress broadcastAddress = IPAddress.Parse("255.255.255.255"); // Broadcasting to all devices in the LAN

            // Create a UDP socket
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true
            };

            // Message to send
            string messageToSend = "Quiero COMPRAR TU SERVIDOR DE LEAGER!!!";

            // Convert the message string to a byte array
            byte[] messageBytes = Encoding.ASCII.GetBytes(messageToSend);

            // Create an endpoint for broadcasting
            IPEndPoint endPoint = new IPEndPoint(broadcastAddress, ServerDiscoveryPort);

            // Send the data through the socket
            udpSocket.SendTo(messageBytes, endPoint);

            Debug.Log($"Sent message to all devices in the LAN on port {ServerDiscoveryPort}");

            byte[] receiveBuffer = new byte[1024];
            EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Debug.Log("Waiting for responses...");

            while (true)
            {
                int bytesRead = udpSocket.ReceiveFrom(receiveBuffer, ref receiveEndPoint);
                string receivedMessage = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                string[] msg = receivedMessage.Split('#');

                hostedWorlds.Add(msg[0]);
                hostedWorldsName.Add(msg[1]);
                updateList = true;
            }
        }
        catch(ThreadAbortException)
        {

        }
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
