using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Mmo2d.Textures
{
    public class SpriteSheet
    {
        public static SpriteSheet Ui { get; private set; }
        public static SpriteSheet Characters { get; private set; }
        public static SpriteSheet Fireball { get; private set; }
        public static SpriteSheet Frostbolt { get; private set; }
        public static SpriteSheet FrostNova { get; private set; }

        static SpriteSheet()
        {
            Characters = new SpriteSheet(Mmo2d.Properties.Resources.roguelikeChar_transparent, 16, 1);
            Ui = new SpriteSheet(Mmo2d.Properties.Resources.UIpackSheet_transparent, 16, 2);
            Fireball = new SpriteSheet(Mmo2d.Properties.Resources.fireball, 16, 0);
            Frostbolt = new SpriteSheet(Mmo2d.Properties.Resources.frostbolt, 16, 0);
            FrostNova = new SpriteSheet(Mmo2d.Properties.Resources.frost_nova, 16, 0);
        }

        public int TextureId { get; private set; }

        public float Margins { get; private set; }
        public float SpriteDimensions { get; private set; }
        public float SpriteHeight { get; private set; }

        public float SheetWidth { get; private set; }
        public float SheetHeight { get; private set; }

        public SpriteRow[] SpriteColumns { get; set; }

        private SpriteSheet(Bitmap bitmap, int spriteDim_px, int spriteMargin_px)
        {
            TextureId = TextureLoader.LoadTexture(bitmap);

            SheetWidth = bitmap.Width;
            SheetHeight = bitmap.Height;
            SpriteDimensions = spriteDim_px;
            Margins = spriteMargin_px;
        }

        public SpriteRow this[int row]
        {
            get
            {
                return new SpriteRow(this, row);
            }
        }
    }

    public class SpriteRow
    {
        public SpriteSheet SpriteSheet { get; private set; }
        public int Row { get; private set; }

        public SpriteRow(SpriteSheet spriteSheet, int row)
        {
            SpriteSheet = spriteSheet;
            Row = row;
        }

        public Sprite this[int column]
        {
            get
            {
                return new Sprite(SpriteSheet, Row, column);
            }
        }
    }

    public class Sprite
    {
        public SpriteSheet SpriteSheet { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }

        public Sprite(SpriteSheet spriteSheet, int row, int column)
        {
            SpriteSheet = spriteSheet;
            Row = row;

            Column = column;
        }

        public void Render(Vector2 location, float width, float heigth)
        {
            Render(location, width, heigth, 1.0f);
        }

        public void Render(Vector2 location, float width, float height, float percentFromLeft)
        {
            percentFromLeft = Math.Min(percentFromLeft, 1.0f);
            percentFromLeft = Math.Max(percentFromLeft, 0.0f);

            var halfWidth = Vector2.Multiply(Vector2.UnitX, width / 2);
            var halfHeight = Vector2.Multiply(Vector2.UnitY, height / 2);

            var topLeftCorner = location - halfWidth + halfHeight;

            GL.BindTexture(TextureTarget.Texture2D, SpriteSheet.TextureId);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(TopLeftCorner); GL.Vertex2(topLeftCorner);

            GL.TexCoord2(TopLeftCorner + Vector2.Multiply(TopRightCorner - TopLeftCorner, percentFromLeft)); GL.Vertex2(topLeftCorner + Vector2.Multiply(new Vector2(width, 0.0f), percentFromLeft));

            GL.TexCoord2(BottomLeftCorner + Vector2.Multiply(BottomRightCorner - BottomLeftCorner, percentFromLeft)); GL.Vertex2(topLeftCorner + Vector2.Multiply(new Vector2(width, 0.0f), percentFromLeft) - new Vector2(0.0f, height));

            GL.TexCoord2(BottomLeftCorner); GL.Vertex2(topLeftCorner - new Vector2(0.0f, height));

            GL.End();

            GL.Disable(EnableCap.Blend);
        }

        public Vector2 TopLeftCorner
        {
            get
            {
                return new Vector2(((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Column) / SpriteSheet.SheetWidth,
                                   ((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Row) / SpriteSheet.SheetHeight);
            }
        }

        public Vector2 TopRightCorner
        {
            get
            {
                return new Vector2(((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Column + SpriteSheet.SpriteDimensions) / SpriteSheet.SheetWidth,
                                   ((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Row) / SpriteSheet.SheetHeight);
            }
        }

        public Vector2 BottomRightCorner
        {
            get
            {
                return new Vector2(((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Column + SpriteSheet.SpriteDimensions) / SpriteSheet.SheetWidth,
                                   ((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Row + SpriteSheet.SpriteDimensions) / SpriteSheet.SheetHeight);
            }
        }

        public Vector2 BottomLeftCorner
        {
            get
            {
                return new Vector2(((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Column) / SpriteSheet.SheetWidth,
                                   ((SpriteSheet.SpriteDimensions + SpriteSheet.Margins) * Row + SpriteSheet.SpriteDimensions) / SpriteSheet.SheetHeight);
            }
        }

    }
}
