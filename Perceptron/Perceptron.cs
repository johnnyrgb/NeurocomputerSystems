using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Perceptron
{
    public struct Coordinates
    {
        public int X, Y;
    }

    public enum ImageType
    {
        Circle,
        Rhombus,
    }

    public static class Perceptron
    {

        #region Параметры модели
        // Размер изображения
        private static int Size { get; set; } = 100;
        // Порог активации нейрона
        private static int ActivationLimit { get; set; }
        // Изменение веса при коррекции
        private static double WeightCorrection { get; set; }
        // Количество связей между рецептором и нейронами
        private static int ConnectionsCount { get; set; }
        #endregion

        #region Структуры данных
        // Слой рецепторов
        private static Bitmap ReceptorLayer { get; set; }
        // Связи между рецепторным и ассоциативным слоями
        private static List<List<List<Coordinates>>> Connections { get; set; }
        // Ассоциативный слой
        private static List<List<int>> AssociationLayer { get; set; }
        // Веса
        private static List<List<int>> Weights { get; set; }
        #endregion

        #region Параметры обучения
        // Успешные ответы
        private static int Successes { get; set; } = 0;
        // Количество итераций
        private static int StepCount { get; set; }
        // Текущая итерация
        private static int CurrentStep { get; set; }
        #endregion
        #region Служебные переменные
        private static Random Random { get; set; } = new Random();
        private static bool IsDebugging { get; set; }
        #endregion


        static Perceptron()
        {
            // Инициализация свойств, если необходимо
        }

        private static bool Run(int connectionsCount)
        {
            // TODO: засунуть все в фор по количеству итераций. Связи постоянны или меняются каждую итерацию? Очищать слои на каждой итерации.
            // Генерация случайных связей между рецепторами и нейронами
            ConnectionsCount = connectionsCount;
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var k = 0; k < connectionsCount; k++)
                        Connections[x][y][k] = new Coordinates { X = Random.Next(Size), Y = Random.Next(Size) };
                }
            }


            return true;
        }
        public static Bitmap GenerateImage(ImageType imageType)
        {
            var image = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);
                switch (imageType)
                {
                    case ImageType.Circle:
                        int radius = Random.Next(Size / 5, Size / 2);
                        int x = Random.Next(radius, Size - radius);
                        int y = Random.Next(radius, Size - radius);
                        g.DrawEllipse(Pens.Black, x - radius, y - radius, radius * 2, radius * 2);
                        break;

                    case ImageType.Rhombus:
                        int side = Random.Next(Size / 5, Size / 2);
                        int centerX = Random.Next(side, Size - side);
                        int centerY = Random.Next(side, Size - side);
                        Point[] points = new Point[4];
                        points[0] = new Point(centerX, centerY - side);
                        points[1] = new Point(centerX + side, centerY);
                        points[2] = new Point(centerX, centerY + side);
                        points[3] = new Point(centerX - side, centerY);
                        g.DrawPolygon(Pens.Black, points);
                        break;
                }
            }
            return image;
        }
    }
}
