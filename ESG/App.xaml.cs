using ESG.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ESG
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Устанавливаем ShutdownMode в OnExplicitShutdown, чтобы приложение не завершалось автоматически
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Создаем и показываем окно авторизации
                var loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                // Проверяем результат авторизации
                if (loginResult == true)
                {
                    // Успешная авторизация — создаем и показываем MainWindow
                    var mainWindow = new MainWindow(loginWindow.AuthenticatedUser, loginWindow.RoleName);
                    MainWindow = mainWindow; // Устанавливаем MainWindow как главное окно приложения

                    // После создания MainWindow устанавливаем ShutdownMode в OnMainWindowClose
                    ShutdownMode = ShutdownMode.OnMainWindowClose;
                    mainWindow.Show();
                }
                else
                {
                    // Авторизация отменена — показываем сообщение и завершаем приложение
                    MessageBox.Show("Авторизация отменена. Приложение будет закрыто.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                // Ловим и показываем любые ошибки, которые могут возникнуть
                MessageBox.Show($"Произошла ошибка при запуске приложения: {ex.Message}\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Можно добавить логику перед завершением приложения, если нужно
        }
    }
}