using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTr.Core
{
    public class SimpleCamera
    {
        public Vector3 Position { get; set; }
        public int Width { get; }
        public int Height { get; }
        public double Fov { get; set; }

        private double aspectRatio;
        private double scale;

        public SimpleCamera(Vector3 position, int width, int height, double fov)
        {
            Position = position;
            Width = width;
            Height = height;
            Fov = fov;

            aspectRatio = (double)width / height;
            scale = Math.Tan(fov * 0.5 * Math.PI / 180.0);
        }

        public Ray GenerateRay(int x, int y)
        {
            double px = (2.0 * (x + 0.5) / Width - 1.0) * aspectRatio * scale;
            double py = (1.0 - 2.0 * (y + 0.5) / Height) * scale;

            Vector3 direction = new Vector3(px, py, 1).Normalized();

            return new Ray(Position, direction);
        }
    }
}
