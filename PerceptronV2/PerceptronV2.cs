using System.Drawing;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;

namespace PerceptronV2
{
    public struct Coordinates
    {
        public int X, Y;
    }

    public enum ImageType
    {
        LetterB,
        LetterG,
        LetterE,
        LetterI,
        Unknown
    }

    public class PerceptronV2
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
        private List<List<List<Coordinates>>> Connections { get; set; }

        // Веса
        private List<List<List<double>>> Weights { get; set; }
        #endregion

        #region Параметры обучения
        // Успешные ответы
        private int Successes { get; set; } = 0;
        // Количество итераций
        private int StepCount { get; set; } = 200000;
        // Текущая итерация
        private int CurrentStep { get; set; }
        #endregion
        #region Служебные переменные
        private Random Random { get; set; } = new Random();
        private bool IsDebugging { get; set; }
        #endregion
        public PerceptronV2(int size = 20, int connectionsCount = 20, int activationLimit = 3)
        {
            Size = size;
            ConnectionsCount = connectionsCount;
            ActivationLimit = activationLimit;

            Weights = new List<List<List<double>>>();
            for (var i = 0; i < Size; i++)
            {
                Weights.Add(new List<List<double>>());
                for (var j = 0; j < Size; j++)
                {
                    Weights[i].Add(new List<double>());
                    for (var k = 0; k < 4; k++)
                    {
                        Weights[i][j].Add(0.0);
                    }
                }
            }

            Connections = new List<List<List<Coordinates>>>();

            for (var i = 0; i < Size; i++)
            {
                Connections.Add(new List<List<Coordinates>>());
                for (var j = 0; j < Size; j++)
                {
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

                ImageType imageType = ImageType.Unknown;
                if (step % 4 == 0)
                    imageType = ImageType.LetterB;
                else if (step % 4 == 1)
                    imageType = ImageType.LetterG;
                else if (step % 4 == 2)
                    imageType = ImageType.LetterE;
                else if (step % 4 == 3)
                    imageType = ImageType.LetterI;
                var image = GenerateImage(imageType);

                // Распознаем изображение
                var recognizedImages = Recognize(image);

                // Обучаем, если изображение не распознано
                if (recognizedImages.Item1.Count == 1 && recognizedImages.Item1.Contains(imageType))
                {
                    Successes += 1;
                }
                else
                {
                    // наказать неугадавшего
                    if (recognizedImages.Item1.Contains(imageType) == false)
                    {
                        for (var x = 0; x < Size; x++)
                        {
                            for (var y = 0; y < Size; y++)
                            {
                                if (recognizedImages.Item2[x][y] == 1)
                                {
                                    Weights[x][y][(int)imageType] += WeightCorrection;
                                }
                            }
                        }
                    }
                    // наказать лишних
                    var wrongImages = recognizedImages.Item1.Where(img => img != imageType).ToList();
                    foreach (var wrongImage in wrongImages)
                    {
                        for (var x = 0; x < Size; x++)
                        {
                            for (var y = 0; y < Size; y++)
                            {
                                if (recognizedImages.Item2[x][y] == 1)
                                {
                                    Weights[x][y][(int)wrongImage] -= WeightCorrection;
                                }
                            }
                        }
                    }
                }
                
     

                if (step % 100 == 0)
                {
                    double percentOfSuccess = Successes * 100.0 / step;

                    Console.WriteLine($"Шаг: {step} | Успешно: {percentOfSuccess}%");
                }
            }

            return true;
        }

        public (List<ImageType>, List<List<int>>) Recognize(Bitmap image)
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
            var result = new List<ImageType>();
            for (var k = 0; k < 4; k++)
            {
                for (var x = 0; x < Size; x++)
                {
                    for (var y = 0; y < Size; y++)
                    {
                        effectorLayer += associationLayer[x][y] * Weights[x][y][k];
                        
                    }
                }
                if (effectorLayer > 0)
                    result.Add((ImageType)k);
            }

            return (result, associationLayer);
        }

        public Bitmap GenerateImage(ImageType imageType)
        {
            var image = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);

                var side = Random.Next(Size / 5, Size / 2);
                var centerX = Random.Next(side, Size - side);
                var centerY = Random.Next(side, Size - side);
                Point[] points;
                switch (imageType)
                {
                    case ImageType.LetterB:
                        points = new Point[6];
                        points[0] = new Point(centerX + side / 2, centerY - side / 2);
                        points[1] = new Point(centerX - side / 2, centerY - side / 2);
                        points[2] = new Point(centerX - side / 2, centerY + side / 2);
                        points[3] = new Point(centerX + side / 2, centerY + side / 2);
                        points[4] = new Point(centerX + side / 2, centerY);
                        points[5] = new Point(centerX - side / 2, centerY);
                        g.DrawLines(Pens.Black, points);
                        break;

                    case ImageType.LetterG:
                        points = new Point[3];
                        points[0] = new Point(centerX + side / 2, centerY - side / 2);
                        points[1] = new Point(centerX - side / 2, centerY - side / 2);
                        points[2] = new Point(centerX - side / 2, centerY + side / 2);
                        g.DrawLines(Pens.Black, points);
                        break;

                    case ImageType.LetterE:
                        points = new Point[4];
                        points[0] = new Point(centerX + side / 2, centerY - side / 2);
                        points[1] = new Point(centerX - side / 2, centerY - side / 2);
                        points[2] = new Point(centerX - side / 2, centerY + side / 2);
                        points[3] = new Point(centerX + side / 2, centerY + side / 2);
                        g.DrawLines(Pens.Black, points);
                        g.DrawLine(Pens.Black, new Point(centerX + side / 2, centerY), new Point(centerX - side / 2, centerY));
                        break;

                        case ImageType.LetterI:
                        points = new Point[4];
                        points[0] = new Point(centerX - side / 2, centerY - side / 2);
                        points[1] = new Point(centerX - side / 2, centerY + side / 2);
                        points[2] = new Point(centerX + side / 2, centerY - side / 2);
                        points[3] = new Point(centerX + side / 2, centerY + side / 2);
                        g.DrawLines(Pens.Black, points);
                        break;
                }
            }
            return image;
        }
    }
}
