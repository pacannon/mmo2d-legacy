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
using Mmo2d.Controller;

namespace Example
{
    class MyApplication
    {
        static IServer Server { get; set; }
        static IServer AuthoritativeServer { get; set; }
        public static long? IssuedId { get; private set; }

        public static GameState GameState;
        public static Ui Ui;
       
        public static float CameraWidth { get; set; }
        public static float CameraHeight { get; set; }

        [STAThread]
        public static void Main()
        {
            CameraHeight = CameraWidth = 9.0f;

            GameState = new GameState(null);
            DisplayLogin();

            var playerController = new EntityController();

            using (var game = new GameWindow(750, 750))
            {
                //game.CursorVisible = false;

                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                    
                    Ui = new Ui();
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
                            GL.Ortho(playerEntity.Location.X - CameraWidth / 2.0, playerEntity.Location.X + CameraWidth / 2.0, playerEntity.Location.Y - CameraHeight / 2.0, playerEntity.Location.Y + CameraHeight / 2.0, 0.0, 4.0);
                        }

                        GameState.Render(playerController);
                    }

                    Ui.Render(playerEntity, game);

                    game.SwapBuffers();
                };

                game.KeyDown += (sender, e) =>
                {
                    if (e.IsRepeat)
                    {
                        return;
                    }

                    var userCommand = new UserCommand() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = false, }, };

                    var stateChange = playerController.ApplyUserCommand(userCommand);

                    if (stateChange != null)
                    {
                        playerController.ChangeState(stateChange);
                        Server.QueueUserCommand(userCommand);
                    }
                };

                game.KeyUp += (sender, e) =>
                {
                    if (e.IsRepeat)
                    {
                        return;
                    }

                    var userCommand = new UserCommand() { KeyEventArgs = new KeyEventArgs { Key = e.Key, KeyUp = true }, };

                    var stateChange = playerController.ApplyUserCommand(userCommand);

                    if (stateChange != null)
                    {
                        playerController.ChangeState(stateChange);
                        Server.QueueUserCommand(userCommand);
                    }
                };

                game.MouseDown += (sender, e) =>
                {
                    var userCommand = Ui.HandleClick(new MouseEventArgs(e.X, game.Height - e.Y));

                    if (userCommand == null)
                    {
                        var bottomLeftOfScreen = CameraBottomLeft();
                        var clicked = new Vector2 { X = ((float)e.Position.X / (float)game.Width) * CameraWidth, Y = CameraHeight - ((float)e.Position.Y / (float)game.Height) * CameraHeight };

                        var setTargetTo = GameState.TargetId(clicked + bottomLeftOfScreen);

                        userCommand = new UserCommand() { SetTargetId = setTargetTo, };

                        if (setTargetTo == null && playerController[EntityController.States.TargetId].LongVal.HasValue)
                        {
                            userCommand.DeselectTarget = true;
                        }
                    }

                    var stateChange = playerController.ApplyUserCommand(userCommand);

                    if (stateChange != null)
                    {
                        playerController.ChangeState(stateChange);
                        Server.QueueUserCommand(userCommand);
                    }
                };

                game.MouseUp += (sender, e) =>
                {
                    //playerController = playerController.ApplyUserCommand(userCommand);

                    //Server.QueueUserCommand(userCommand);
                };

                game.MouseWheel += (sender, e) =>
                {
                    CameraWidth = (CameraHeight -= e.Delta);
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
 
        public static Vector2 CameraCentered()
        {
            if (GameState != null)
            {
                var playerEntity = GameState.Entities.FirstOrDefault(en => en.Id == IssuedId);

                if (playerEntity != null)
                {
                    return new Vector2(playerEntity.Location.X, playerEntity.Location.Y);
                }
            }

            return Vector2.Zero;
        }

        public static Vector2 CameraBottomLeft()
        {
            if (GameState != null)
            {
                var playerEntity = GameState.Entities.FirstOrDefault(en => en.Id == IssuedId);

                if (playerEntity != null)
                {
                    return new Vector2(playerEntity.Location.X - CameraWidth / 2.0f, playerEntity.Location.Y - CameraHeight / 2.0f);
                }
            }

            return Vector2.Zero;
        }
    }
}