namespace RayTr.Core
{
    public class Scene
    {
        public List<Sphere> Spheres { get; set; }
        public List<LightSource> Lights { get; set; }

        public Scene()
        {
            Spheres = new List<Sphere>();
            Lights = new List<LightSource>();
        }

        public (bool intersects, double distance, Vector3 normal, Sphere sphere) FindClosestIntersection(Ray ray)
        {
            bool hasIntersection = false;
            double closestDistance = double.MaxValue;
            Vector3 closestNormal = new Vector3(0, 0, 0);
            Sphere closestSphere = null;

            foreach (var sphere in Spheres)
            {
                var intersection = sphere.Intersect(ray);
                if (intersection.intersects && intersection.distance < closestDistance)
                {
                    hasIntersection = true;
                    closestDistance = intersection.distance;
                    closestNormal = intersection.normal;
                    closestSphere = sphere;
                }
            }

            return (hasIntersection, closestDistance, closestNormal, closestSphere);
        }

        public bool IsInShadow(Vector3 point, LightSource light, Sphere excludedSphere = null)
        {
            Vector3 lightDirection = (light.Position - point).Normalized();

            Vector3 shadowRayOrigin = point + lightDirection * 0.001;
            Ray shadowRay = new Ray(shadowRayOrigin, lightDirection);

            foreach (var sphere in Spheres)
            {
                if (excludedSphere != null && sphere == excludedSphere)
                    continue;

                var intersection = sphere.Intersect(shadowRay);
                if (intersection.intersects && intersection.distance > 0)
                {
                    double distanceToLight = (light.Position - point).Length();
                    if (intersection.distance < distanceToLight)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
