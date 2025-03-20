using System.Diagnostics;
using Energy;

// Генератор датасета
var generator = new DatasetManager(new List<string> { "Э", "Н", "Е", "Р", "Г", "И", "Я", "!" });

// Настройки перцептрона
var settings = new Settings
{
    Size = 100,
    ConnectionsCount = 100,
    ActivationLimit = 3,
    StepCount = 50_000,
    WeightCorrection = 0.05
};

var perceptron = new Perceptron(generator, settings);


// Обучение перцптрона
var threadsNumber = 12;
var stopwatch = Stopwatch.StartNew();
perceptron.Run(threadsNumber);
stopwatch.Stop();

// Проверка на 1000 тестовых данных
var successes = 0;
for (var i = 0; i < 1000; i++)
{
    var characterToRecognize = generator.GetCharacterByIndex(i);
    var imageToRecognize = generator.GenerateImage(characterToRecognize, 100);
    var result = perceptron.Recognize(imageToRecognize);
    if (generator.GetBinaryCodeByCharacter(characterToRecognize) == result.Code)
        successes += 1;

    // Вывод в консоль последних 20 изображений
    if (i >= 979)
    {
        Console.WriteLine($"Шаг: {i}");
        for (var y = 0; y < imageToRecognize.Height; y++)
        {
            for (var x = 0; x < imageToRecognize.Width; x++)
            {
                var pixelColor = imageToRecognize.GetPixel(x, y);
                Console.Write(pixelColor.R > 128 ? "█" : " ");
            }

            Console.WriteLine();
        }

        if (generator.GetBinaryCodeByCharacter(characterToRecognize) == result.Code)

            Console.WriteLine($"Я думаю, что это {characterToRecognize}!\n======================");
        else
            Console.WriteLine("Я не знаю, что это.");
    }
}

Console.WriteLine($"Процент угадываний: {successes * 100 * 0.001}%");
Console.WriteLine($"Время обучения: {stopwatch.Elapsed}");
Console.WriteLine($"Количество потоков: {threadsNumber}");