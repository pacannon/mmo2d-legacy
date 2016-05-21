﻿using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mmo2d;
using Newtonsoft.Json;
using Mmo2d.ServerMessages;
using Mmo2d.AuthoritativePackets;

namespace Example
{
    class MyApplication
    {
        static IServer Server { get; set; }
        public static long? IssuedId { get; private set; }

        public static State State;

        [STAThread]
        public static void Main()
        {
            State = new State();
            DisplayLogin();
                
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }

                    ProcessServerData();
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

                    State.Render();

                    game.SwapBuffers();
                };

                game.KeyPress += Game_KeyPress;

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }

        private static void ProcessServerData()
        {
            while (Server.ResponseQueue.Count > 0)
            {
                AuthoritativePacket packet = null;

                bool dequeueSucceeded = false;

                do
                {
                    try
                    {
                        dequeueSucceeded = Server.ResponseQueue.TryDequeue(out packet);
                    }

                    catch (InvalidOperationException)
                    {
                        return;
                    }
                }

                while (!dequeueSucceeded);

                Console.WriteLine("Processing packet:");
                Console.WriteLine(packet.ToString());

                if (packet.IdIssuance != null)
                {
                    IssuedId = packet.IdIssuance;
                }

                if (packet.State != null)
                {
                    State = packet.State;
                }
            }
        }

        private static void Game_KeyPress(object sender, KeyPressEventArgs e)
        {
            BroadcastKeystroke(e.KeyChar);
        }

        public static void DisplayLogin()
        {
            var validInput = false;

            while (!validInput)
            {
                validInput = true;

                Console.WriteLine("Would you like to host a server?");
                Console.WriteLine("1) Yes");
                Console.WriteLine("2) No");

                var input = Console.ReadLine().ToLowerInvariant();

                switch (input)
                {
                    case "y":
                    case "yes":
                    case "1":
                        HostServer();
                        break;
                    case "n":
                    case "no":
                    case "2":
                        ConnectToServer();
                        break;
                    default:
                        validInput = false;
                        break;
                }
            }
        }

        private static void HostServer()
        {
            Server = new HostServer();
        }

        private static void ConnectToServer()
        {
            var validInput = false;

            IPAddress hostIp = IPAddress.None;
            String input = null;

            while (!validInput)
            {
                validInput = true;

                Console.WriteLine("Enter host IP:");

                input = Console.ReadLine();

                validInput = IPAddress.TryParse(input, out hostIp);

                if (!validInput)
                {
                    Console.WriteLine(@"Error: Invalid IP address: ""{0}""", input);
                }
            }

            Server = new ClientServer(input);
        }

        private static void BroadcastKeystroke(char c)
        {
            //server object - sends messages to server
            Server.SendMessage(new KeyPress() { TypedCharacter = c });
        }
    }
}