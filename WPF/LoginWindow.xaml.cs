using System.Windows;
using RepairCenter.Security;

namespace RepairCenter
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += delegate { LoginText.Focus(); };
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            UserSessionInfo session;
            string message;
            if (!AuthService.TryLogin(LoginText.Text, PasswordInput.Password, out session, out message))
            {
                MessageBox.Show(message, "Вход в RepairCenter", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordInput.Clear();
                PasswordInput.Focus();
                return;
            }
            UserSession.Start(session);
            DialogResult = true;
        }
    }
}
