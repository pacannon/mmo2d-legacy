using Mmo2d.ServerMessages;
using Mmo2d.ServerResponses;
using Newtonsoft.Json;
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
        public ConcurrentQueue<IServerResponse> ResponseQueue { get; private set; }

        public State State { get; set; }

        public LocalServer()
        {
            State = new State();

            var ip = GetLocalIPAddress();

            ResponseQueue = new ConcurrentQueue<IServerResponse>();
            Streams = new ConcurrentDictionary<NetworkStream, object>();

            var TcpListenerTask = new Task(() =>
            {
                IPAddress localAddress = IPAddress.Parse(ip);

                // TcpListener server = new TcpListener(port);
                TcpListener = new TcpListener(localAddress, RemoteServer.Port);

                // Start listening for client requests.
                TcpListener.Start();

                Console.Clear();
                Console.WriteLine("Your IP address is: {0}", ip);

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

                        var idIssuance = new IdIssuance { Id = Guid.NewGuid()};

                        string json = JsonConvert.SerializeObject(idIssuance, Formatting.Indented);

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(json);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);

                        int i;

                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            var message = JsonConvert.DeserializeObject<IServerMessage>(data);
                            SendMessage(message);
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

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public void SendMessage(IServerMessage message)
        {
            //HandleMessage(message);
        }
    }
}
