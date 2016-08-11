using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Net;
using Mmo2d;
using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using System.Linq;
using System.Drawing.Imaging;
using System.Reflection;
using System.IO;
using Mmo2d.Textures;

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

                    GL.Enable(EnableCap.Texture2D);

                    var tl = new TextureLoader();
                    var id = tl.LoadTexture("Mmo2d.Resources.roguelikeChar_transparent.png");
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

                    /*var keyboardState = Keyboard.GetState();

                    PlayerInput.Update(keyboardState, )

                    if (state[Key.Up])
                        ; // move up
                    if (state[Key.Down])
                        ; // move down*/

                    ProcessServerData();
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();

                    if (State != null)
                    {
                        var playerEntity = State.Entities.FirstOrDefault(en => en.Id == IssuedId);

                        if (playerEntity != null)
                        {
                            GL.Ortho(playerEntity.Location.X - 1.0, playerEntity.Location.X + 1.0, playerEntity.Location.Y - 1.0, playerEntity.Location.Y + 1.0, 0.0, 4.0);
                        }

                        State.Render();
                    }

                    game.SwapBuffers();
                };

                game.KeyDown += (sender, e) =>
                {
                    Server.SendMessage(new ServerUpdatePacket() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = false, IsRepeat = e.IsRepeat, }, PlayerId = IssuedId, });
                };

                game.KeyUp += (sender, e) =>
                {
                    Server.SendMessage(new ServerUpdatePacket() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = true, IsRepeat = e.IsRepeat,}, PlayerId = IssuedId, });
                };

                game.MouseDown += (sender, e) =>
                {
                    Server.SendMessage(new ServerUpdatePacket() { MousePressed = e.IsPressed, PlayerId = IssuedId, });
                };

                game.MouseUp += (sender, e) =>
                {
                    Server.SendMessage(new ServerUpdatePacket() { MousePressed = e.IsPressed, PlayerId = IssuedId, });
                };

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

                if (packet.IdIssuance != null)
                {
                    IssuedId = packet.IdIssuance;
                }

                if (packet.State != null)
                {
                    State = packet.State;
                }

                if (packet != null)
                {
                    State = packet.State;
                }
            }
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
    }
}