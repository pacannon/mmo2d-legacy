using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    class LocalServer : IServer
    {
        public TcpListener TcpListener { get; set; }
        public ConcurrentDictionary<NetworkStream, object> Streams { get; set; }

        public LocalServer()
        {
            ResponseQueue = new ConcurrentQueue<ServerResponse>();
            Streams = new ConcurrentDictionary<NetworkStream, object>();

            var TcpListenerTask = new Task(() =>
            {
                IPAddress localAddress = IPAddress.Parse("192.168.0.3");

                // TcpListener server = new TcpListener(port);
                TcpListener = new TcpListener(localAddress, RemoteServer.Port);

                // Start listening for client requests.
                TcpListener.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = TcpListener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    var task = new Task(() =>
                    {
                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();
                        Streams.AddOrUpdate(stream, new object(), (a, b) => { return new object(); });

                        int i;

                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            ResponseQueue.Enqueue(new ServerResponse() { TypedCharacter = data.ToCharArray()[0] });
                        }

                        object temp;

                        while (!Streams.TryRemove(stream, out temp)) { }

                        // Shutdown and end connection
                        client.Close();
                    });

                    task.ContinueWith(antecedent =>
                    {
                        if (antecedent.Exception != null)
                        {

                        }
                    });

                    task.Start();                    
                }
            });

            TcpListenerTask.ContinueWith(antecedent =>
            {
                if (antecedent.Exception != null)
                {
                    Console.WriteLine(antecedent.Exception);
                }
            });

            TcpListenerTask.Start(TaskScheduler.Default);
        }

        public ConcurrentQueue<ServerResponse> ResponseQueue { get; private set; }

        public void SendMessage(ServerMessage message)
        {
            var typedCharacter = message.TypedCharacter;

            foreach (var stream in Streams.Keys)
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(typedCharacter.ToString());

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
            }

            ResponseQueue.Enqueue(new ServerResponse { TypedCharacter = typedCharacter });
        }
    }
}
