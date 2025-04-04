﻿using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

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

    public class Perceptron
    {

        #region Параметры модели
        // Размер изображения
        private int Size { get; set; } = 100;
        // Порог активации нейрона
        private int ActivationLimit { get; set; } = 3;
        // Изменение веса при коррекции
        private double WeightCorrection { get; set; } = 0.03;
        // Количество связей между рецептором и нейронами
        private int ConnectionsCount { get; set; }
        #endregion

        #region Структуры данных

        // Связи между рецепторным и ассоциативным слоями
        private  List<List<List<Coordinates>>> Connections { get; set; }

        // Веса
        private List<List<double>> Weights { get; set; }
        #endregion

        #region Параметры обучения
        // Успешные ответы
        private int Successes { get; set; } = 0;
        // Количество итераций
        private int StepCount { get; set; } = 1_000_000;
        // Текущая итерация
        private int CurrentStep { get; set; }
        #endregion
        #region Служебные переменные
        private Random Random { get; set; } = new Random();
        #endregion
        public Perceptron(int size = 20, int connectionsCount = 20, int activationLimit = 3)
        {
            Size = size;
            ConnectionsCount = connectionsCount;
            ActivationLimit = activationLimit;

            Weights = new List<List<double>>();
            Connections = new List<List<List<Coordinates>>>();

            for (var i = 0; i < Size; i++)
            {
                Weights.Add(new List<double>());
                Connections.Add(new List<List<Coordinates>>());
                for (var j = 0; j < Size; j++)
                {
                    Weights[i].Add(0.0);
                    Connections[i].Add(new List<Coordinates>());
                    for (var k = 0; k < ConnectionsCount; k++)
                    {
                        Connections[i][j].Add(new Coordinates());
                    }
                }
            }
        }

        public bool Run()
        {
            // TODO: засунуть все в фор по количеству итераций. Связи постоянны или меняются каждую итерацию? Очищать слои на каждой итерации.
            // Генерация случайных связей между рецепторами и нейронами
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var k = 0; k < ConnectionsCount; k++)
                        Connections[x][y][k] = new Coordinates { X = Random.Next(Size), Y = Random.Next(Size) };
                }
            }

            for (var step = 1; step <= StepCount; step++)
            {
                // Генерируем случайчное изображение
                var imageType = (step % 2) == 0 ? ImageType.Circle : ImageType.Rhombus;
                var image = GenerateImage(imageType);

                // Распознаем изображение
                var recognizedImage = Recognize(image);

                // Обучаем, если изображение не распознано
                if (imageType != recognizedImage.Item1)
                {
                    for (var x = 0; x < Size; x++)
                    {
                        for (var y = 0; y < Size; y++)
                        {
                            if (recognizedImage.Item2[x][y] == 1)
                            {
                                if (imageType == ImageType.Circle)
                                    Weights[x][y] += WeightCorrection;
                                else
                                    Weights[x][y] -= WeightCorrection;
                            }
                        }
                    }
                }
                else
                {
                    Successes += 1;
                }

                if (step % 100 == 0)
                {
                    double percentOfSuccess = Successes * 100.0 / step;

                    Console.WriteLine($"Шаг: {step} | Успешно: {percentOfSuccess}%");
                }
            }

            return true;
        }

        public (ImageType, List<List<int>>) Recognize(Bitmap image)
        {
            // Формируем ассоциативный слой
            var associationLayer = new List<List<int>>();
            for (var i = 0; i < Size; i++)
            {
                associationLayer.Add(new List<int>());
                for (var j = 0; j < Size; j++)
                {
                    associationLayer[i].Add(0);
                }
            }

            // Формируем входы ассоциативного слоя
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    var pixel = image.GetPixel(x, y);
                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    {
                        // Если пиксель черный, то активируем нейроны
                        for (var k = 0; k < ConnectionsCount; k++)
                        {
                            var connection = Connections[x][y][k];
                            associationLayer[connection.X][connection.Y] += 1;
                        }
                    }
                }
            }

            // Формируем выходы ассоциативного слоя
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    associationLayer[x][y] = associationLayer[x][y] > ActivationLimit ? 1 : 0;
                }
            }

            // Отображение
            var effectorLayer = 0.0;
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    effectorLayer += associationLayer[x][y] * Weights[x][y];
                }
            }

            var recognizedImage = (effectorLayer > 0 ? ImageType.Circle : ImageType.Rhombus, associationLayer);
            return recognizedImage;
        }
        public Bitmap GenerateImage(ImageType imageType)
        {
            var image = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
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
