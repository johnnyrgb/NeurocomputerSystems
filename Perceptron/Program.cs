using System.Drawing.Imaging;
using Perceptron;

Perceptron.Perceptron perceptron = new Perceptron.Perceptron();
perceptron.Run();

for (int i = 0; i < 1000; i++)
{
    var imageType = i % 2 == 0 ? ImageType.Circle : ImageType.Rhombus;
    var image = perceptron.GenerateImage(imageType);
    var result = perceptron.Recognize(image).Item1;

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