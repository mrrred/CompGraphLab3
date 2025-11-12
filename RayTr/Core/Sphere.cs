using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTr.Core
{
    public class Sphere
    {
        public Vector3 Center { get; }
        public double Radius { get; }
        public ColorRGB Color { get; }

        public Sphere(Vector3 center, double radius, ColorRGB color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }

        public (bool intersects, double distance, Vector3 normal) Intersect(Ray ray)
        {
            Vector3 oc = ray.Origin - Center;

            double a = Vector3.Dot(ray.Direction, ray.Direction);
            double b = 2.0 * Vector3.Dot(oc, ray.Direction);
            double c = Vector3.Dot(oc, oc) - Radius * Radius;

            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return (false, 0, new Vector3(0, 0, 0));
            }

            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtDiscriminant) / (2 * a);
            double t2 = (-b + sqrtDiscriminant) / (2 * a);

            double distance = 0;
            if (t1 > 0.001)
            {
                distance = t1;
            }
            else if (t2 > 0.001)
            {
                distance = t2;
            }
            else
            {
                return (false, 0, new Vector3(0, 0, 0));
            }

            Vector3 intersectionPoint = ray.PointAt(distance);
            Vector3 normal = (intersectionPoint - Center).Normalized();

            return (true, distance, normal);
        }

        public override string ToString()
        {
            return $"Sphere(Center: {Center}, Radius: {Radius:F2}, Color: {Color})";
        }
    }
}
