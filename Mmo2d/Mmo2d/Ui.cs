using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.IO;
using Mmo2d.Textures;
using OpenTK;
using OpenTK.Input;
using Mmo2d.UserCommands;

namespace Mmo2d
{
    public static class Settings
    {
        public static int GlyphsPerLine = 16;
        public static int GlyphLineCount = 16;
        public static int GlyphWidth = 11;
        public static int GlyphHeight = 22;

        public static int CharXSpacing = 11;

        // Used to offset rendering glyphs to bitmap
        public static int AtlasOffsetX = -3, AtlassOffsetY = -1;
        public static int FontSize = 14;
        public static bool BitmapFont = false;
        public static string FromFile;// = @"D:\Users\Phillip\Documents\Kenney Game Assets (version 40)\Fonts\KenPixel Mini Square.ttf";
    
        public static string FontName = "Consolas";
    }

    public class Ui
    {
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int FontTextureId { get; set; }
        public const float ButtonSize = 50.0f;
        public readonly Vector2 ButtonLocation = new Vector2(26, 26);

        public Ui()
        {
            GenerateFontImage();
        }

        public void Render(Entity playerEntity, GameWindow game)
        {
            if (playerEntity == null)
            {
                return;
            }

            DrawText(10, 10, playerEntity.Kills.ToString(), game.Width, game.Height);

            //var mouseState = game.Mouse;

            //SpriteSheet.Ui[25][9].Render(new Vector2(20 + size/2, game.Height - 20 - size/2), ButtonSize, ButtonSize);


            SpriteSheet.Ui[24][14].Render(ButtonLocation, ButtonSize, ButtonSize);
        }

        public void DrawText(int x, int y, string text, int gameWidth, int gameHeight)
        {
            GL.BindTexture(TextureTarget.Texture2D, FontTextureId);

            GL.Enable(EnableCap.Blend);
            GL.LoadIdentity();
            GL.Ortho(0, gameWidth, 0, gameHeight, 0, 1);

            GL.Color3(Color.Transparent);

            GL.Begin(PrimitiveType.Quads);

            float u_step = (float)Settings.GlyphWidth / (float)TextureWidth;
            float v_step = (float)Settings.GlyphHeight / (float)TextureHeight;

            for (int n = 0; n < text.Length; n++)
            {
                char idx = text[n];
                float u = (float)(idx % Settings.GlyphsPerLine) * u_step;
                float v = (float)(idx / Settings.GlyphsPerLine) * v_step;

                GL.TexCoord2(u, v);
                GL.Vertex2(x, y);
                GL.TexCoord2(u + u_step, v);
                GL.Vertex2(x + Settings.GlyphWidth, y);
                GL.TexCoord2(u + u_step, v + v_step);
                GL.Vertex2(x + Settings.GlyphWidth, y + Settings.GlyphHeight);
                GL.TexCoord2(u, v + v_step);
                GL.Vertex2(x, y + Settings.GlyphHeight);

                x += Settings.CharXSpacing;
            }

            GL.End();
            GL.Disable(EnableCap.Blend);
        }

        private void GenerateFontImage()
        {
            int bitmapWidth = Settings.GlyphsPerLine * Settings.GlyphWidth;
            int bitmapHeight = Settings.GlyphLineCount * Settings.GlyphHeight;

            using (Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                Font font;
                if (!String.IsNullOrWhiteSpace(Settings.FromFile))
                {
                    var collection = new PrivateFontCollection();
                    collection.AddFontFile(Settings.FromFile);
                    var fontFamily = new FontFamily(Path.GetFileNameWithoutExtension(Settings.FromFile), collection);
                    font = new Font(fontFamily, Settings.FontSize);
                }
                else
                {
                    font = new Font(new FontFamily(Settings.FontName), Settings.FontSize);
                }

                using (var g = Graphics.FromImage(bitmap))
                {
                    if (Settings.BitmapFont)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    }
                    else
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        //g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    }

                    for (int p = 0; p < Settings.GlyphLineCount; p++)
                    {
                        for (int n = 0; n < Settings.GlyphsPerLine; n++)
                        {
                            char c = (char)(n + p * Settings.GlyphsPerLine);
                            g.DrawString(c.ToString(), font, Brushes.White,
                                n * Settings.GlyphWidth + Settings.AtlasOffsetX, p * Settings.GlyphHeight + Settings.AtlassOffsetY);
                        }
                    }
                }

                FontTextureId = TextureLoader.LoadTexture(bitmap);

                TextureWidth = bitmap.Width; TextureHeight = bitmap.Height;
            }
        }

        public  UserCommand HandleClick(MouseEventArgs e)
        {
            if (e.X <= ButtonLocation.X + ButtonSize / 2 && e.X >= ButtonLocation.X - ButtonSize / 2 &&
                e.Y <= ButtonLocation.Y + ButtonSize / 2 && e.Y >= ButtonLocation.Y - ButtonSize / 2)
            {
                return new UserCommand() { CastFireball = true, };
            }

            return null;
        }
    }
}

 
