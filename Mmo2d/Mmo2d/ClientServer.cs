using Lidgren.Network;
using Mmo2d.AuthoritativePackets;
using Mmo2d.UserCommands;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class ClientServer : IServer
    {
        public float CommandRate { get; set; }

        public ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; private set; }
        public CancellationTokenSource CommanPacketTaskCancellationTokenSource { get; set; }
        
        public ConcurrentQueue<UserCommand> QueuedUserCommands { get; set; }

        public GameState State { get; set; }

        public NetClient NetClient { get; set; }

        public Task CommandPacketSendTask { get; set; }

        public ClientServer(string hostIpAddress)
        {
            CommandRate = 30.0f;
            QueuedUserCommands = new ConcurrentQueue<UserCommand>();

            State = new GameState();

            ResponseQueue = new ConcurrentQueue<AuthoritativePacket>();


            NetPeerConfiguration config = new NetPeerConfiguration(HostServer.ApplicationIdentifier);
            config.AutoFlushSendQueue = false;
                        
            //config.SimulatedMinimumLatency = 0.2f;
            //config.SimulatedLoss = 0.5f;

            NetClient = new NetClient(config);

            var t = new Task(() =>
            {
                NetClient.Start();
                NetOutgoingMessage hail = NetClient.CreateMessage("This is the hail message");
                NetClient.Connect(hostIpAddress, HostServer.Port, hail);

                NetIncomingMessage im;

                try {

                    while (NetClient.MessageReceivedEvent.WaitOne())
                    {
                        while ((im = NetClient.ReadMessage()) != null)
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
                                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                                    if (status == NetConnectionStatus.Connected) { }
                                    //s_form.EnableInput();
                                    else
                                    //s_form.DisableInput();

                                    if (status == NetConnectionStatus.Disconnected) { }
                                    //s_form.button2.Text = "Connect";

                                    string reason = im.ReadString();
                                    Console.WriteLine(status.ToString() + ": " + reason);

                                    if (status == NetConnectionStatus.Connected)
                                    {
                                        StartCommandPacketTask();
                                    }

                                    break;
                                case NetIncomingMessageType.Data:
                                    string serializedAuthoritativePacket = im.ReadString();

                                    AuthoritativePacket authoritativePacket = JsonSerializer.Deserialize<AuthoritativePacket>(serializedAuthoritativePacket);

                                    ResponseQueue.Enqueue(authoritativePacket);
                                    break;
                                default:
                                    Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                                    break;
                            }
                            NetClient.Recycle(im);
                        }
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            t.Start();
        }

        private void StartCommandPacketTask()
        {
            CommanPacketTaskCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = CommanPacketTaskCancellationTokenSource.Token;

            CommandPacketSendTask = new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    while (QueuedUserCommands.Count > 0)
                    {
                        UserCommand userCommand = null;

                        bool dequeueSucceeded = false;

                        do
                        {
                            try
                            {
                                dequeueSucceeded = QueuedUserCommands.TryDequeue(out userCommand);
                            }

                            catch (InvalidOperationException)
                            {
                                return;
                            }
                        }

                        while (!dequeueSucceeded);

                        BundleUserCommand(userCommand);
                    }

                    NetClient.FlushSendQueue();

                    Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / CommandRate));
                }
            }, cancellationToken);

            CommandPacketSendTask.Start();
        }

        private void BundleUserCommand(UserCommand message)
        {
            var serializedMessage = message.ToString();
            NetOutgoingMessage netOutGoindMessage = NetClient.CreateMessage(serializedMessage);
            NetClient.SendMessage(netOutGoindMessage, NetDeliveryMethod.ReliableOrdered);

            //Console.WriteLine(serializedMessage);
        }

        public void QueueUserCommand(UserCommand userCommand)
        {
            QueuedUserCommands.Enqueue(userCommand);
        }
    }
}
