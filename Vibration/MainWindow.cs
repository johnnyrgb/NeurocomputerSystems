using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace Vibration
{
    public class MainWindow
    {
        private RenderWindow window;
        private VertexArray functionVertices;
        private VertexArray predictedVertices;
        private VertexArray axes;
        private Manager manager;
        private List<int> xAxis;
        private Font font;
        private Text weightsText;

        public MainWindow(WindowSetting settings)
        {
            window = new RenderWindow(new VideoMode(settings.Width, settings.Height), settings.Title);
            window.SetFramerateLimit(settings.FramerateLimit);
            window.Closed += OnWindowClosed;
            functionVertices = new VertexArray(PrimitiveType.LineStrip, 51);
            predictedVertices = new VertexArray(PrimitiveType.LineStrip, 51);

            axes = new VertexArray(PrimitiveType.Lines, 4);

            // Фиксируем оси в центре окна
            float zeroX = window.Size.X / 2f;
            float zeroY = window.Size.Y / 2f;

            // Ось X
            axes[0] = new Vertex(new Vector2f(0, zeroY), Color.White);
            axes[1] = new Vertex(new Vector2f(window.Size.X, zeroY), Color.White);

            // Ось Y
            axes[2] = new Vertex(new Vector2f(zeroX, 0), Color.White);
            axes[3] = new Vertex(new Vector2f(zeroX, window.Size.Y), Color.White);

            xAxis = new List<int>();
            for (int i = 0; i < 51; i++)
            {
                xAxis.Add((int)(i * (window.Size.X / 50.0)));
            }

            manager = new Manager();

            font = new Font("Raleway.ttf");
            weightsText = new Text("", font, 20)
            {
                FillColor = Color.White,
                Position = new Vector2f(10, 10)
            };
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            window.Close();
        }

        public void Run()
        {
            while (window.IsOpen)
            {
                window.DispatchEvents();

                Update();
                Draw();

                window.Display();
            }
        }

        private void Update()
        {
            manager.Run();

            var maxFunctionValue = manager.Function.CurrentValues.Max();
            var minFunctionValue = manager.Function.CurrentValues.Min();

            var maxPredictedValue = manager.Prediction.PredictedValues.Max();
            var minPredictedValue = manager.Prediction.PredictedValues.Min();

            var minValue = Math.Min(minFunctionValue, minPredictedValue);
            var maxValue = Math.Max(maxFunctionValue, maxPredictedValue);

            float windowHeight = window.Size.Y;
            var normalizedValues = new List<double>(manager.Function.CurrentValues);
            // Нормализация значений функции относительно высоты окна
            for (int i = 0; i < manager.Function.CurrentValues.Count; i++)
            {
                normalizedValues[i] = (manager.Function.CurrentValues[i] - minValue) / (maxValue - minValue) * windowHeight;
            }

            for (uint i = 0; i < 51; i++)
            {
                float x = xAxis[(int)i];
                float y = windowHeight - (float)normalizedValues[(int)i];
                functionVertices[i] = new Vertex(new Vector2f(x, y), Color.Green);
            }

            var normalizedPredictedValues = new List<double>(manager.Prediction.PredictedValues);
            for (int i = 0; i < manager.Prediction.PredictedValues.Count - 1; i++)
            {
                normalizedPredictedValues[i] = (manager.Prediction.PredictedValues[i] - minValue) / (maxValue - minValue) * windowHeight;
            }

            for (uint i = 0; i < 51; i++)
            {
                predictedVertices[i] = new Vertex(new Vector2f(xAxis[(int)i], windowHeight - (float)normalizedPredictedValues[(int)i]), Color.Red);
            }

            // Обновление текста с весами
            weightsText.DisplayedString = string.Join("\n", manager.Prediction.Weights.Select((w, i) => $"W{i + 1} = {w:F4}"));
        }

        private void Draw()
        {
            window.Clear(Color.Black);
            window.Draw(axes);
            window.Draw(functionVertices);
            window.Draw(predictedVertices);
            window.Draw(weightsText);
        }
    }

    public struct WindowSetting
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public string Title { get; set; }
        public uint FramerateLimit { get; set; }
    }
}
