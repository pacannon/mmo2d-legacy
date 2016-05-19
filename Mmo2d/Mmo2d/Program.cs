using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mmo2d;

namespace Example
{
    class MyApplication
    {
        static IServer Server { get; set; }
        static Entity Player { get; set; }


        [STAThread]
        public static void Main()
        {
            DisplayLogin();
            Player = new Entity();

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
                    try
                    {
                        while (Server.ResponseQueue.Count > 0)
                        {
                            ServerResponse serverResponse = null;

                            while (!Server.ResponseQueue.TryDequeue(out serverResponse))
                            { }

                            //inputs for entity control
                            Console.WriteLine(serverResponse.TypedCharacter);
                            Player.InputHandler(serverResponse);

                        }
                    }
                    catch (InvalidOperationException)
                    { }

                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

                    Player.Render();

                    game.SwapBuffers();
                };

                game.KeyPress += Game_KeyPress;

                // Run the game at 60 updates per second
                game.Run(60.0);
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
            Server = new LocalServer();
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

            Server = new RemoteServer(input);
        }

        static void Connect(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        private static void BroadcastKeystroke(char c)
        {
            //server object - sends messages to server
            Server.SendMessage(new ServerMessage { TypedCharacter = c });
        }
    }
}