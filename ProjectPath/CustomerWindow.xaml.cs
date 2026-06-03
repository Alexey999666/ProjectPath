using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using ProjectPath.Modelsdb;

namespace ProjectPath
{
    public partial class CustomerWindow : Window
    {
        private ProjectNewPartsContext _db;
        private Customer? _selectedCustomer = null;
        private bool _isEditMode = false;
        private System.Windows.Threading.DispatcherTimer _timer;

        public CustomerWindow()
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateDateTime();
            this.Closing += CustomerWindow_Closing;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            tbDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            tbTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

            if (Data.UserRole != "Администратор" && Data.UserRole != "Менеджер")
            {
                btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }

            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                _db.ChangeTracker.Clear();
                _db.Customers.Load();
                dgCustomers.ItemsSource = _db.Customers.Local.ToObservableCollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказчиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = dgCustomers.SelectedItem as Customer;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = tbSearch.Text.ToLower();

                if (string.IsNullOrEmpty(searchText))
                {
                    dgCustomers.ItemsSource = _db.Customers.Local.ToObservableCollection();
                }
                else
                {
                    var filtered = _db.Customers.Local
                        .Where(c => c.NameOrganization.ToLower().Contains(searchText) ||
                                    c.ContactPerson.ToLower().Contains(searchText) ||
                                    c.Phone.Contains(searchText) ||
                                    c.Email.ToLower().Contains(searchText))
                        .ToList();
                    dgCustomers.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            tbNameOrganization.Text = "";
            tbContactPerson.Text = "";
            tbPhone.Text = "";
            tbEmail.Text = "";
            tbAdress.Text = "";
        }

        private void ShowEditPanel(bool show, bool isEdit = false)
        {
            if (show)
            {
                editPanel.Visibility = Visibility.Visible;
                _isEditMode = isEdit;

                if (isEdit)
                {
                    tbFormTitle.Text = "✏️ Редактирование заказчика";
                    btnSave.Content = "💾 Сохранить изменения";
                }
                else
                {
                    tbFormTitle.Text = "➕ Добавление заказчика";
                    btnSave.Content = "💾 Сохранить";
                    ClearForm();
                }
            }
            else
            {
                editPanel.Visibility = Visibility.Collapsed;
                ClearForm();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ShowEditPanel(true, false);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Выберите заказчика для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            tbNameOrganization.Text = _selectedCustomer.NameOrganization;
            tbContactPerson.Text = _selectedCustomer.ContactPerson;
            tbPhone.Text = _selectedCustomer.Phone;
            tbEmail.Text = _selectedCustomer.Email;
            tbAdress.Text = _selectedCustomer.Adress;

            ShowEditPanel(true, true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Выберите заказчика для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Сохраняем ID и имя до удаления
            int customerId = _selectedCustomer.CustomerId;
            string customerName = _selectedCustomer.NameOrganization;

            // Получаем все проекты заказчика
            var projects = _db.Projects.Where(p => p.CustomerId == customerId).ToList();

            // Проверяем наличие незавершённых проектов
            var activeProjects = projects.Where(p => p.Status != "Завершён").ToList();

            if (activeProjects.Any())
            {
                string projectList = string.Join("\n• ", activeProjects.Select(p => p.Name));
                MessageBox.Show($"Невозможно удалить заказчика \"{customerName}\".\n\n" +
                                $"У него есть активные/незавершённые проекты:\n• {projectList}\n\n" +
                                $"Сначала завершите эти проекты.", "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если есть завершённые проекты
            var completedProjects = projects.Where(p => p.Status == "Завершён").ToList();

            string message;
            if (completedProjects.Any())
            {
                string projectList = string.Join("\n• ", completedProjects.Select(p => p.Name));
                message = $"Заказчик \"{customerName}\" имеет завершённые проекты:\n" +
                          $"• {projectList}\n\n" +
                          $"Они будут автоматически удалены вместе с заказчиком.\n\n" +
                          $"Продолжить удаление?";
            }
            else
            {
                message = $"Удалить заказчика \"{customerName}\"?\n\n" +
                          $"У него нет связанных проектов.";
            }

            var result = MessageBox.Show(message, "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        // Удаляем все завершённые проекты заказчика вместе с их историей
                        foreach (var project in completedProjects)
                        {
                            // Удаляем историю проекта
                            var projectActions = _db.ProjectActions.Where(a => a.ProjectId == project.ProjectId);
                            _db.ProjectActions.RemoveRange(projectActions);

                            // Удаляем состав проекта (если есть)
                            var projectCompositions = _db.ProjectCompositions.Where(pc => pc.ProjectId == project.ProjectId);
                            _db.ProjectCompositions.RemoveRange(projectCompositions);

                            // Удаляем проект
                            _db.Projects.Remove(project);
                        }

                        // Удаляем самого заказчика
                        _db.Customers.Remove(_selectedCustomer);

                        _db.SaveChanges();
                        transaction.Commit();
                    }

                  
                    _db.ChangeTracker.Clear();
                    LoadCustomers();

                    // Сбрасываем выбранного заказчика
                    _selectedCustomer = null;

                    if (editPanel.Visibility == Visibility.Visible)
                    {
                        ShowEditPanel(false);
                    }

                    string deleteMessage = completedProjects.Any()
                        ? $"Заказчик \"{customerName}\" и его завершённые проекты успешно удалены"
                        : $"Заказчик \"{customerName}\" успешно удалён";

                    MessageBox.Show(deleteMessage, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNameOrganization.Text))
            {
                MessageBox.Show("Введите название организации", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbNameOrganization.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbContactPerson.Text))
            {
                MessageBox.Show("Введите контактное лицо", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbContactPerson.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPhone.Text))
            {
                MessageBox.Show("Введите телефон", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbPhone.Focus();
                return;
            }

            try
            {
                if (!_isEditMode)
                {
                    var newCustomer = new Customer
                    {
                        NameOrganization = tbNameOrganization.Text,
                        ContactPerson = tbContactPerson.Text,
                        Phone = tbPhone.Text,
                        Email = tbEmail.Text,
                        Adress = tbAdress.Text
                    };

                    _db.Customers.Add(newCustomer);
                    _db.SaveChanges();

                    MessageBox.Show("Заказчик успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (_selectedCustomer == null)
                    {
                        MessageBox.Show("Ошибка: заказчик не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _selectedCustomer.NameOrganization = tbNameOrganization.Text;
                    _selectedCustomer.ContactPerson = tbContactPerson.Text;
                    _selectedCustomer.Phone = tbPhone.Text;
                    _selectedCustomer.Email = tbEmail.Text;
                    _selectedCustomer.Adress = tbAdress.Text;

                    _db.SaveChanges();

                    MessageBox.Show("Заказчик успешно обновлён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadCustomers();
                ShowEditPanel(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ShowEditPanel(false);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                if (this.Owner is MainWindow mainWindow)
                {
                    mainWindow.Visibility = Visibility.Visible;
                    mainWindow.Activate();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void CustomerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (this.Owner is MainWindow mainWindow && mainWindow.Visibility != Visibility.Visible)
            {
                mainWindow.Visibility = Visibility.Visible;
            }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            string mes = "Для того чтобы добавить, изменить или удалить заказчика воспользуйтесь панелью кнопок справа от списка.Это может делать только администратор или менеджер.\nДля редактирования или удаления выберите заказчика из списка. Удаление возможно только если у заказчика нет незавершенных проектов.\nДля возращение на главное окно нажмите на кнопку ←Назад";
            MessageBox.Show(mes, "Справка", MessageBoxButton.OK, MessageBoxImage.Question);
        }
    }
}