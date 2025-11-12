using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTr.Core
{
    public class LightSource
    {
        public Vector3 Position { get; }
        public ColorRGB Color { get; }

        public LightSource(Vector3 position, ColorRGB color)
        {
            Position = position;
            Color = color;
        }

        public override string ToString()
        {
            return $"Light(Position: {Position}, Color: {Color})";
        }
    }
}
