using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace Networking
{
    public class UDP_Networking
    {

        UdpClient socket;
        bool recibir = true;
        List<string> cola_recibida = new List<string>();
        public void Stop()
        {
            recibir = false;
            if (socket != null)
            {
                socket.Close();
            }
        }

        public string ToMD5(string message)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.ASCII.GetBytes(message);
                byte[] hash = md5.ComputeHash(input);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        //lee el primer valor de los recibidos y lo borra
        public string lee_diccionario()
        {
            string volver = cola_recibida[0];
            cola_recibida.RemoveAt(0);
            return volver;

           
        }
        //hay mensajes para leer?
        public int read_msg()
        {

            return cola_recibida.Count;

        }

        //Cuando recibo
        void OnUdpData(IAsyncResult result)
        {
            // this is what had been passed into BeginReceive as the second parameter:
            UdpClient socket = result.AsyncState as UdpClient;

            // points towards whoever had sent the message:
            IPEndPoint source = new IPEndPoint(0, 0);

            // get the actual message and fill out the source:
            byte[] message = socket.EndReceive(result, ref source);

            // do what you'd like with `message` here:
            //Debug.Log("Got " + message.Length + " bytes from " + source);

            //MENSAJES!!
            cola_recibida.Add(source.ToString() + "#" + Encoding.UTF8.GetString(message));


            // schedule the next receive operation once reading is done:
            //Seguir recibiendo
            if (recibir == true)
            {
                socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            }
        }

        //levanto el socket
        public void Main(int port_listen)
        {
            //int port = 9002;
            try
            {
                //Debug.Log("Listening on port " + port_listen);
                socket = new UdpClient(port_listen);
            }
            catch (Exception e)
            {
                Console.Write("Failed to listen for UDP at port " + port_listen + ": " + e.Message);
                return;
            }

            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);

        }

        public void Send(string msg, IPAddress dest_ip, int port_dest)
        {
            // sending data (for the sake of simplicity, back to ourselves):
            //IPEndPoint target = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9002);

            IPEndPoint target = new IPEndPoint(dest_ip, port_dest);

            byte[] message = Encoding.UTF8.GetBytes(msg);
            socket.Send(message, message.Length, target);


        }
    }
}
