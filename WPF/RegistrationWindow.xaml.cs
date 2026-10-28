using System.Windows;
using RepairCenter.Security;

namespace RepairCenter
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            if (UserSession.Current == null || !UserSession.Current.IsAdministrator)
            {
                IsEnabled = false;
                Loaded += delegate
                {
                    MessageBox.Show("Создавать учётные записи может только администратор.", "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                };
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.Current == null || !UserSession.Current.IsAdministrator)
            {
                MessageBox.Show("Создавать учётные записи может только администратор.", "Недостаточно прав", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (PasswordInput.Password != ConfirmPasswordInput.Password)
            {
                MessageBox.Show("Пароль и подтверждение не совпадают.", "Регистрация сотрудника", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordInput.Clear();
                ConfirmPasswordInput.Focus();
                return;
            }
            string message;
            if (!AuthService.RegisterEmployee(UserSession.Current, LoginText.Text, FullNameText.Text, PhoneText.Text, PasswordInput.Password, out message))
            {
                MessageBox.Show(message, "Регистрация сотрудника", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show("Учётная запись сотрудника создана.", "RepairCenter", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
