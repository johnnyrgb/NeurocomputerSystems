
using PerceptronV3;

PerceptronV3.PerceptronV3 perceptron = new PerceptronV3.PerceptronV3();
perceptron.Run();
int successes = 0;
for (int i = 0; i < 1000; i++)
{
    ImageType imageType = ImageType.Unknown;
    if (i % 4 == 0)
        imageType = ImageType.LetterB;
    else if (i % 4 == 1)
        imageType = ImageType.LetterG;
    else if (i % 4 == 2)
        imageType = ImageType.LetterE;
    else if (i % 4 == 3)
        imageType = ImageType.LetterI;
    var image = perceptron.GenerateImage(imageType);
    var result = perceptron.Recognize(image).ImageType;
    if (imageType == result)
        successes += 1;

    if (i >= 979)
    {
        Console.WriteLine($"Шаг: {i}");
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixelColor = image.GetPixel(x, y);
                Console.Write(pixelColor.R > 128 ? "█" : " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Я думаю, что это {result}!\n======================");
    }
}

Console.WriteLine($"Процент угадываний: {successes * 100 * 0.001}%");