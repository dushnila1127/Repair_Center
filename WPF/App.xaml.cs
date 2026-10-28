using System.Windows;

namespace RepairCenter
{
    public partial class App : Application
    {
        public static Entities.user24_dbEntities1 Context { get { return new Entities.user24_dbEntities1(); } }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var login = new LoginWindow();
            var result = login.ShowDialog();
            if (result == true && Security.UserSession.Current != null)
            {
                var main = new MainWindow();
                MainWindow = main;
                main.Show();
                ShutdownMode = ShutdownMode.OnLastWindowClose;
            }
            else
            {
                Shutdown();
            }
        }
    }
}
