namespace Vibration
{
    public class Program
    {
        static void Main(string[] args)
        {
             var settings = new WindowSetting
             {
                 Width = 1600,
                 Height = 900,
                 Title = "Окно",
                 FramerateLimit = 400
             };
             var window = new MainWindow(settings);
             window.Run();

            

        }


    }

    
}
