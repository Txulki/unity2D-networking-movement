using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System.Globalization;
using System.Net;
using UnityEngine;

namespace shared_handler
{
    public class Handler
    {

        /*===============================
              
        (SOME OF THESE FUNCTIONS ARE NOT USED, BUT THEY MAY BE USEFUL FOR YOU!)

           handler_CompileList(char separator, params string[] strings)
                --> Joins all the strings in strings and adds a separator in between
                each of the values.

            handler_StringVector(float[,] toString)
                --> This will make a string of type "(0.0o0.0)"
            
            handler_StringVector(Vector2 toString()
                --> This will also make a string as StringVector, but from a Vector2.

            handler_ListFromArray<T>(T[] array)
                --> This takes an Array and will return a List.

            handler_StringFromArray<T>(T[] list)
                --> Will return a comma separated string of that array.

            handler_ExtractVector(string input)
                --> Receives an input of format (0.0o0.0) and turns it into a float[1, 2].
            

        ===============================*/

        public string StringVector(float[,] toString)
        {
            return ("(" + toString[0, 0].ToString() + "o" + toString[0, 1].ToString() + ")").Replace(",", ".");
        }

        public string StringVector(Vector2 toString)
        {
            float[,] converted = new float[1, 2] { { toString.x, toString.y } };
            return ("(" + converted[0, 0].ToString() + "o" + converted[0, 1].ToString() + ")").Replace(",", ".");
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

        public List<T> ListFromArray<T>(T[] array)
        {
            List<T> ToReturn = new List<T>();
            for(int i = 0; i < array.Length; i++)
            {
                ToReturn.Add(array[i]);
            }
            return ToReturn;
        }

        public string StringFromArray<T>(T[] list, Type type)
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
            result = result.Replace("\"", "");
            string[] two = result.Split("o");
            float[,] Final = new float[1, 2] { { float.Parse(two[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(two[1], CultureInfo.InvariantCulture.NumberFormat) } };
            return Final;
        }
    }

    public class ConfirmationRequest
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

    public class MovementUnit
    {
        public float[,] TargetFloat;
        public int ID;
        public Vector2 Target;
    }

    public class Client_Instance //Instance for every other client in the server. It also manages the movement and other functions of said client's gameObject.
    {
        public string Tag;
        public Vector2 Pos;

        public GameObject Instance;
        public List<MovementUnit> PastMovements;

        public MovementUnit InterpolatingTo;

        public void Setup_Client(string tag, GameObject Prefab)
        {
            PastMovements = new List<MovementUnit>();
            Tag = tag;
            Pos = Vector2.zero;
            Instance = Prefab;
        }

        public void SetPos(float[,] Position)
        {
            Pos = new Vector2(Position[0, 0], Position[0, 1]);
        }

        public void InterpolateMovement()
        {
            if(InterpolatingTo != null)
            {

                Pos = Vector2.MoveTowards(Pos, InterpolatingTo.Target, 5 * Time.fixedDeltaTime);
                Instance.transform.position = Pos;
            }
        }

    }
}