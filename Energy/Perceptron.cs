using System.Drawing;

namespace Energy;

public struct Coordinates
{
    public int X, Y;
}

/// <summary>
///     Структура с гиперпараметрами перцептрона.
/// </summary>
public struct Settings
{
    public int Size;
    public int ConnectionsCount;
    public int ActivationLimit;
    public int StepCount;
    public double WeightCorrection;
}
public class Perceptron
{
    public Perceptron(DatasetManager datasetManager, Settings settings)
    {
        // Задаем параметры для перцептрона
        Size = settings.Size;
        ConnectionsCount = settings.ConnectionsCount;
        ActivationLimit = settings.ActivationLimit;
        StepCount = settings.StepCount;
        WeightCorrection = settings.WeightCorrection;

        // Задаем датасет и длину бинарного кода образов
        DatasetManager = datasetManager;
        BinaryCodeLength = datasetManager.GetCurrentBinaryCodeLength();

        // Инициализация списка весов
        InitializeWeights();

        // Инициализация списка связей между рецепторами и ассоциативным слоем
        InitializeConnections();
    }

    /// <summary>
    ///     Инициализация весов пустыми списками.
    /// </summary>
    private void InitializeWeights()
    {
        Weights = new List<List<List<double>>>();
        for (var i = 0; i < Size; i++)
        {
            Weights.Add(new List<List<double>>());
            for (var j = 0; j < Size; j++)
            {
                Weights[i].Add(new List<double>());
                for (var k = 0; k < BinaryCodeLength; k++) Weights[i][j].Add(0.0);
            }
        }
    }

    /// <summary>
    ///     Инициализация связей между рецепторами и ассоциативным слоем пустыми списками.
    /// </summary>
    private void InitializeConnections()
    {
        Connections = new List<List<List<Coordinates>>>();
        for (var i = 0; i < Size; i++)
        {
            Connections.Add(new List<List<Coordinates>>());
            for (var j = 0; j < Size; j++)
            {
                Connections[i].Add(new List<Coordinates>());
                for (var k = 0; k < ConnectionsCount; k++) Connections[i][j].Add(new Coordinates());
            }
        }
    }

