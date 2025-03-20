using System.Drawing;

namespace Energy;

public class DatasetManager
{
    public DatasetManager(List<string> characters)
    {
        Dataset = new HashSet<string>(characters);
        BinaryCodes = GenerateBinaryCodes();
    }

    // Набор символов
    private HashSet<string> Dataset { get; }

    // Двоичные коды символов
    private Dictionary<string, string> BinaryCodes { get; }
    private Random Random { get; } = new();

    /// <summary>
    ///     Генерация двоичных кодов для символов.
    /// </summary>
    ///    /// <returns></returns>
    private Dictionary<string, string> GenerateBinaryCodes()
    {
        Dictionary<string, string> binaryCodes = new();

        // Вычисление длины двоичного кода путем округления вверх двоичного логарифма от количества символов  
        var binaryLength = (int)Math.Ceiling(Math.Log2(Dataset.Count));
        for (var i = 0; i < Dataset.Count; i++)
        {
            // Преобразование числа в двоичный код и добавление нулей слева до длины двоичного кода  
            var binaryCode = Convert.ToString(i, 2).PadLeft(binaryLength, '0');
            // Добавление символа и двоичного кода в словарь  
            binaryCodes[Dataset.ElementAt(i)] = binaryCode;
        }

        return binaryCodes;
    }

    /// <summary>
    ///     Генерация изображения с символом.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="canvasSize"></param>
    /// <returns></returns>
    public Bitmap GenerateImage(string character, int canvasSize)
    {
        // Создание холста
        var image = new Bitmap(canvasSize, canvasSize);
        using (var g = Graphics.FromImage(image))
        {
            // Очистка холста
            g.Clear(Color.White);

            // Выбор случайного размера хитбокса символа и его координат
            // Для ускорения обучения размер одинаковый
            var hitboxSize = Random.Next(canvasSize / 2, canvasSize / 2);

            // Выбор случайной координаты центра хитбокса символа
            var hitboxCenterX = Random.Next(hitboxSize / 2, canvasSize - hitboxSize / 2);
            var hitboxCenterY = Random.Next(hitboxSize / 2, canvasSize - hitboxSize / 2);

            // Установка прямоугольной области для рисования символа
            var hitbox = new Rectangle(hitboxCenterX - hitboxSize / 2, hitboxCenterY - hitboxSize / 2, hitboxSize,
                hitboxSize);

            using (var drawFont = new Font("Times New Roman", hitboxSize, GraphicsUnit.Pixel))
            {
                // Рисование символа в прямоугольной области
                g.DrawString(character, drawFont, Brushes.Black, hitbox);

                /*
                 * ВНИМАНИЕ: Фактический контур символа меньше, чем размер строки,
                 * поэтому символ занимает не весь хитбокс.
                 */
            }
        }

        return image;
    }

    /// <summary>
    /// Получение бинарного кода символа.
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public string? GetBinaryCodeByCharacter(string character)
    {
        return BinaryCodes.GetValueOrDefault(character);
    }

    /// <summary>
    /// Получение символа по индексу из датасета.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetCharacterByIndex(int index)
    {
        var circularIndex = index % Dataset.Count;
        return Dataset.ElementAt(circularIndex);
    }

    /// <summary>
    /// Получение длины текущих двоичных кодов, которыми кодируются символы.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentBinaryCodeLength()
    {
        if (BinaryCodes.Count == 0) return 0;
        return BinaryCodes.First().Value.Length;
    }
}