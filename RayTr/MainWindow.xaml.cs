using Microsoft.Win32;
using RayTr.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace RayTr
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap currentBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int width = 640;
                int height = 480;

                currentBitmap = new WriteableBitmap(
                    width, height, 96, 96, PixelFormats.Bgr32, null);

                Vector3 sphere1Center = new Vector3(
                    ParseDouble(Sphere1X.Text),
                    ParseDouble(Sphere1Y.Text),
                    ParseDouble(Sphere1Z.Text)
                );
                double sphere1Radius = ParseDouble(Sphere1Radius.Text);
                ColorRGB sphere1Color = new ColorRGB(
                    ParseDouble(Sphere1R.Text),
                    ParseDouble(Sphere1G.Text),
                    ParseDouble(Sphere1B.Text)
                );

                Vector3 sphere2Center = new Vector3(
                    ParseDouble(Sphere2X.Text),
                    ParseDouble(Sphere2Y.Text),
                    ParseDouble(Sphere2Z.Text)
                );
                double sphere2Radius = ParseDouble(Sphere2Radius.Text);
                ColorRGB sphere2Color = new ColorRGB(
                    ParseDouble(Sphere2R.Text),
                    ParseDouble(Sphere2G.Text),
                    ParseDouble(Sphere2B.Text)
                );

                List<Sphere> spheres = new List<Sphere>
                {
                    new Sphere(sphere1Center, sphere1Radius, sphere1Color),
                    new Sphere(sphere2Center, sphere2Radius, sphere2Color)
                };

                Vector3 lightPosition = new Vector3(
                    ParseDouble(LightX.Text),
                    ParseDouble(LightY.Text),
                    ParseDouble(LightZ.Text)
                );

                LightSource light = new LightSource(lightPosition, ColorRGB.White);

                Vector3 cameraPosition = new Vector3(
                    ParseDouble(CameraX.Text),
                    ParseDouble(CameraY.Text),
                    ParseDouble(CameraZ.Text)
                );

                SimpleCamera camera = new SimpleCamera(
                    cameraPosition,
                    width, height,
                    60.0
                );

                RenderScene(currentBitmap, camera, spheres, light);

                RenderImage.Source = currentBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при рендеринге: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentBitmap == null)
            {
                MessageBox.Show("Сначала выполните рендеринг", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PNG Image|*.png";
                saveDialog.Title = "Сохранить изображение";
                saveDialog.FileName = "raytracing_result.png";

                if (saveDialog.ShowDialog() == true)
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(currentBitmap));

                    using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    MessageBox.Show($"Изображение сохранено: {saveDialog.FileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double ParseDouble(string text)
        {
            if (double.TryParse(text, out double result))
            {
                return result;
            }
            throw new ArgumentException($"Некорректное числовое значение: {text}");
        }

        private void RenderScene(WriteableBitmap bitmap, SimpleCamera camera, List<Sphere> spheres, LightSource light)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            byte[] pixelData = new byte[width * height * 4];

            int pixelIndex = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Ray ray = camera.GenerateRay(x, y);

                    (bool intersects, double distance, Vector3 normal, Sphere sphere) =
                        FindClosestIntersection(ray, spheres);

                    ColorRGB pixelColor;

                    if (intersects)
                    {
                        Vector3 intersectionPoint = ray.PointAt(distance);

                        bool inShadow = IsPointInShadow(intersectionPoint, normal, light, spheres, sphere);

                        if (inShadow)
                        {
                            pixelColor = ColorRGB.Black;
                        }
                        else
                        {
                            Vector3 lightDirection = (light.Position - intersectionPoint).Normalized();

                            double cosTheta = Vector3.Dot(normal, lightDirection);

                            double intensity = Math.Max(0.0, cosTheta);

                            pixelColor = sphere.Color * light.Color * intensity;
                        }
                    }
                    else
                    {
                        pixelColor = new ColorRGB(0.2, 0.2, 0.8);
                    }

                    pixelData[pixelIndex] = (byte)(pixelColor.B * 255);
                    pixelData[pixelIndex + 1] = (byte)(pixelColor.G * 255);
                    pixelData[pixelIndex + 2] = (byte)(pixelColor.R * 255);
                    pixelData[pixelIndex + 3] = 0;

                    pixelIndex += 4;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);
        }

        private (bool intersects, double distance, Vector3 normal, Sphere sphere)
            FindClosestIntersection(Ray ray, List<Sphere> spheres)
        {
            bool intersects = false;
            double closestDistance = double.MaxValue;
            Vector3 closestNormal = new Vector3();
            Sphere closestSphere = null;

            foreach (Sphere sphere in spheres)
            {
                var intersection = sphere.Intersect(ray);
                if (intersection.intersects && intersection.distance < closestDistance)
                {
                    intersects = true;
                    closestDistance = intersection.distance;
                    closestNormal = intersection.normal;
                    closestSphere = sphere;
                }
            }

            return (intersects, closestDistance, closestNormal, closestSphere);
        }

        private bool IsPointInShadow(Vector3 point, Vector3 normal, LightSource light, List<Sphere> spheres, Sphere currentSphere)
        {
            Vector3 lightDirection = (light.Position - point).Normalized();

            Vector3 shadowRayOrigin = point + normal * 0.001;

            Ray shadowRay = new Ray(shadowRayOrigin, lightDirection);

            foreach (Sphere sphere in spheres)
            {
                if (sphere == currentSphere) continue;

                var intersection = sphere.Intersect(shadowRay);
                if (intersection.intersects)
                {
                    return true;
                }
            }

            return false;
        }
    }
}