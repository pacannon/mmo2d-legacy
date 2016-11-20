using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Net;
using Mmo2d;
using Mmo2d.AuthoritativePackets;
using Mmo2d.UserCommands;
using System.Linq;
using Mmo2d.Textures;
using System.Collections.Generic;

namespace Example
{
    class MyApplication
    {
        static IServer Server { get; set; }
        static IServer AuthoritativeServer { get; set; }
        public static long? IssuedId { get; private set; }

        public static GameState GameState;
        public static Ui Ui;
        public static TextureLoader TextureLoader;
        
        [STAThread]
        public static void Main()
        {
            GameState = new GameState();
            DisplayLogin();
                
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;

                    TextureLoader = new TextureLoader();

                    Entity.CharacterTextureId = TextureLoader.LoadTexture(Mmo2d.Properties.Resources.roguelikeChar_transparent);

                    Ui = new Ui(TextureLoader);
                    GL.Enable(EnableCap.Texture2D);
                    GL.ClearColor(Color.ForestGreen);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                    /*GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);*/
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }

                    ProcessServerData(TimeSpan.FromSeconds(e.Time));
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();

                    Entity playerEntity = null;

                    if (GameState != null)
                    {
                        playerEntity = GameState.Entities.FirstOrDefault(en => en.Id == IssuedId);

                        if (playerEntity != null)
                        {
                            GL.Ortho(playerEntity.Location.X - 1.0, playerEntity.Location.X + 1.0, playerEntity.Location.Y - 1.0, playerEntity.Location.Y + 1.0, 0.0, 4.0);
                        }

                        GameState.Render();
                    }

                    Ui.Render(playerEntity, game.Width, game.Height);

                    game.SwapBuffers();
                };

                game.KeyDown += (sender, e) =>
                {
                    if (e.IsRepeat)
                    {
                        return;
                    }

                    Server.QueueUserCommand(new UserCommand() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = false, }, });
                };

                game.KeyUp += (sender, e) =>
                {
                    if (e.IsRepeat)
                    {
                        return;
                    }

                    Server.QueueUserCommand(new UserCommand() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = true, }, });
                };

                game.MouseDown += (sender, e) =>
                {
                    Server.QueueUserCommand(new UserCommand() { MousePressed = e.IsPressed, });
                };

                game.MouseUp += (sender, e) =>
                {
                    Server.QueueUserCommand(new UserCommand() { MousePressed = e.IsPressed, });
                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }

        private static void ProcessServerData(TimeSpan delta)
        {
            var gameStateDeltas = new List<GameStateDelta>();

            while (Server.AuthoritativePacketQueue.Count > 0)
            {
                AuthoritativePacket packet = null;

                bool dequeueSucceeded = false;

                do
                {
                    try
                    {
                        dequeueSucceeded = Server.AuthoritativePacketQueue.TryDequeue(out packet);
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

                if (packet.GameState != null)
                {
                    GameState = packet.GameState;
                }

                else if (packet.GameStateDelta != null)
                {
                    gameStateDeltas.Add(packet.GameStateDelta);
                }
            }

            GameState.ApplyUpdates(gameStateDeltas, delta);
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
            AuthoritativeServer = new HostServer();
            ConnectToServer("127.0.0.1");
        }

        private static void ConnectToServer(string hostIpString = null)
        {
            if (hostIpString == null)
            {
                var validInput = false;

                IPAddress hostIp = IPAddress.None;

                while (!validInput)
                {
                    validInput = true;

                    Console.WriteLine("Enter host IP:");

                    hostIpString = Console.ReadLine();

                    validInput = IPAddress.TryParse(hostIpString, out hostIp);

                    if (!validInput)
                    {
                        Console.WriteLine(@"Error: Invalid IP address: ""{0}""", hostIpString);
                    }
                }
            }

            Server = new ClientServer(hostIpString);
        }
    }
}