    /// <summary>
    ///     Генерация случайных связей между рецепторами и нейронами.
    /// </summary>
    private void GenerateRandomConnections()
    {
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var k = 0; k < ConnectionsCount; k++)
            Connections[x][y][k] = new Coordinates { X = Random.Next(Size), Y = Random.Next(Size) };
    }

    /// <summary>
    ///     Запуск обучения перцептрона.
    /// </summary>
    /// <param name="threadsNumber">Количество потоков процессора, занимающихся обучением.</param>
    /// <returns></returns>
    public bool Run(int threadsNumber)
    {
        // Генерация случайных связей между рецепторами и нейронами
        GenerateRandomConnections();

        // Настройка количества потоков
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = threadsNumber
        };

        /* 
         * Обучение перцептрона.
         * Каждый поток берет следующую доступную итерацию, по мере завершения предыдущей
         * Общими перменными являются счетчик итераций, количество успешных ответов и список весов
         */
        Parallel.For((long)1, StepCount + 1, parallelOptions, step =>
        {
            // Получаем случайный символ
            var characterToRecognize = DatasetManager.GetCharacterByIndex((int)step);

            // Получаем код символа
            var characterToRecognizeBinaryCode =
                DatasetManager.GetBinaryCodeByCharacter(characterToRecognize)!;

            // Генерируем изображение символа
            var imageToRecognize = DatasetManager.GenerateImage(characterToRecognize, Size);

            // Распознаем изображение
            var recognizeResult = Recognize(imageToRecognize);
            // Если изображение распознано, прибавляем успех
            if (recognizeResult.Code == characterToRecognizeBinaryCode)
                Interlocked.Increment(ref successes);

            
            // Обучаем, если изображение не распознано
            // Перебираем распознанный код и наказываем ответственных за неверные биты
            // Блокируем чтение и запись Weights протяжении всего процесса наказывания
            else
            {
                weightsLock.EnterWriteLock();
                try
                { 
                for (var k = 0; k < BinaryCodeLength; k++)
                    // Если биты не совпадают, изменяем ответственные веса
                    if (recognizeResult.Code[k] != characterToRecognizeBinaryCode[k])
                        for (var x = 0; x < Size; x++)
                            for (var y = 0; y < Size; y++)
                                if (recognizeResult.AssociationLayer[x][y] == 1)
                                        if (recognizeResult.Code[k] == '0')
                                            Weights[x][y][k] += WeightCorrection;
                                        else
                                            Weights[x][y][k] -= WeightCorrection;
                }
                finally
                {
                    weightsLock.ExitWriteLock();
                }
            }
               

            // Увеличиваем общее количество выполненных итераций
            Interlocked.Increment(ref сurrentStepCount);

            // Выводим промежуточные результаты
            if (step % 1000 == 0)
            {
                var percentOfSuccess = successes * 100.0 / сurrentStepCount;
                Console.WriteLine(
                    $"Процесс: {Thread.CurrentThread.ManagedThreadId} | Шаг: {сurrentStepCount} | Успешно: {percentOfSuccess}%");
            }
        });

        return true;
    }

    /// <summary>
    ///     Инициализация ассоциативного слоя.
    /// </summary>
    /// <returns></returns>
    private List<List<int>> InitializeAssociationLayer()
    {
        var associationLayer = new List<List<int>>();
        for (var i = 0; i < Size; i++)
        {
            associationLayer.Add(new List<int>());
            for (var j = 0; j < Size; j++) associationLayer[i].Add(0);
        }

        return associationLayer;
    }

    /// <summary>
    ///     Генерация входов ассоциативного слоя.
    /// </summary>
    /// <param name="associationLayer"></param>
    /// <param name="image"></param>
    private void GenerateAssociationLayerInputs(List<List<int>> associationLayer, Bitmap image)
    {
        // Перебираем пиксели изображения
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        {
            var pixel = image.GetPixel(x, y);
            if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                // Если пиксель черный, то активируем нейроны
                for (var k = 0; k < ConnectionsCount; k++)
                {
                    var connection = Connections[x][y][k];
                    associationLayer[connection.X][connection.Y] += 1;
                }
        }
    }

    /// <summary>
    ///     Генерация выходов ассоциативного слоя.
    /// </summary>
    /// <param name="associationLayer"></param>
    private void GenerateAssociationLayerOutputs(List<List<int>> associationLayer)
    {
        // Активируем нейроны, которые больше установленного лимита (остальные 0)
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
            associationLayer[x][y] = associationLayer[x][y] > ActivationLimit ? 1 : 0;
    }

    /// <summary>
    ///     Получение бинарного кода.
    /// </summary>
    /// <param name="associationLayer"></param>
    /// <returns></returns>
    private string GetBinaryCode(List<List<int>> associationLayer)
    {
        var effectorLayer = new List<double>();
        var code = "";
        // Блокируем веса на запись, чтобы никто их не менял во время отображения
        // Остальные потоки могут их читать, но не могут записывать
        weightsLock.EnterReadLock();
        try
        {
            for (var k = 0; k < BinaryCodeLength; k++)
            {
                effectorLayer.Add(0.0);
                for (var x = 0; x < Size; x++)
                    for (var y = 0; y < Size; y++)
                        effectorLayer[k] += associationLayer[x][y] * Weights[x][y][k];

                if (effectorLayer[k] > 0)
                    code += "1";
                else
                    code += "0";
            }
        }
        finally
        {
            weightsLock.ExitReadLock();
        }
        return code;
    }

    public RecognizeResult Recognize(Bitmap image)
    {
        // Инициализируем ассоциативный слой
        var associationLayer = InitializeAssociationLayer();

        // Формируем входы ассоциативного слоя
        GenerateAssociationLayerInputs(associationLayer, image);

        // Формируем выходы ассоциативного слоя
        GenerateAssociationLayerOutputs(associationLayer);

        // Распознаем изображение и получаем код символа
        var binaryCode = GetBinaryCode(associationLayer);
     

        return new RecognizeResult
        {
            Code = binaryCode,
            AssociationLayer = associationLayer
        };
    }

    /// <summary>
    ///     Результат распознавания изображения.
    /// </summary>
    public struct RecognizeResult
    {
        public string Code;
        public List<List<int>> AssociationLayer;
    }

    #region Параметры

    // Размер изображения
    private int Size { get; }

    // Порог активации нейрона
    private int ActivationLimit { get; }

    // Изменение веса при коррекции
    private double WeightCorrection { get; set; }

    // Количество связей между рецептором и нейронами
    private int ConnectionsCount { get; }

    // Общее количество итераций
    private int StepCount { get; } = 150_000;

    #endregion

    #region Вычисляемые значения

    // Длина бинарного кода, которым кодируется каждый символ
    private int BinaryCodeLength { get; }

    // Связи между рецепторным и ассоциативным слоями
    private List<List<List<Coordinates>>> Connections { get; set; }

    // Веса
    private List<List<List<double>>> Weights { get; set; }

    #endregion

    #region Служебные переменные

    
    private readonly ReaderWriterLockSlim weightsLock = new ReaderWriterLockSlim();

    // Текущий счетчик итераций
    private int сurrentStepCount;

    // Успешные ответы
    private int successes;

    private Random Random { get; } = new();

    private DatasetManager DatasetManager { get; }

    #endregion
}