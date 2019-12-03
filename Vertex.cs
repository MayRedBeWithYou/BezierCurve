using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public class Vertex
    {
        public Vertex(int x, int y)
        {
            Position = new Vector2(x, y);
        }

        public Vertex(Vector2 position)
        {
            Position = position;
        }

        public Vertex(Vector2 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public Vertex()
        {
        }

        public Vector2 Position { get; set; } = new Vector2(0, 0);

        public Point Point
        {
            get => new Point(X, Y);
            set
            {
                Position = new Vector2(value.X, value.Y);
            }
        }

        public Color Color { get; set; } = Color.Red;

        public int Size { get; set; } = 9;

        public int X
        {
            get => (int)Position.X;
        }

        public int Y
        {
            get => (int)Position.Y;
        }
    }
}
