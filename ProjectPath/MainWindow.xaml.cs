using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using ProjectPath.Modelsdb;

namespace ProjectPath
{
    public partial class MainWindow : Window
    {
        private ProjectNewPartsContext _db;
        private Project? _selectedProject = null;
        private System.Windows.Threading.DispatcherTimer _timer;
        private bool _isHighlightEnabled = true; // Флаг состояния подсветки

        public MainWindow()
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateDateTime();
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
            try
            {
                Authorization auth = new Authorization();
                bool? result = auth.ShowDialog();

                if (result != true)
                {
                    Close();
                    return;
                }

                tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

                if (Data.UserRole == "Администратор")
                {
                    btnAdd.IsEnabled = true;
                    btnEdit.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    btnAddEvent.IsEnabled = true;
                    btnAddEvent.Visibility = Visibility.Visible;
                }
                else if (Data.UserRole == "Менеджер")
                {
                    btnAdd.IsEnabled = true;
                    btnEdit.IsEnabled = true;
                    btnDelete.IsEnabled = false;
                    btnAddEvent.IsEnabled = true;
                    btnAddEvent.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAdd.IsEnabled = false;
                    btnEdit.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                    btnAddEvent.IsEnabled = false;
                    btnAddEvent.Visibility = Visibility.Collapsed;
                }

                LoadProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void miHelp_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            if (sender == miHelpMain) message = "Инструкция для главного окна\n\n1. Выберите проект из списка\n2. Используйте кнопки для управления\n3. Просматривайте историю изменений\n4. Кнопка 'Подсветка' включает/отключает цветовую маркировку проектов";
            else if (sender == miHelpProject) message = "Инструкция для работы с проектами\n\n• Добавить - создание нового проекта\n• Редактировать - изменение выбранного проекта\n• Удалить - удаление проекта и всей его истории\n• Обновить - обновление списка проектов";
            else if (sender == miHelpEvents) message = "Инструкция для работы с событиями\n\n• Добавить событие - запись нового события по проекту\n• Двойной клик по событию - редактирование (только администратор)";
            else if (sender == miHelpAuth) message = "Инструкция для авторизации\n\n• Введите логин и пароль\n• Администратор: полный доступ\n• Менеджер: ограниченный доступ\n• Пользователь и гость: только просмотр";

            MessageBox.Show(message, "Инструкция", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadProjects()
        {
            try
            {
                _db.ChangeTracker.Clear();

                _db.Projects
                    .Include(p => p.Customer)
                    .Include(p => p.SupervisorNavigation)
                    .Load();

                dgProjects.ItemsSource = _db.Projects.Local.ToObservableCollection();
                UpdateProjectStats();
                UpdateRowHighlight(); // Обновляем подсветку после загрузки
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProjectStats()
        {
            try
            {
                var projects = _db.Projects.Local.ToList();

                tbTotalProjects.Text = projects.Count.ToString();
                tbActiveProjects.Text = projects.Count(p => p.Status == "Активный").ToString();
                tbCompletedProjects.Text = projects.Count(p => p.Status == "Завершён").ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для переключения подсветки
        private void btnToggleHighlight_Click(object sender, RoutedEventArgs e)
        {
            _isHighlightEnabled = !_isHighlightEnabled;
            UpdateRowHighlight();

            // Меняем текст кнопки в зависимости от состояния
            if (_isHighlightEnabled)
            {
                btnToggleHighlight.Content = "🎨 Подсветка (Вкл)";
                btnToggleHighlight.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                //MessageBox.Show("Цветовая подсветка ВКЛЮЧЕНА", "Подсветка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                btnToggleHighlight.Content = "⚪ Подсветка (Выкл)";
                btnToggleHighlight.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));
                //MessageBox.Show("Цветовая подсветка ВЫКЛЮЧЕНА", "Подсветка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод обновления стиля строк
        private void UpdateRowHighlight()
        {
            if (_isHighlightEnabled)
            {
                dgProjects.RowStyle = (Style)FindResource("HighlightRowStyle");
            }
            else
            {
                dgProjects.RowStyle = (Style)FindResource("NormalRowStyle");
            }
        }

        private void dgProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProject = dgProjects.SelectedItem as Project;
            UpdateProjectDetails();
        }

        private void dgProjects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProjects.SelectedItem is Project selected)
            {
                _selectedProject = selected;
                UpdateProjectDetails();
            }
        }

        private void UpdateProjectDetails()
        {
            try
            {
                if (_selectedProject != null)
                {
                    projectInfoPanel.Visibility = Visibility.Visible;
                    noProjectPanel.Visibility = Visibility.Collapsed;

                    tbDetailName.Text = _selectedProject.Name;
                    tbDetailCustomer.Text = _selectedProject.Customer?.NameOrganization ?? "—";
                    tbDetailSupervisor.Text = _selectedProject.SupervisorNavigation != null
                        ? $"{_selectedProject.SupervisorNavigation.LastName} {_selectedProject.SupervisorNavigation.FirstName}"
                        : "—";
                    tbDetailStatus.Text = _selectedProject.Status;

                    switch (_selectedProject.Status)
                    {
                        case "Активный":
                            statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                            break;
                        case "Завершён":
                            statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                            break;
                        case "Заморожен":
                            statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67E22"));
                            break;
                        default:
                            statusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));
                            break;
                    }

                    tbDetailDates.Text = $"Старт: {_selectedProject.StartDate:dd.MM.yyyy} | План: {_selectedProject.PlannedCompletionDate:dd.MM.yyyy}";
                    if (_selectedProject.ActualCompletionDate.HasValue)
                    {
                        tbDetailDates.Text += $" | Факт: {_selectedProject.ActualCompletionDate.Value:dd.MM.yyyy}";
                    }

                    if (Data.UserRole == "Администратор" || Data.UserRole == "Менеджер")
                    {
                        btnAddEvent.Visibility = Visibility.Visible;
                    }

                    LoadHistory();
                }
                else
                {
                    projectInfoPanel.Visibility = Visibility.Collapsed;
                    noProjectPanel.Visibility = Visibility.Visible;
                    dgHistory.ItemsSource = null;
                    btnAddEvent.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения деталей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (_selectedProject == null) return;

                var history = _db.ProjectActions
                    .Where(a => a.ProjectId == _selectedProject.ProjectId)
                    .OrderByDescending(a => a.DateExecution)
                    .ToList();

                dgHistory.ItemsSource = history;

                if (history.Count == 0)
                {
                    tbEmptyHistory.Visibility = Visibility.Visible;
                    dgHistory.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbEmptyHistory.Visibility = Visibility.Collapsed;
                    dgHistory.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = tbSearch.Text.ToLower();

                if (string.IsNullOrEmpty(searchText))
                {
                    dgProjects.ItemsSource = _db.Projects.Local.ToObservableCollection();
                }
                else
                {
                    var filtered = _db.Projects.Local
                        .Where(p => p.Name.ToLower().Contains(searchText) ||
                                    (p.Customer != null && p.Customer.NameOrganization.ToLower().Contains(searchText)))
                        .ToList();
                    dgProjects.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddEditProject addEdit = new AddEditProject();
                addEdit.Owner = this;
                if (addEdit.ShowDialog() == true)
                {
                    _db.ChangeTracker.Clear();
                    LoadProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна добавления: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedProject != null)
                {
                    int projectId = _selectedProject.ProjectId;

                    AddEditProject addEdit = new AddEditProject(_selectedProject);
                    addEdit.Owner = this;
                    if (addEdit.ShowDialog() == true)
                    {
                        _db.ChangeTracker.Clear();
                        LoadProjects();

                        _selectedProject = _db.Projects
                            .Include(p => p.Customer)
                            .Include(p => p.SupervisorNavigation)
                            .FirstOrDefault(p => p.ProjectId == projectId);

                        if (_selectedProject != null)
                        {
                            dgProjects.SelectedItem = _selectedProject;
                            dgProjects.ScrollIntoView(_selectedProject);
                        }

                        UpdateProjectDetails();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите проект для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна редактирования: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedProject != null)
                {
                    var result = MessageBox.Show($"Удалить проект \"{_selectedProject.Name}\"?\nЭто действие также удалит всю историю проекта.",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        var eventsToDelete = _db.ProjectActions.Where(a => a.ProjectId == _selectedProject.ProjectId);
                        _db.ProjectActions.RemoveRange(eventsToDelete);

                        _db.Projects.Remove(_selectedProject);
                        _db.SaveChanges();

                        _db.ChangeTracker.Clear();
                        LoadProjects();
                        _selectedProject = null;
                        UpdateProjectDetails();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите проект для удаления", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _db.ChangeTracker.Clear();
                LoadProjects();
                tbSearch.Text = "";
                _selectedProject = null;
                UpdateProjectDetails();
                //MessageBox.Show("Список проектов обновлён", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedProject == null)
                {
                    MessageBox.Show("Выберите проект", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                AddEditEvent addEvent = new AddEditEvent(_selectedProject.ProjectId);
                addEvent.Owner = this;
                if (addEvent.ShowDialog() == true)
                {
                    _db.Entry(_selectedProject).Reload();
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна добавления события: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgHistory.SelectedItem is ProjectAction selectedEvent)
                {
                    if (Data.UserRole != "Администратор")
                    {
                        MessageBox.Show("Только администратор может редактировать события", "Доступ запрещён",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (_selectedProject == null) return;

                    AddEditEvent addEvent = new AddEditEvent(_selectedProject.ProjectId, selectedEvent);
                    addEvent.Owner = this;
                    if (addEvent.ShowDialog() == true)
                    {
                        _db.ChangeTracker.Clear();

                        _selectedProject = _db.Projects
                            .Include(p => p.Customer)
                            .Include(p => p.SupervisorNavigation)
                            .FirstOrDefault(p => p.ProjectId == _selectedProject.ProjectId);

                        LoadProjects();

                        if (_selectedProject != null)
                        {
                            dgProjects.SelectedItem = _selectedProject;
                        }

                        LoadHistory();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна редактирования события: {ex.Message}\n\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _db.Dispose();
                _db = new ProjectNewPartsContext();

                Authorization auth = new Authorization();
                if (auth.ShowDialog() == true)
                {
                    tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

                    if (Data.UserRole == "Администратор")
                    {
                        btnAdd.IsEnabled = true;
                        btnEdit.IsEnabled = true;
                        btnDelete.IsEnabled = true;
                        btnAddEvent.IsEnabled = true;
                        btnAddEvent.Visibility = Visibility.Visible;
                    }
                    else if (Data.UserRole == "Менеджер")
                    {
                        btnAdd.IsEnabled = true;
                        btnEdit.IsEnabled = true;
                        btnDelete.IsEnabled = false;
                        btnAddEvent.IsEnabled = true;
                        btnAddEvent.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnAdd.IsEnabled = false;
                        btnEdit.IsEnabled = false;
                        btnDelete.IsEnabled = false;
                        btnAddEvent.IsEnabled = false;
                        btnAddEvent.Visibility = Visibility.Collapsed;
                    }

                    LoadProjects();
                    _selectedProject = null;
                    UpdateProjectDetails();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, не открыто ли уже окно заказчиков
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is CustomerWindow && window.IsVisible)
                    {
                        window.Activate();
                        return;
                    }
                }

                CustomerWindow customerWindow = new CustomerWindow();
                customerWindow.Owner = this; // Устанавливаем владельца
                customerWindow.Show();
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна заказчиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNomenclature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, не открыто ли уже окно 
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Nomenclature && window.IsVisible)
                    {
                        window.Activate();
                        return;
                    }
                }

                Nomenclature nomenclature = new Nomenclature();
                nomenclature.Owner = this; // Устанавливаем владельца
                nomenclature.Show();
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна номенклатуры: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDepartmentAndWarehouse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, не открыто ли уже окно 
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is DepartmentAndWarehouseWindow && window.IsVisible)
                    {
                        window.Activate();
                        return;
                    }
                }

                DepartmentAndWarehouseWindow departmentAndWarehouseWindow = new DepartmentAndWarehouseWindow();
                departmentAndWarehouseWindow.Owner = this; // Устанавливаем владельца
                departmentAndWarehouseWindow.Show();
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна номенклатуры: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}