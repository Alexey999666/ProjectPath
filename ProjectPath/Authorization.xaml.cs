using ProjectPath.Modelsdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProjectPath
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private int _attemptCount = 0;
        private DispatcherTimer? _blockTimer;
        private string _currentCaptcha = "";

        public Authorization()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbLogin.Focus();
            _blockTimer = new DispatcherTimer();
            _blockTimer.Interval = TimeSpan.FromSeconds(10);
            _blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object? sender, EventArgs e)
        {
            _blockTimer?.Stop();
            btnLogin.IsEnabled = true;
            btnGuest.IsEnabled = true;
            tbStatus.Text = "";
        }

        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            Random rnd = new Random();
            char[] captcha = new char[5];
            for (int i = 0; i < 5; i++)
            {
                captcha[i] = chars[rnd.Next(chars.Length)];
            }
            _currentCaptcha = new string(captcha);
            tbCaptchaCode.Text = _currentCaptcha;
            spCaptcha.Visibility = Visibility.Visible;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text.Trim();
            string password = pbPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                tbStatus.Text = "Введите логин и пароль";
                return;
            }

            if (_attemptCount >= 1)
            {
                if (tbCaptcha.Text != _currentCaptcha)
                {
                    tbStatus.Text = "Неверная CAPTCHA";
                    GenerateCaptcha();
                    tbCaptcha.Text = "";
                    return;
                }
            }

            using (var db = new ProjectNewPartsContext())
            {
                var user = db.Employees
                    .FirstOrDefault(u => u.EmployeeLogin == login && u.EmployeePassword == password);

                if (user != null)
                {
                    Data.IsLoggedIn = true;
                    Data.CurrentUser = user;
                    Data.UserFullName = $"{user.LastName} {user.FirstName} {user.Surname}";

                    var role = db.Roles.FirstOrDefault(r => r.UserRoleId == user.EmployeeRoleId);
                    Data.UserRole = role?.Role1 ?? "Сотрудник";

                    DialogResult = true;
                    Close();
                }
                else
                {
                    _attemptCount++;
                    tbStatus.Text = "Неверный логин или пароль";

                    if (_attemptCount >= 1)
                    {
                        GenerateCaptcha();
                    }

                    if (_attemptCount >= 2)
                    {
                        btnLogin.IsEnabled = false;
                        btnGuest.IsEnabled = false;
                        _blockTimer?.Start();
                        tbStatus.Text = "Доступ заблокирован на 10 секунд";
                    }

                    tbLogin.Text = "";
                    pbPassword.Password = "";
                    tbLogin.Focus();
                }
            }
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            Data.IsLoggedIn = true;
            Data.CurrentUser = null;
            Data.UserFullName = "Гость";
            Data.UserRole = "Гость";
            DialogResult = true;
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
