using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using Mmo2d.ServerUpdatePackets;

namespace Mmo2d
{
    public class Entity
    {
        //position of the top left vertex of the triangle (-0.1f,0.1f) = starting position
        
        public float x = 0.0f;
        public float y = 0.0f;
        //object size
        public float width = 0.2f;
        public float height = 0.2f;

        public long Id { get; set; }

        public void Render()
        {
            //renders a tringle according to the position of the top left vertex of triangle
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(Color.White);
            GL.Vertex2(x - (width / 2.0), y + (height / 2.0));

            GL.Color3(Color.Blue);
            GL.Vertex2(x - (width / 2.0), y - (height / 2.0));

            GL.Color3(Color.Gray);
            GL.Vertex2(x + (width / 2.0), y - (height / 2.0));

            GL.Color3(Color.Orange);
            GL.Vertex2(x + (width / 2.0), y + (height / 2.0));

            GL.End();
        }

        internal void InputHandler(ServerUpdatePacket message)
        {
            if (message.TypedCharacter == 'w')
            {
                y += 0.1f;
            }
            else if (message.TypedCharacter == 's')
            {
                y -= 0.1f;
            }
            else if (message.TypedCharacter == 'd')
            {
                x += 0.1f;
            }
            else if (message.TypedCharacter == 'a')
            {
                x -= 0.1f;
            }
        }
    }
}
