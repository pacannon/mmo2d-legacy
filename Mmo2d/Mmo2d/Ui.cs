using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;

namespace Mmo2d
{
    public class Ui
    {
        private Bitmap text_bmp;
        private int text_texture;

        public Ui(Bitmap text_bmp, int text_texture)
        {
            this.text_bmp = text_bmp;
            this.text_texture = text_texture;
        }

        public void Render(Entity playerEntity)
        {
            if (playerEntity == null)
            {
                return;
            }

            // Render text using System.Drawing.
            // Do this only when text changes.
            using (Graphics gfx = Graphics.FromImage(text_bmp))
            {
                gfx.Clear(Color.Transparent);
                gfx.DrawString(playerEntity.Kills.ToString(), new Font("Verdana", 13.0f, FontStyle.Regular, GraphicsUnit.Point), Brushes.White, new PointF() { X = 0, Y = 0}); // Draw as many strings as you need
            }

            // Upload the Bitmap to OpenGL.
            // Do this only when text changes.
            BitmapData data = text_bmp.LockBits(new Rectangle(0, 0, text_bmp.Width, text_bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, text_bmp.Width, text_bmp.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            text_bmp.UnlockBits(data);

            // Finally, render using a quad. 
            // Do this every frame.
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, text_bmp.Width, text_bmp.Height, 0, -1, 1);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);   

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0f, 1f); GL.Vertex2(0f, 0f);
            GL.TexCoord2(1f, 1f); GL.Vertex2(1f, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0f, 1f);
            GL.End();
        }
    }
}

 
