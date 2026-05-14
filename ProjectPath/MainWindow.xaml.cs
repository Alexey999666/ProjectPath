using ProjectPath.ModelsDB;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using ProjectPath.ModelsDB;

namespace ProjectPath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            if (Data.UserRole != "Администратор")
            {
                btnDelete.IsEnabled = false;
                btnAddEvent.IsEnabled = false;
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

            if (_selectedProject != null)
            {
                tabHistory.IsEnabled = true;
                tbSelectedProject.Text = $"История проекта: {_selectedProject.Name}";
                LoadHistory();
            }
            else
            {
                tabHistory.IsEnabled = false;
            }
        }

        private void LoadHistory()
        {
            if (_selectedProject == null) return;

            _db.ProjectActions
                .Where(a => a.ProjectId == _selectedProject.ProjectId)
                .Load();

            dgHistory.ItemsSource = _db.ProjectActions.Local.ToObservableCollection();
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
                }
            }
            else
            {
                MessageBox.Show("Выберите проект для редактирования");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject != null)
            {
                var result = MessageBox.Show($"Удалить проект \"{_selectedProject.Name}\"?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _db.Projects.Remove(_selectedProject);
                    _db.SaveChanges();
                    _ = LoadProjectsAsync();
                    dgHistory.ItemsSource = null;
                    tabHistory.IsEnabled = false;
                    _selectedProject = null;
                }
            }
            else
            {
                MessageBox.Show("Выберите проект для удаления");
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _db.ChangeTracker.Clear();
            await LoadProjectsAsync();
            tbSearch.Text = "";
        }

        private void btnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null)
            {
                MessageBox.Show("Выберите проект");
                return;
            }

            AddEditEvent addEvent = new AddEditEvent(_selectedProject.ProjectId);
            addEvent.Owner = this;
            if (addEvent.ShowDialog() == true)
            {
                LoadHistory();
            }
        }

        private void btnRefreshHistory_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            _db.Dispose();
            _db = new ProjectNewPartsContext();

            Authorization auth = new Authorization();
            if (auth.ShowDialog() == true)
            {
                tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";
                if (Data.UserRole != "Администратор")
                {
                    btnDelete.IsEnabled = false;
                    btnAddEvent.IsEnabled = false;
                }
                else
                {
                    btnDelete.IsEnabled = true;
                    btnAddEvent.IsEnabled = true;
                }
                _ = LoadProjectsAsync();
                dgHistory.ItemsSource = null;
                tabHistory.IsEnabled = false;
                _selectedProject = null;
            }
            else
            {
                Close();
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
    }
}