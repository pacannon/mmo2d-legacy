using Lidgren.Network;
using Mmo2d.AuthoritativePackets;
using Mmo2d.UserCommands;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mmo2d
{
    class HostServer : IServer
    {
        public Task ServerResponseListener { get; set; }
        public CancellationTokenSource ServerResponseListenerCancellationTokenSource { get; set; }

        public ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; set; }
        public ConcurrentQueue<UserCommand> UserCommandQueue { get; set; }

        public const int Port = 11000;
        public const string ApplicationIdentifier = "mmo2d";

        public GameState GameState { get; set; }
        public GameState GameStateClone { get; set; }

        public NetServer NetServer { get; set; }

        public TimeSpan Tickrate { get; set; }

        public HostServer()
        {
            Tickrate = TimeSpan.FromMilliseconds(15.0);
            GameState = new GameState { GoblinSpawner = new GoblinSpawner(Vector2.Zero) };

            NetPeerConfiguration config = new NetPeerConfiguration(ApplicationIdentifier);
            config.MaximumConnections = 100;
            config.Port = Port;
            NetServer = new NetServer(config);

            NetServer.Start();

            ResponseQueue = new ConcurrentQueue<AuthoritativePacket>();
            UserCommandQueue = new ConcurrentQueue<UserCommand>();

            ServerResponseListenerCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = ServerResponseListenerCancellationTokenSource.Token;

            ServerResponseListener = new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    NetIncomingMessage im;

                    while ((im = NetServer.ReadMessage()) != null)
                    {
                        // handle incoming message
                        switch (im.MessageType)
                        {
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.ErrorMessage:
                            case NetIncomingMessageType.WarningMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                string text = im.ReadString();
                                Console.WriteLine(text);
                                break;

                            case NetIncomingMessageType.StatusChanged:
                                {
                                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                                    string reason = im.ReadString();
                                    Console.WriteLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

                                    if (status == NetConnectionStatus.Connected)
                                    {
                                        Console.WriteLine("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());

                                        //UpdateConnectionsList();
                                        var remoteUniqueIdentifier = im.SenderConnection.RemoteUniqueIdentifier;

                                        GameState.Entities.Add(new Entity { Id = remoteUniqueIdentifier, SwordEquipped = true });

                                        NetOutgoingMessage om = NetServer.CreateMessage();
                                        var authoritativePacket = new AuthoritativePacket() { IdIssuance = remoteUniqueIdentifier, };

                                        om.Write(authoritativePacket.ToString());
                                        NetServer.SendMessage(om, im.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                    }
                                }
                                break;
                            case NetIncomingMessageType.Data:
                                {
                                    // incoming chat message from a client
                                    string serializedServerUpdatePacket = im.ReadString();

                                    UserCommand serverUpdatePacket = JsonSerializer.Deserialize<UserCommand>(serializedServerUpdatePacket);
                                    serverUpdatePacket.PlayerId = im.SenderConnection.RemoteUniqueIdentifier;

                                    QueueUserCommand(serverUpdatePacket);
                                }
                                break;
                            default:
                                Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                                break;
                        }
                        NetServer.Recycle(im);
                    }
                }
            }, cancellationToken);

            ServerResponseListener.Start();


            var pushStateTask = new Task(() =>
            {
                while (true)
                {
                    var packet = new AuthoritativePacket { State = GameStateClone, };
                    SendToAllClients(packet);

                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / 30.0));
                }
            });

            pushStateTask.Start();


            var updateStateTask = new Task(() =>
            {
                TimeSpan lastElapsed = TimeSpan.Zero;
                var stopwatch = Stopwatch.StartNew();

                while (true)
                {
                    var awakened = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

                    while (UserCommandQueue.Count > 0)
                    {
                        UserCommand userCommand = null;

                        bool dequeueSucceeded = false;

                        do
                        {
                            try
                            {
                                dequeueSucceeded = UserCommandQueue.TryDequeue(out userCommand);
                            }

                            catch (InvalidOperationException)
                            {
                                return;
                            }
                        }

                        while (!dequeueSucceeded);

                        var playerEntity = GameState.Entities.Where(e => e.Id == userCommand.PlayerId).FirstOrDefault();

                        if (playerEntity != null)
                        {
                            playerEntity.InputHandler(userCommand);
                        }
                    }

                    var elapsed = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

                    GameState.Update(elapsed - lastElapsed);
                    lastElapsed = elapsed;

                    GameStateClone = GameState.Clone();

                    var asleepend = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

                    var sleepFor = Tickrate - (asleepend - awakened);

                    Thread.Sleep(sleepFor.TotalMilliseconds > 0 ? sleepFor : TimeSpan.Zero);
                }
            });

            updateStateTask.ContinueWith(antecedent =>
            {
                if (antecedent.Exception != null)
                {

                }
            });

            updateStateTask.Start();
        }

        private void SendToAllClients(AuthoritativePacket packet)
        {
            List<NetConnection> all = NetServer.Connections; // get copy

            if (all.Count > 0)
            {
                NetOutgoingMessage om = NetServer.CreateMessage();

                var serializedPacket = packet.ToString();
                om.Write(serializedPacket);
                NetServer.SendMessage(om, all, NetDeliveryMethod.UnreliableSequenced, 0);
            }

            ResponseQueue.Enqueue(packet);
        }

        public void QueueUserCommand(UserCommand userCommand)
        {
            UserCommandQueue.Enqueue(userCommand);
        }
    }
}
