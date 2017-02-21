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
    
        public static string FontName = "Times New Roman";
    }

    public class Ui
    {
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int FontTextureId { get; set; }
        public const float ButtonSize = 37.0f;
        public readonly Vector2 ButtonLocation = new Vector2(50, 26);

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

            GL.LoadIdentity();
            GL.Ortho(0, game.Width, 0, game.Height, 0, 1);

            DrawText(20, game.Height - 40, playerEntity.Kills.ToString(), false);

            //var mouseState = game.Mouse;

            //SpriteSheet.Ui[25][9].Render(new Vector2(20 + size/2, game.Height - 20 - size/2), ButtonSize, ButtonSize);


            SpriteSheet.Ui[13][3].Render(ButtonLocation, ButtonSize * 2.0f, ButtonSize * 2.0f);
            SpriteSheet.Ui[13][4].Render(ButtonLocation + new Vector2(ButtonSize * 3, 0), ButtonSize * 2.0f, ButtonSize * 2.0f);
            SpriteSheet.Ui[13][4].Render(ButtonLocation + new Vector2(ButtonSize * 2, 0), ButtonSize * 2.0f, ButtonSize * 2.0f);
            SpriteSheet.Ui[13][5].Render(ButtonLocation + new Vector2(ButtonSize * 5, 0), ButtonSize * 2.0f, ButtonSize * 2.0f);
            
            //GL.Color3(Color.DimGray);
            SpriteSheet.Fireball[0][0].Render(ButtonLocation, ButtonSize, ButtonSize);
            SpriteSheet.Frostbolt[0][0].Render(ButtonLocation + new Vector2(ButtonSize * 1 + 5, 0), ButtonSize, ButtonSize);
            SpriteSheet.FrostNova[0][0].Render(ButtonLocation + new Vector2(ButtonSize * 2 + 10, 0), ButtonSize, ButtonSize);
            SpriteSheet.Blink[0][0].Render(ButtonLocation + new Vector2(ButtonSize * 3 + 15, 0), ButtonSize, ButtonSize);
            SpriteSheet.Poly[0][0].Render(ButtonLocation + new Vector2(ButtonSize * 4 + 20, 0), ButtonSize, ButtonSize);

            GL.Color3(Color.Transparent);
            DrawText(58, 26, "1", false);
            DrawText(58 + (int)(ButtonSize * 1 + 5), 26, "2", false);
            DrawText(58 + (int)(ButtonSize * 2 + 10), 26, "3", false);
            DrawText(58 + (int)(ButtonSize * 3 + 15), 26, "4", false);
            DrawText(58 + (int)(ButtonSize * 4 + 20), 26, "5", false);
        }

        public void DrawText(int x, int y, string text, bool black)
        {
            if (black)
            {
                GL.Color3(Color.Black);
            }

            else
            {
                DrawText(x - 1, y - 1, text, true);
                DrawText(x - 1, y + 1, text, true);
                DrawText(x + 1, y + 1, text, true);
                DrawText(x + 1, y - 1, text, true);

                GL.Color3(Color.Transparent);
            }

            GL.BindTexture(TextureTarget.Texture2D, FontTextureId);

            GL.Enable(EnableCap.Blend);

            GL.Begin(PrimitiveType.Quads);

            float u_step = (float)Settings.GlyphWidth / (float)TextureWidth;
            float v_step = (float)Settings.GlyphHeight / (float)TextureHeight;

            for (int n = 0; n < text.Length; n++)
            {
                char idx = text[n];
                float u = (float)(idx % Settings.GlyphsPerLine) * u_step;
                float v = (float)(idx / Settings.GlyphsPerLine) * v_step;

                GL.TexCoord2(u, v + v_step);
                GL.Vertex2(x, y);
                GL.TexCoord2(u + u_step, v + v_step);
                GL.Vertex2(x + Settings.GlyphWidth, y);
                GL.TexCoord2(u + u_step, v);
                GL.Vertex2(x + Settings.GlyphWidth, y + Settings.GlyphHeight);
                GL.TexCoord2(u, v);
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

 
