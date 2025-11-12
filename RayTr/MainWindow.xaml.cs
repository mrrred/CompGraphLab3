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
                // Параметры изображения
                int width = 640;
                int height = 480;

                // Создаем bitmap для рендеринга
                currentBitmap = new WriteableBitmap(
                    width, height, 96, 96, PixelFormats.Bgr32, null);

                // Читаем параметры сферы 1 из интерфейса
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

                // Читаем параметры сферы 2 из интерфейса
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

                // Создаем сферы
                List<Sphere> spheres = new List<Sphere>
                {
                    new Sphere(sphere1Center, sphere1Radius, sphere1Color),
                    new Sphere(sphere2Center, sphere2Radius, sphere2Color)
                };

                // Читаем параметры источника света из интерфейса
                Vector3 lightPosition = new Vector3(
                    ParseDouble(LightX.Text),
                    ParseDouble(LightY.Text),
                    ParseDouble(LightZ.Text)
                );

                // Создаем источник света
                LightSource light = new LightSource(lightPosition, ColorRGB.White);

                // Читаем параметры камеры из интерфейса
                Vector3 cameraPosition = new Vector3(
                    ParseDouble(CameraX.Text),
                    ParseDouble(CameraY.Text),
                    ParseDouble(CameraZ.Text)
                );

                // Создаем простую камеру
                SimpleCamera camera = new SimpleCamera(
                    cameraPosition,    // Позиция камеры (из интерфейса)
                    width, height,     // Разрешение
                    60.0               // Угол обзора
                );

                // Рендерим сцену
                RenderScene(currentBitmap, camera, spheres, light);

                // Отображаем результат
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
                    // Создаем encoder для PNG
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(currentBitmap));

                    // Сохраняем в файл
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

            // Создаем массив пикселей (буфер вывода)
            byte[] pixelData = new byte[width * height * 4];

            int pixelIndex = 0;

            // Проходим по всем пикселям
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Генерируем луч для текущего пикселя
                    Ray ray = camera.GenerateRay(x, y);

                    // Ищем ближайшее пересечение с любой сферой
                    (bool intersects, double distance, Vector3 normal, Sphere sphere) =
                        FindClosestIntersection(ray, spheres);

                    ColorRGB pixelColor;

                    if (intersects)
                    {
                        // Вычисляем точку пересечения
                        Vector3 intersectionPoint = ray.PointAt(distance);

                        // Проверяем, находится ли точка в тени
                        bool inShadow = IsPointInShadow(intersectionPoint, normal, light, spheres, sphere);

                        if (inShadow)
                        {
                            // Точка в тени - ЧЕРНЫЙ ЦВЕТ
                            pixelColor = ColorRGB.Black;
                        }
                        else
                        {
                            // Вычисляем направление к источнику света
                            Vector3 lightDirection = (light.Position - intersectionPoint).Normalized();

                            // Вычисляем косинус угла между нормалью и направлением к свету
                            double cosTheta = Vector3.Dot(normal, lightDirection);

                            // Ограничиваем значение от 0 до 1
                            double intensity = Math.Max(0.0, cosTheta);

                            // Применяем модель освещения Ламберта
                            pixelColor = sphere.Color * light.Color * intensity;
                        }
                    }
                    else
                    {
                        // Если нет пересечения - синий фон
                        pixelColor = new ColorRGB(0.2, 0.2, 0.8);
                    }

                    // Записываем цвет пикселя в буфер вывода (формат Bgr32)
                    pixelData[pixelIndex] = (byte)(pixelColor.B * 255);     // Blue
                    pixelData[pixelIndex + 1] = (byte)(pixelColor.G * 255); // Green
                    pixelData[pixelIndex + 2] = (byte)(pixelColor.R * 255); // Red
                    pixelData[pixelIndex + 3] = 0;                          // Alpha (не используется)

                    pixelIndex += 4;
                }
            }

            // Копируем буфер вывода в bitmap
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);
        }

        // Метод для поиска ближайшего пересечения луча со сферами
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

        // Метод для проверки, находится ли точка в тени
        private bool IsPointInShadow(Vector3 point, Vector3 normal, LightSource light, List<Sphere> spheres, Sphere currentSphere)
        {
            // Вычисляем направление к источнику света
            Vector3 lightDirection = (light.Position - point).Normalized();

            // Смещаем точку немного по нормали, чтобы избежать самопересечения
            Vector3 shadowRayOrigin = point + normal * 0.001;

            // Создаем теневой луч
            Ray shadowRay = new Ray(shadowRayOrigin, lightDirection);

            // Проверяем пересечение теневого луча со всеми сферами, кроме текущей
            foreach (Sphere sphere in spheres)
            {
                if (sphere == currentSphere) continue; // Пропускаем текущую сферу

                var intersection = sphere.Intersect(shadowRay);
                if (intersection.intersects)
                {
                    // Если есть пересечение с другой сферой - точка в тени
                    return true;
                }
            }

            return false;
        }
    }
}