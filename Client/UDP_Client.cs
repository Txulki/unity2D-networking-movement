using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using System.Net;
using System;
using System.Text;
using shared_handler;
using ClientLib;

public class UDP_Client : MonoBehaviour
{
    /*========================
          SERVER INFO!
    ========================*/

    [SerializeField] private int sv_Port; //Port of the server
    [SerializeField] private string serverIP; //IP of the server
    private IPAddress sv_IP; //Converted the IP to an IPAddress object.

    /*==================================
        CLIENT-SIDE CONNECTION INFO!
    ==================================*/
    [SerializeField] private int client_Port; //Port the client will be using.

    /*========================
       SERVER VARIABLES
   ========================*/
    public List<int> LastIDs; 

    /*========================
        CLIENT VARIABLES
    ========================*/
    public string Tag; //Tag or Username 
    public PlayerController Controller;

    public List<MovementUnit> MovementHistory;

    /*==================================
       CONNECTION VALUES (NO TOUCH)
    ==================================*/
    private UDP_Networking _connection; //Connection
    public int TimeID; //MESSAGE ORDER ID
    public char separator_parts = '#', separator_identifier = ';', separator_message = ';'; //Characters for message structures.
    public List<ConfirmationRequest> AwaitingResponses; //List of AwaitingResponses.
    private int TickDown = 60; //TIMER VARIABLE

    /*========================
       UTILITIES (HANDLER)
    ========================*/
    private Handler _handler; //Handler for some common operations.

    /*========================
       OTHER USERS INFO
   ========================*/
    public List<Client_Instance> server_Clients; //List with the other clients stored.
    public GameObject Instance_Prefab; //Prefab for the object a client would use.

    
    
    
    /*========================
        SETUP SETUP SETUP
    ========================*/
    private void Awake()
    {
        MovementHistory = new List<MovementUnit>();

        //SET FRAMERATE TO 60fps SO THAT GAME DOESNT BURN MY PC.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //Prepare the handler, the server IP to IPAddress format and the list of other clients connected to the server.
        _handler = new Handler();
        sv_IP = IPAddress.Parse(serverIP);
        server_Clients = new List<Client_Instance>();

        //Setup LastIDs of Server.
        LastIDs = new List<int>();
        for(int i = 0; i < 5; i++)
        {
            LastIDs.Add(-1);
        }
        //Setup Awaiting Responses
        AwaitingResponses = new List<ConfirmationRequest>();
    }

    private void Start()
    {
        connection_create(); //Create connection;
    }

    private void connection_create()
    {
        _connection = new UDP_Networking(); //Creates new connection
        _connection.Main(client_Port); //Starts listening in client_Port port

        Send("0", true, "Join");
    }

    /*=======================
        READING LOOP!
    =======================*/

    private void FixedUpdate()
    {
        read();
        TickAwaiting();
        InterpolateCycle();
    }

    private void read()
    {
        try
        {
            if (_connection.read_msg() > 0) //Checks if there are new MSGs
            {
                for (int i = 0; i < _connection.read_msg(); i++) //Loops trough all the messages
                {
                    string msg = _connection.lee_diccionario();
                    Client_Read newRead = new Client_Read();
                    newRead.Setup(msg, _connection, _handler, this);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Server couldn't read message:" + e);
        }
    }

    /*=======================
     SEND SEND SEND SEND SEND
    =======================*/

    public void Send(string header, bool ACK = false, params string[] parts)
    {
        TimeID++;

        // LAYERID; TIMEID; HEADER-- > Identifiers
        string Identifier = _handler.CompileList(separator_identifier, Tag, TimeID.ToString(), header);

        // MSG1|MSG2|MSG3|MSG4...  --> Message 
        string Message = _handler.CompileList(separator_message, parts);

        //Identifier = _handler.CompileList(separator_identifier, main.cl_Tag, main.TimeID.ToString(), header);

        //Joins parts together.
        string ToSend = Identifier + separator_parts + Message;

        if (ACK)
        {
            ConfirmationRequest newRequest = new ConfirmationRequest();

            string Confirmation = _handler.CompileList(separator_identifier, "1");
            ToSend = ToSend + separator_parts + Confirmation;

            newRequest.SetupRequest(ToSend, TimeID);
            AwaitingResponses.Add(newRequest);
        }

        //print(ToSend);
        _connection.Send(ToSend, sv_IP, sv_Port);
    }

    public void ResendMessage(string ToSend)
    {
        //print(ToSend + " RESENT");
        _connection.Send(ToSend, sv_IP, sv_Port);
    }

    public void SendACK(int rc_TimeID)
    {
        string Message = _handler.CompileList(separator_message, "ACK");
        string Identifier = _handler.CompileList(separator_identifier, Tag, rc_TimeID.ToString(), "1");

        string ToSend = Identifier + separator_parts + Message;

        //Debug.Log(ToSend);
        ResendMessage(ToSend);
    }

    /*==============================
     REFRESH AWAITING CONFIRMATIONS
    ==============================*/

    private void TickAwaiting()
    {
        TickDown--;
        if (TickDown <= 0)
        {
            TickDown = 50;

            if (AwaitingResponses.Count == 0)
            {
                return;
            }

            List<ConfirmationRequest> ToRemove = new List<ConfirmationRequest>();
            foreach (ConfirmationRequest Request in AwaitingResponses)
            {
                Request.AmountsTicked++;

                if (Request.AmountsTicked > 10)
                {
                    ToRemove.Add(Request);
                }
                else
                {
                    ResendMessage(Request.Message);
                }
            }

            while (ToRemove.Count > 0)
            {
                AwaitingResponses.Remove(ToRemove[0]);
                ToRemove.Remove(ToRemove[0]);
            }
        }
    }

    /*==============================
    SEND INPUTS TO SERVER (MOVEMENT)
    ==============================*/

    public void SendMoveInput(int X, int Y)
    {

        float[,] Input = new float[1, 2] { { X, Y } };

        MovementUnit movement = new MovementUnit();
        movement.ID = TimeID + 1;
        movement.TargetFloat = Input;
        MovementHistory.Add(movement);
        Send("2", false, _handler.StringVector(Input));
    }

    /*==============================
       INTERPOLATION OF MOVEMENT
    ==============================*/

    public void InterpolateCycle()
    {
        foreach(Client_Instance cl in server_Clients)
        {
            cl.InterpolateMovement();
        }
    }
}
