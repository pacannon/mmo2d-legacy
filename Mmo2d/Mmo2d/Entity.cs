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
using Newtonsoft.Json;

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

        public Color? OverriddenColor { get; set; }

        public TimeSpan? TimeSinceAttack { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }

        public const float SwordLength = 0.4f;
        public static readonly Color GoblinColor = Color.Green;

        public int Hits { get; set; } = 0;

        public void Render()
        {
            //renders a tringle according to the position of the top left vertex of triangle
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(OverriddenColor ?? Color.White);
            GL.Vertex2(x - (width / 2.0), y + (height / 2.0));

            GL.Color3(OverriddenColor ?? Color.Blue);
            GL.Vertex2(x - (width / 2.0), y - (height / 2.0));

            GL.Color3(OverriddenColor ?? Color.Gray);
            GL.Vertex2(x + (width / 2.0), y - (height / 2.0));

            GL.Color3(OverriddenColor ?? Color.Orange);
            GL.Vertex2(x + (width / 2.0), y + (height / 2.0));

            if (TimeSinceAttack != null && TimeSinceAttack.Value < TimeSpan.FromMilliseconds(300))
            {
                GL.Color3(Color.Red);
                GL.Vertex2(x - SwordLength / 2.0, y + SwordLength / 2.0);
                GL.Vertex2(x - SwordLength / 2.0, y - SwordLength / 2.0);
                GL.Vertex2(x + SwordLength / 2.0, y - SwordLength / 2.0);
                GL.Vertex2(x + SwordLength / 2.0, y + SwordLength / 2.0);
            }

            GL.End();
        }

        public void InputHandler(ServerUpdatePacket message)
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
            else if (message.TypedCharacter == ' ')
            {
                TimeSinceAttack = TimeSpan.Zero;
            }
        }

        public void Update(TimeSpan delta, List<Entity> entitiesCopy)
        {
            if (TimeSinceAttack != null)
            {
                if (TimeSinceAttack == TimeSpan.Zero)
                {
                    foreach (var attackedEntity in entitiesCopy.Where(e => e.OverriddenColor == GoblinColor && Attacking(e)))
                    {
                        attackedEntity.Hits++;

                        if (attackedEntity.Hits >= 4)
                        {
                            attackedEntity.TimeSinceDeath = TimeSpan.Zero;
                        }
                    }
                }

                TimeSinceAttack += delta;

                if (TimeSinceAttack > TimeSpan.FromMilliseconds(1000))
                {
                    TimeSinceAttack = null;
                }
            }

            if (TimeSinceDeath != null)
            {
                TimeSinceDeath += delta;
            }
        }

        public bool Attacking(Entity entity)
        {
            if (TimeSinceAttack == TimeSpan.Zero)
            {
                if (Math.Abs(entity.x - x) < SwordLength &&
                    Math.Abs(entity.y - y) < SwordLength)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
