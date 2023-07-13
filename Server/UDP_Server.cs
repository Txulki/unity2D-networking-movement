using System.Collections;
using System.Collections.Generic;
using Networking;
using System.Net;
using System;
using ServerLib;
using shared_handler;
using System.Threading.Tasks;
using vector_library;

public class UDP_Server
{


    /*=============================
            MAIN LOOP
    ==============================*/

    private static async Task Main(String[] args)
    {
        // Start a task to read messages in the background
        Console.WriteLine("Starting server...");
        BackgroundRun ToRun = new BackgroundRun(); //BackgroundRun()
        UDP_Networking Network = new UDP_Networking(); //Setup the connection
        ServerHandler Handler = new ServerHandler(); //Setup the handler
        ToRun.setup(Handler, Network);

        int Port = 27015; //SERVER PORT VARIABLE
        Network.Main(Port);

        //Task readerTask = Task.Run(() => ToRun.read());*/
        await Task.Run(() => MainLoop(ToRun));
    }

    private static async Task MainLoop(BackgroundRun Background) //This MainLoop is constantly executed.
    {
        Console.WriteLine("Server started.");
        while (true)
        {
            Background.read(); //Check if there are any messages in the queue
            Background.TickAwaiting(); //For the Acknowledge check.
            Background.UpdateInfo(); //Send update packets periodically to the clients.

            //await Task.Run(() => Background.SecondLoop(Background));
            // Do other processing or wait for a specific time interval

            await Task.Delay(100); //Delay to execute MainLoop.
            //The TPS of the UpdateInfo function is right now of 10, but it can be changed by adding a specific countdown value for it like X = 10 and then X-- each time;
        }
    }
}

public class BackgroundRun
{
    //SERVER & CONNECTION INFO
    private UDP_Server _host;
    private UDP_Networking _server;

    //UTILITIES &  HANDLER
    private ServerHandler _handler;

    //CLIENTS CONNECTED INFO
    public List<Server_Client> server_Clients;

    //VARIABLES FOR THE MESSAGES
    private char separator_parts = '#', separator_identifier = ';', separator_message = ';';
    public int TimeID = 0; //TimeID



    public void setup(ServerHandler handler, UDP_Networking server)
    {
        _handler = handler;
        handler.SetupHandler(_host, this);
        _server = server;
        server_Clients = new List<Server_Client>();
    }
    public void read()
    {
        try
        {
            if (_server.read_msg() > 0) //Checks if there are new MSGs
            {
                for (int i = 0; i < _server.read_msg(); i++) //Loops trough all the messages
                {
                    string msg = _server.lee_diccionario();
                    Server_Read newRead = new Server_Read();
                    newRead.Setup(msg, _server, _handler, this, server_Clients);
                }
            }
        }
        catch (Exception e)
        {
            Console.Write("Server couldn't read message:" + e);
        }
    }

    public void TickAwaiting()
    {
        if (server_Clients.Count != 0)
        {
            foreach (Server_Client cl in server_Clients)
            {
                List<ConfirmationRequest> ToRemove = new List<ConfirmationRequest>();
                if (cl.AwaitingResponses.Count != 0)
                {
                    foreach (ConfirmationRequest Request in cl.AwaitingResponses)
                    {
                        Request.AmountsTicked++;

                        if (Request.AmountsTicked > 10)
                        {
                            ToRemove.Add(Request);
                        }
                        else
                        {
                            ResendMessage(Request.Message, cl);
                        }
                    }
                }

                while (ToRemove.Count > 0)
                {
                    cl.AwaitingResponses.Remove(ToRemove[0]);
                    ToRemove.Remove(ToRemove[0]);
                }
            }
        }
    }

    public void SendTo(string header, Server_Client target, bool ACK = false, bool MoveUpdate = false, params string[] parts)
    {
        // MSG1|MSG2|MSG3|MSG4...  --> Message 
        string Message = _handler.CompileList(separator_message, parts);

        //If we want to receive an acknowledgement.
        string Identifier = null;

        if(!MoveUpdate)
        {
            TimeID++;
            Identifier = _handler.CompileList(separator_identifier, "sv", TimeID.ToString(), header);
        }
        else
        {
            Identifier = _handler.CompileList(separator_identifier, "sv", target.LastMoveID.ToString(), header);
        }

        //Joins parts together.
        string ToSend = Identifier + separator_parts + Message;

        if (ACK)
        {
            string Confirmations = _handler.CompileList(separator_identifier, "1");
            //Joins parts together.
            ToSend = ToSend + separator_parts + Confirmations;
            target.AwaitConfirmation(ToSend, TimeID);
        }
        _server.Send(ToSend, target.IP, target.Port);
    }

    public void ResendMessage(string ToSend, Server_Client client)
    {
        _server.Send(ToSend, client.IP, client.Port);
    }

    public void UpdateInfo()
    {
            foreach (Server_Client client in server_Clients)
            {
                foreach (Server_Client other_client in server_Clients)
                {
                    SendTo("2", client, false, true, _handler.CompileUpdatePack(other_client));
                }
            }
    }
}



