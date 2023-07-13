using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using ServerLib;
using System.Globalization;
using System.Net;
using Defective;

namespace shared_handler
{
    public class ServerHandler
    {

        UDP_Server main; //UDP Server is the starter
        BackgroundRun back; //BackgroundRun is basically the main async loop.

        public void SetupHandler(UDP_Server Server, BackgroundRun Background) //simple setup of the variables.
        {
            main = Server;
            back = Background;
        }

        /*===============================
              
        (SOME OF THESE FUNCTIONS ARE NOT USED, BUT THEY MAY BE USEFUL FOR YOU!)

           handler_ClientByTag(string search_Tag) 
                --> tries to find a client that matches with the given Tag. 
                returns null or the client instance.

           handler_ClientByAddr(int search_Port, IPAddress search_IP) 
                --> tries to find a client that matches with the given port and IP.
                useful when you don't have the username or perhaps to check if there is
                another client connected from the same port and IP.
                returns null or the client instance.

           handler_CompileList(char separator, params string[] strings)
                --> Joins all the strings in strings and adds a separator in between
                each of the values.

            handler_CompileClient(Server_Client client, bool Player = false)
                --> If Player = true, it would pack a client with more information if needed,
                for example some private info that other clients in the game should not get.
                Returns a Defective.JSON

            handler_CompileUpdatePack(Server_Client client)
                --> This will prepare a JSON message with both the TAG and the POS 
                Used for the movement.

            handler_StringVector(float[,] toString)
                --> This will make a string of type "(0.0o0.0)"

            handler_ListFromArray<T>(T[] array)
                --> This takes an Array and will return a List.

            handler_StringFromArray<T>(T[] list)
                --> Will return a comma separated string of that array.

            handler_ExtractVector(string input)
                --> Receives an input of format (0.0o0.0) and turns it into a float[1, 2].
            

        ===============================*/

        public Server_Client ClientByTag(string search_Tag) 
        {
            foreach(Server_Client cl in back.server_Clients) //Loop trough all connected clients
            {
                if (cl.Tag == search_Tag) //Tag match
                {
                    return cl;
                }
            }
            return null;
        }

        public Server_Client ClientByAddr(int search_Port, IPAddress search_IP)
        {
            foreach (Server_Client cl in back.server_Clients) //Loop trough all connected clients
            {
                if (cl.IP.ToString() == search_IP.ToString() && cl.Port == search_Port) //Address matches.
                {
                    return cl;
                }
            }
            return null;
        }

        public string CompileList(char separator, params string[] strings)
        {
            try
            {
                string toReturn = "";
                for (int i = 0; i < strings.Length; i++)
                {
                    if(i == strings.Length-1)
                    {
                        toReturn = toReturn + strings[i];
                    }
                    else
                    {
                        toReturn = toReturn + strings[i] + separator;
                    }
                }
                return toReturn;
            }
            catch (Exception e)
            {
                Console.Write("Error at handler_CompileList " + e);
                return null;
            }
        }

        public string CompileClient(Server_Client client, bool Player = false)
        {
            if(Player) //THE PLAYER REPLYING TO
            {
                Defective.JSON.JSONObject PlayerJson = new Defective.JSON.JSONObject();
                PlayerJson.AddField("tag", client.Tag);
                return PlayerJson.ToString();
            }
            else //OTHER PLAYER
            { 
                Defective.JSON.JSONObject clientJSON = new Defective.JSON.JSONObject();
                clientJSON.AddField("Tag", client.Tag);
                return clientJSON.ToString();
            }
        }

        public string CompileUpdatePack(Server_Client client)
        {
            Defective.JSON.JSONObject UpdateJson = new Defective.JSON.JSONObject();
            UpdateJson.AddField("tag", client.Tag);
            UpdateJson.AddField("pos", StringVector(client.Pos));
            return UpdateJson.ToString();
        }

        public string StringVector(float[,] toString)
        {
            return ("(" + toString[0, 0].ToString() + "o" + toString[0, 1].ToString() + ")").Replace(",", ".");
        }

        public List<T> ListFromArray<T>(T[] array)
        {
            List<T> ToReturn = new List<T>();
            for(int i = 0; i < array.Length; i++)
            {
                ToReturn.Add(array[i]);
            }
            return ToReturn;
        }

        public string StringFromArray<T>(T[] list)
        {
            string result = "";

            for(int i = 0; i < list.Length; i++)
            {
                result = result + "," + list[i];
            }

            return result;
        }

        public float[,] ExtractVector(string input)
        {
            string result = input.Replace("(", "");
            result = result.Replace(")", "");
            string[] two = result.Split('o');
            float[,] Final = new float[1, 2] { { float.Parse(two[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(two[1], CultureInfo.InvariantCulture.NumberFormat) } };
            return Final;
        }
    }

    public class ConfirmationRequest //This is the class for an acknowledgement request. 
    {
        public string Message;
        public int TimeID;
        public int AmountsTicked;

        public void SetupRequest(string msg, int id)
        {
            Message = msg;
            TimeID = id;
        }
    }
}