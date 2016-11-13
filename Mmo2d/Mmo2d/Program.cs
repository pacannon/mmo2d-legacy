using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Net;
using Mmo2d;
using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using System.Linq;
using Mmo2d.Textures;
using System.Collections.Generic;

namespace Example
{
    class MyApplication
    {
        static IServer Server { get; set; }
        public static long? IssuedId { get; private set; }
        
        public static Ui Ui;
        public static TextureLoader TextureLoader;

        public static SortedDictionary<uint, GameState> GameStates { get; set; }
        
        [STAThread]
        public static void Main()
        {
            GameStates = new SortedDictionary<uint, GameState>();

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

                    Entity playerEntity = null;

                    GameState gameState = null;
                    uint tickOffset = 6;

                    while (gameState == null && tickOffset < 100)
                    {
                        var tick = Server.Tick - tickOffset;

                        if (GameStates.ContainsKey(tick))
                        {
                            gameState = GameStates[tick];
                        }

                        tickOffset++;
                    }

                    if (gameState != null)
                    {
                        playerEntity = gameState.Entities.FirstOrDefault(en => en.Id == IssuedId);

                        if (playerEntity != null)
                        {
                            GL.Ortho(playerEntity.Location.X - 1.0, playerEntity.Location.X + 1.0, playerEntity.Location.Y - 1.0, playerEntity.Location.Y + 1.0, 0.0, 4.0);
                        }

                        gameState.Render();
                    }

                    Ui.Render(playerEntity, game.Width, game.Height);

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

                if (packet.GameState != null)
                {
                    GameStates[packet.GameState.Tick] = packet.GameState;

                    if (GameStates.Last().Key - GameStates.First().Key > 100)
                    {
                        GameStates.Remove(GameStates.First().Key);
                    }
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