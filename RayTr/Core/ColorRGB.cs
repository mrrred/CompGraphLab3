namespace RayTr.Core
{
    public struct ColorRGB
    {
        public double R { get; }

        public double G { get; }

        public double B { get; }


        public ColorRGB(double r, double g, double b)
        {
            R = Math.Max(0, Math.Min(1, r));
            G = Math.Max(0, Math.Min(1, g));
            B = Math.Max(0, Math.Min(1, b));
        }

        public static ColorRGB White => new ColorRGB(1, 1, 1);
        public static ColorRGB Black => new ColorRGB(0, 0, 0);
        public static ColorRGB Red => new ColorRGB(1, 0, 0);
        public static ColorRGB Green => new ColorRGB(0, 1, 0);
        public static ColorRGB Blue => new ColorRGB(0, 0, 1);
        public static ColorRGB Gray => new ColorRGB(0.5, 0.5, 0.5);

        public static ColorRGB operator *(ColorRGB a, ColorRGB b)
        {
            return new ColorRGB(a.R * b.R, a.G * b.G, a.B * b.B);
        }

        public static ColorRGB operator *(ColorRGB color, double scalar)
        {
            return new ColorRGB(color.R * scalar, color.G * scalar, color.B * scalar);
        }

        public static ColorRGB operator *(double scalar, ColorRGB color)
        {
            return color * scalar;
        }

        public static ColorRGB operator +(ColorRGB a, ColorRGB b)
        {
            return new ColorRGB(a.R + b.R, a.G + b.G, a.B + b.B);
        }

        public System.Windows.Media.Color ToMediaColor()
        {
            return System.Windows.Media.Color.FromRgb(
                (byte)(R * 255),
                (byte)(G * 255),
                (byte)(B * 255)
            );
        }

        public override string ToString()
        {
            return $"R: {R:F2}, G: {G:F2}, B: {B:F2}";
        }
    }
}