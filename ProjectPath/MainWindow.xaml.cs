using System;
using System.Linq;
using System.Threading.Tasks;
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

        public MainWindow()
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Authorization auth = new Authorization();
            bool? result = auth.ShowDialog();

            if (result != true)
            {
                Close();
                return;
            }

            tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

            // Настройка прав доступа
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

            await LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            await _db.Projects
                .Include(p => p.Customer)
                .Include(p => p.SupervisorNavigation)
                .LoadAsync();

            dgProjects.ItemsSource = _db.Projects.Local.ToObservableCollection();
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

                // Оформление статуса цветом
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

        private void LoadHistory()
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

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditProject addEdit = new AddEditProject();
            addEdit.Owner = this;
            if (addEdit.ShowDialog() == true)
            {
                _ = LoadProjectsAsync();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                AddEditProject addEdit = new AddEditProject(_selectedProject);
                addEdit.Owner = this;
                if (addEdit.ShowDialog() == true)
                {
                    _ = LoadProjectsAsync();
                    UpdateProjectDetails();
                }
            }
            else
            {
                MessageBox.Show("Выберите проект для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                var result = MessageBox.Show($"Удалить проект \"{_selectedProject.Name}\"?\nЭто действие также удалит всю историю проекта.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _db.Projects.Remove(_selectedProject);
                    _db.SaveChanges();
                    _ = LoadProjectsAsync();
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

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _db.ChangeTracker.Clear();
            await LoadProjectsAsync();
            tbSearch.Text = "";
            _selectedProject = null;
            UpdateProjectDetails();
        }

        private void btnAddEvent_Click(object sender, RoutedEventArgs e)
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
                LoadHistory();
            }
        }

        private void dgHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgHistory.SelectedItem is ProjectAction selectedEvent)
            {
                if (Data.UserRole != "Администратор")
                {
                    MessageBox.Show("Только администратор может редактировать события", "Доступ запрещён",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddEditEvent addEvent = new AddEditEvent(_selectedProject.ProjectId, selectedEvent);
                addEvent.Owner = this;
                if (addEvent.ShowDialog() == true)
                {
                    LoadHistory();
                }
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            _db.Dispose();
            _db = new ProjectNewPartsContext();

            Authorization auth = new Authorization();
            if (auth.ShowDialog() == true)
            {
                tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

                // Обновляем права
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

                _ = LoadProjectsAsync();
                _selectedProject = null;
                UpdateProjectDetails();
            }
            else
            {
                Close();
            }
        }
    }
}