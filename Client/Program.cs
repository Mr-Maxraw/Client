using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Client
    {
        public Socket _socket { get; set; }
        public ReceivePacket Receive { get; set; }
        public int Id { get; set; }

        public Client(Socket socket, int id)
        {
            Receive = new ReceivePacket(socket, id);
            Receive.StartReceiving();
            _socket = socket;
            Id = id;
        }
        public static void SetClient(Socket socket)
        {
            Id = 1;
            Socket = socket;
            Receive = new ReceivePacket(socket, Id);
            SendPacket = new SendPacket(socket);
        }
    }

    public class ReceivePacket
    {
        private byte[] _buffer;
        private Socket _receiveSocket;
        private int _clientId;

        public ReceivePacket(Socket receiveSocket, int id)
        {
            _receiveSocket = receiveSocket;
            _clientId = id;
        }

        public void StartReceiving()
        {
            try
            {
                _buffer = new byte[4];
                _receiveSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                // if bytes are less than 1 takes place when a client disconnect from the server.
                // So we run the Disconnect function on the current client
                if (_receiveSocket.EndReceive(AR) > 1)
                {
                    // Convert the first 4 bytes (int 32) that we received and convert it to an Int32 (this is the size for the coming data).
                    _buffer = new byte[BitConverter.ToInt32(_buffer, 0)];
                    // Next receive this data into the buffer with size that we did receive before
                    _receiveSocket.Receive(_buffer, _buffer.Length, SocketFlags.None);
                    // When we received everything its onto you to convert it into the data that you've send.
                    // For example string, int etc... in this example I only use the implementation for sending and receiving a string.

                    // Convert the bytes to string and output it in a message box
                    string data = Encoding.Default.GetString(_buffer);
                    Console.WriteLine(data);
                    // Now we have to start all over again with waiting for a data to come from the socket.
                    StartReceiving();
                }
                else
                {
                    Disconnect();
                }
            }
            catch
            {
                // if exeption is throw check if socket is connected because than you can startreive again else Dissconect
                if (!_receiveSocket.Connected)
                {
                    Disconnect();
                }
                else
                {
                    StartReceiving();
                }
            }
        }

        private void Disconnect()
        {
            // Close connection
            _receiveSocket.Disconnect(true);
            // Next line only apply for the server side receive
            //ClientController.RemoveClient(_clientId);
            // Next line only apply on the Client Side receive
            //Here you want to run the method TryToConnect()
        }
    }
    class Connector
    {
        private Socket _connectingSocket;

        public void TryToConnect()
        {
            _connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (!_connectingSocket.Connected)
            {
                Thread.Sleep(1000);

                try
                {
                    _connectingSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
                }
                catch { }
            }
            SetupForReceiveing();
        }

        private void SetupForReceiveing()
        {
            // View Client Class bottom of Client Example
            Client.SetClient(_connectingSocket);
            Client.StartReceiving();
        }
    }

    public class SendPacket
    {
        private Socket _sendSocked;

        public SendPacket(Socket sendSocket)
        {
            _sendSocked = sendSocket;
        }

        public void Send(string data)
        {
            try
            {
                /* what hapends here:
                     1. Create a list of bytes
                     2. Add the length of the string to the list.
                        So if this message arrives at the server we can easily read the length of the coming message.
                     3. Add the message(string) bytes
                */

                var fullPacket = new List<byte>();
                fullPacket.AddRange(BitConverter.GetBytes(data.Length));
                fullPacket.AddRange(Encoding.Default.GetBytes(data));

                /* Send the message to the server we are currently connected to.
                Or package stucture is {length of data 4 bytes (int32), actual data}*/
                _sendSocked.Send(fullPacket.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
