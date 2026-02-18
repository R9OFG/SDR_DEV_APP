using System.Reflection;

namespace SDR_DEV_APP
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        // Уникальное имя мьютекса на основе имени сборки
        private static readonly string MutexName = $"Global\\{{SDR_DEV_APP_{Assembly.GetExecutingAssembly().GetName().Name}}}";

        /*[STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMain());
        }*/

        [STAThread]
        static void Main()
        {
            // Пытаемся создать именованный мьютекс
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            {
                // Если мьютекс уже существует — приложение уже запущено
                if (!createdNew)
                {
                    return;
                }

                // Запускаем приложение
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmMain()); // ← замените на имя вашей главной формы
            }
        }        
    }
}