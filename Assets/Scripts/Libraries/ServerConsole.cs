using System;
using System.Threading;
using UnityEngine;

public class ServerConsole : MonoBehaviour
{
    public static ServerConsole serverConsole;
    private Thread consoleThread;
    public string[] inText;
    public bool gotOutput;
    public bool awaiting;
    public string output;

    private void Awake()
    {
        serverConsole = this;
    }

    public void StartConsole()
    {
        if (Application.isBatchMode)
        {
            // Start a new thread to handle console input
            consoleThread = new Thread(new ThreadStart(ConsoleInputThread));
            consoleThread.Start();
            Debug.Log("Server build: Console input thread started.");
        }
        else
        {
            Debug.Log("Not a server build: Console input not started.");
        }
    }

    public void SendEmbed(string[] embed)
    {
        inText = embed;
    }

    public bool GetOutput(out string output)
    {
        if (gotOutput)
        {
            gotOutput = false;
            output = this.output;
            return false;
        }
        else
        {
            output = "urReallyReadingThis? Seriously?";
            return false;
        }
    }

    void OnApplicationQuit()
    {
        if (consoleThread != null && consoleThread.IsAlive)
        {
            consoleThread.Abort();
            consoleThread.Join();
        }
    }

    private void ConsoleInputThread()
    {
        while (true)
        {
            if(inText != null)
            {
                string[] embed = (string[])inText.Clone();
                inText = null;

                foreach(string embedS in embed)
                {
                    Console.WriteLine(embedS);
                }
            }

            awaiting = true;
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                ProcessCommand(input);
            }
            awaiting = false;

            Console.WriteLine("Awaiting command read...");
            while (gotOutput)
            {

            }
            Console.WriteLine("Command readed.");
        }
    }

    private void ProcessCommand(string command)
    {
        // Process your commands here
        switch (command.ToLower())
        {
            case "exit":
                if (Server.isOpen)
                    Server.CloseServer();
                Application.Quit();
                break;
            default:
                output = command.ToLower();
                break;
        }
    }
}