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

        public ConcurrentQueue<AuthoritativePacket> AuthoritativePacketQueue { get; set; }
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
            var random = new Random();
            GameState = new GameState(random) { GoblinSpawner = new GoblinSpawner(Vector2.Zero, random) };

            NetPeerConfiguration config = new NetPeerConfiguration(ApplicationIdentifier);
            config.MaximumConnections = 100;
            config.Port = Port;
            NetServer = new NetServer(config);

            NetServer.Start();

            AuthoritativePacketQueue = new ConcurrentQueue<AuthoritativePacket>();
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

                                        UserCommandQueue.Enqueue(new UserCommand { CreateEntity = new Entity { Id = remoteUniqueIdentifier, SwordEquipped = true, Hp = 10, } });

                                        NetOutgoingMessage om = NetServer.CreateMessage();
                                        var authoritativePacket = new AuthoritativePacket() { IdIssuance = remoteUniqueIdentifier, };

                                        var packet = new AuthoritativePacket { GameState = GameStateClone, };

                                        om.Write(authoritativePacket.ToString());
                                        om.Write(packet.ToString());
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
                    while (AuthoritativePacketQueue.Count > 0)
                    {
                        AuthoritativePacket authoritativePacket = null;

                        bool dequeueSucceeded = false;

                        do
                        {
                            try
                            {
                                dequeueSucceeded = AuthoritativePacketQueue.TryDequeue(out authoritativePacket);
                            }

                            catch (InvalidOperationException)
                            {
                                return;
                            }
                        }

                        while (!dequeueSucceeded);

                        SendToAllClients(authoritativePacket);
                    }

                    NetServer.FlushSendQueue();

                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / 30.0));
                }
            });

            pushStateTask.Start();


            var updateStateTask = new Task(() =>
            {
                TimeSpan lastElapsed = TimeSpan.Zero;
                var stopwatch = Stopwatch.StartNew();

                int sent = 0;

                while (true)
                {
                    var awakened = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
                    var entitiesCreated = new List<Entity>();

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

                        if (userCommand.CreateEntity != null)
                        {
                            entitiesCreated.Add(userCommand.CreateEntity);
                        }
                    }

                    var elapsed = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

                    var update = GameState.GenerateUpdate(elapsed - lastElapsed);
                    update.EntitiesToAdd.AddRange(entitiesCreated);
                    GameState.ApplyUpdates(new[] { update }, elapsed - lastElapsed);

                    lastElapsed = elapsed;

                    GameStateClone = GameState.Clone();

                    if (update.ContainsInformation)
                    {
                        sent++;
                        AuthoritativePacketQueue.Enqueue(new AuthoritativePacket { GameStateDelta = update, GameState = (sent % 200 == 0 ? GameStateClone : null), });
                    }

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
                NetServer.SendMessage(om, all, NetDeliveryMethod.ReliableOrdered, 0);
            }
        }

        public void QueueUserCommand(UserCommand userCommand)
        {
            UserCommandQueue.Enqueue(userCommand);
        }
    }
}
