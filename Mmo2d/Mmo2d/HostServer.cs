using Lidgren.Network;
using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mmo2d
{
    class HostServer : IServer
    {
        public Task ServerResponseListener { get; set; }
        public CancellationTokenSource ServerResponseListenerCancellationTokenSource { get; set; }

        public ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; set; }

        public const int Port = 11000;
        public const string ApplicationIdentifier = "mmo2d";

        public State State { get; set; }

        public NetServer NetServer { get; set; }
        public Stopwatch Stopwatch { get; set; }

        public HostServer()
        {
            Stopwatch = Stopwatch.StartNew();
            State = new State { GoblinSpawner = new GoblinSpawner(Vector2.Zero) };

            NetPeerConfiguration config = new NetPeerConfiguration(ApplicationIdentifier);
            config.MaximumConnections = 100;
            config.Port = Port;
            NetServer = new NetServer(config);

            NetServer.Start();

            ResponseQueue = new ConcurrentQueue<AuthoritativePacket>();

            ServerResponseListenerCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = ServerResponseListenerCancellationTokenSource.Token;

            ServerResponseListener = new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    NetIncomingMessage im;

                    while (NetServer.MessageReceivedEvent.WaitOne())
                    {
                        im = NetServer.ReadMessage();

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

                                        State.Entities.Add(new Entity { Id = remoteUniqueIdentifier, EquippedSword = Entity.Sword });

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

                                    ServerUpdatePacket serverUpdatePacket = JsonSerializer.Deserialize<ServerUpdatePacket>(serializedServerUpdatePacket);
                                    serverUpdatePacket.PlayerId = im.SenderConnection.RemoteUniqueIdentifier;

                                    SendMessage(serverUpdatePacket);
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
                    var packet = new AuthoritativePacket { State = State.Clone(), };
                    SendToAllClients(packet);

                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / 30.0));
                }
            });

            pushStateTask.Start();


            var updateStateTask = new Task(() =>
            {
                TimeSpan lastElapsed = TimeSpan.Zero;

                while (true)
                {
                    var elapsed = TimeSpan.FromMilliseconds(Stopwatch.ElapsedMilliseconds);
                    var stateClone = State.Clone();

                    stateClone.Update(elapsed - lastElapsed);
                    lastElapsed = elapsed;
                    State = stateClone;
                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / 60.0));
                }
            });

            updateStateTask.Start();



            var randomInt = new Random().Next();

            State.Entities.Add(new Entity { Id = randomInt, EquippedSword = Entity.Sword });
            ResponseQueue.Enqueue(new AuthoritativePacket { IdIssuance = randomInt });
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

        public void SendMessage(ServerUpdatePacket message)
        {
            if (message.TypedCharacter != null)
            {
                var player = State.Entities.Where(e => e.Id == message.PlayerId).FirstOrDefault();

                player.InputHandler(message);
            }
        }
    }
}
