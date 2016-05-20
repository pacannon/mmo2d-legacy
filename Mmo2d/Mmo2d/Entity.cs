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
using Mmo2d.ServerMessages;

namespace Mmo2d
{
    class Entity
    {
        //position of the top left vertex of the triangle (-0.1f,0.1f) = starting position
        
        public float x = -0.1f;
        public float y = 0.1f;
        //object size
        public float width = 0.2f;
        public float height = 0.2f;

        public Guid Id { get; private set; }

        public Entity(Guid id)
        {
            Id = id;
        }

        public void Render()
        {
            //renders a tringle according to the position of the top left vertex of triangle
            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(Color.Blue);
            GL.Vertex2(x, y);
            GL.Vertex2(x + width, y);
            GL.Vertex2((x + 0.1f), y - height);

            GL.End();
        }

        public void InputHandler(KeyPress message)
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
