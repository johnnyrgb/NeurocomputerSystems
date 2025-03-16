using System.Drawing;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;
using Font = System.Drawing.Font;

namespace PerceptronV3
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

    public class PerceptronV3
    {
        private Dictionary<string, ImageType> CodeImage = new Dictionary<string, ImageType>()
        {
            { "00", ImageType.LetterB },
            { "01", ImageType.LetterG },
            { "10", ImageType.LetterE },
            { "11", ImageType.LetterI }
        };

        private Dictionary<ImageType, string> ImageCode = new Dictionary<ImageType, string>()
        {
            {  ImageType.LetterB, "00" },
            {  ImageType.LetterG, "01" },
            {  ImageType.LetterE, "10" },
            {  ImageType.LetterI, "11" }
        };

        public ImageType GetImageByCode(string code)
        {
            return CodeImage.GetValueOrDefault(code, ImageType.Unknown);
        }

        public string GetCodeByImage(ImageType imageType)
        {
            return ImageCode.GetValueOrDefault(imageType, "");
        }
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

        private int successes = 0;

        private int iterationCount = 0;
        // Количество итераций
        private int StepCount { get; set; } = 150_000;
        // Текущая итерация
        private int CurrentStep { get; set; }
        #endregion
        #region Служебные переменные
        private Random Random { get; set; } = new Random();
        private bool IsDebugging { get; set; }
        #endregion
        public PerceptronV3(int size = 70, int connectionsCount = 70, int activationLimit = 3)
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
                    for (var k = 0; k < 2; k++)
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

            Parallel.For((long)1, StepCount + 1, step =>
            {
                Interlocked.Increment(ref iterationCount);
                // Генерируем случайное изображение
                ImageType imageType = (ImageType)(step % 4);
                var image = GenerateImage(imageType);

                // Распознаем изображение
                var recognizeResult = Recognize(image);

                // Обучаем, если изображение не распознано
                if (recognizeResult.ImageType == imageType)
                {
                    Interlocked.Increment(ref successes);
                }
                else
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if (recognizeResult.Code[k] != GetCodeByImage(imageType)[k])
                        {
                            for (var x = 0; x < Size; x++)
                            {
                                for (var y = 0; y < Size; y++)
                                {
                                    if (recognizeResult.AssociationLayer[x][y] == 1)
                                    {
                                        lock (Weights)
                                        {
                                           if (recognizeResult.Code[k] == '0')
                                                Weights[x][y][k] += WeightCorrection;
                                           else
                                                Weights[x][y][k] -= WeightCorrection;
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                }
                
                
                if (step % 1000 == 0)
                {
                    double percentOfSuccess = successes * 100.0 / iterationCount;
                    Console.WriteLine($"Процесс: {Thread.CurrentThread.ManagedThreadId} | Шаг: {iterationCount} | Успешно: {percentOfSuccess}%");
                }
            });

            return true;
        }

        public struct RecognizeResult
        {
            public string Code;
            public ImageType ImageType;
            public List<List<int>> AssociationLayer;
        }

        private List<List<int>> GenerateAssociationLayer()
        {
            var associationLayer = new List<List<int>>();
            for (var i = 0; i < Size; i++)
            {
                associationLayer.Add(new List<int>());
                for (var j = 0; j < Size; j++)
                {
                    associationLayer[i].Add(0);
                }
            }

            return associationLayer;
        }

        private void GenerateAssociationLayerInputs(List<List<int>> associationLayer, Bitmap image)
        {
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
        }

        private void GenerateAssociationLayerOutputs(List<List<int>> associationLayer)
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    associationLayer[x][y] = associationLayer[x][y] > ActivationLimit ? 1 : 0;
                }
            }
        }

        private string GetRecognizedImageTypeCode(List<List<int>> associationLayer)
        {
            var effectorLayer = new List<double> { 0.0, 0.0 };
            var code = "";
            for (var k = 0; k < 2; k++)
            {
                for (var x = 0; x < Size; x++)
                {
                    for (var y = 0; y < Size; y++)
                    {
                        effectorLayer[k] += associationLayer[x][y] * Weights[x][y][k];

                    }
                }
                if (effectorLayer[k] > 0)
                    code += "1";
                else
                    code += "0";

               
            }

            return code;
        }
        public RecognizeResult Recognize(Bitmap image)
        {
            // Формируем ассоциативный слой
            var associationLayer = GenerateAssociationLayer();

            // Формируем входы ассоциативного слоя
            GenerateAssociationLayerInputs(associationLayer, image);

            // Формируем выходы ассоциативного слоя
            GenerateAssociationLayerOutputs(associationLayer);

            // Отображение
            var result = GetRecognizedImageTypeCode(associationLayer);

            return new RecognizeResult
            {
                Code = result,
                ImageType = GetImageByCode(result),
                AssociationLayer = associationLayer
            };
        }

        public Bitmap GenerateImage(ImageType imageType)
        {
            var image = new Bitmap(Size, Size);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);

                var side = Random.Next(Size / 3, Size / 2);
                var centerX = Random.Next(side, Size - side);
                var centerY = Random.Next(side, Size - side);

                using (Font font = new Font("Times New Roman", side))
                {
                    var letter = "";
                    switch (imageType)
                    {
                          
                        case ImageType.LetterB:
                            letter = "Б";
                            break;

                        case ImageType.LetterG:
                        letter = "Г";
                        break;

                        case ImageType.LetterE:
                        letter = "Е";
                        break;

                        case ImageType.LetterI:
                            letter = "И";
                        break;
                    }
                    g.DrawString(letter, font, Brushes.Black, centerX - side / 2, centerY - side / 2);
                }
                
            }
            return image;
        }
    }
}