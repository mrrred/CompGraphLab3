using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTr.Core
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
        }

        public Vector3 PointAt(double t)
        {
            return Origin + Direction * t;
        }

        public override string ToString()
        {
            return $"Origin: {Origin}, Direction: {Direction}";
        }
    }
}
