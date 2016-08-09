using Lidgren.Network;
using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class ClientServer : IServer
    {
        public ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; private set; }

        public State State { get; set; }

        public NetClient NetClient { get; set; }

        public ClientServer(string hostIpAddress)
        {
            State = new State();

            ResponseQueue = new ConcurrentQueue<AuthoritativePacket>();


            NetPeerConfiguration config = new NetPeerConfiguration(HostServer.ApplicationIdentifier);
            config.AutoFlushSendQueue = false;
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
                        im = NetClient.ReadMessage();
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
                                //NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                                //if (status == NetConnectionStatus.Connected)
                                //    //s_form.EnableInput();
                                //else
                                //    //s_form.DisableInput();

                                //if (status == NetConnectionStatus.Disconnected)
                                //    //s_form.button2.Text = "Connect";

                                //string reason = im.ReadString();
                                //Console.WriteLine(status.ToString() + ": " + reason);

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

                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            t.Start();
        }

        public void SendMessage(ServerUpdatePacket message)
        {
            var serializedMessage = message.ToString();
            NetOutgoingMessage netOutGoindMessage = NetClient.CreateMessage(serializedMessage);
            NetClient.SendMessage(netOutGoindMessage, NetDeliveryMethod.ReliableSequenced);
            NetClient.FlushSendQueue();
        }
    }
}
