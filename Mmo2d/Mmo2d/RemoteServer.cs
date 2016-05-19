using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mmo2d
{
    class RemoteServer : IServer, IDisposable
    {
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public Task ServerResponseListener { get; set; }
        public CancellationTokenSource ServerResponseListenerCancellationTokenSource { get; set; }

        public const int Port = 11000;

        public RemoteServer(string ipAddress)
        {
            TcpClient = new TcpClient(ipAddress, Port);
            NetworkStream = TcpClient.GetStream();

            ResponseQueue = new ConcurrentQueue<ServerResponse>();

            ServerResponseListenerCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = ServerResponseListenerCancellationTokenSource.Token;

            ServerResponseListener = new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();
                    int numberOfBytesRead = 0;

                    // Incoming message may be larger than the buffer size.
                    do
                    {
                        numberOfBytesRead = NetworkStream.Read(myReadBuffer, 0, myReadBuffer.Length);

                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                    }
                    while (NetworkStream.DataAvailable);

                    var firstCharacterOfMessage = myCompleteMessage.ToString().ToCharArray()[0];

                    ResponseQueue.Enqueue(new ServerResponse { TypedCharacter = firstCharacterOfMessage });
                }
            }, cancellationToken);
        }

        public ConcurrentQueue<ServerResponse> ResponseQueue { get; set; }

        public void SendMessage(ServerMessage message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message.TypedCharacter.ToString());
            NetworkStream.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            ServerResponseListenerCancellationTokenSource.Cancel();
            NetworkStream.Close();
            TcpClient.Close();
        }
    }
}
