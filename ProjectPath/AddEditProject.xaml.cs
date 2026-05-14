using ProjectPath.ModelsDB;
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

namespace ProjectPath
{
    /// <summary>
    /// Логика взаимодействия для AddEditProject.xaml
    /// </summary>
    public partial class AddEditProject : Window
    {
        private ProjectNewPartsContext _db;
        private Project _project;
        private bool _isEditMode;

        public AddEditProject(Project? project = null)
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();

            if (project == null)
            {
                _isEditMode = false;
                _project = new Project();
                _project.StartDate = DateTime.Now;
                _project.PlannedCompletionDate = DateTime.Now.AddMonths(1);
                _project.Status = "Активный";
                Title = "Добавление проекта";
                btnSave.Content = "Добавить";
            }
            else
            {
                _isEditMode = true;
                _project = _db.Projects.Find(project.ProjectId);
                Title = "Редактирование проекта";
                btnSave.Content = "Изменить";
            }

            DataContext = _project;
            LoadComboBoxes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем выбранный статус в ComboBox
            if (!string.IsNullOrEmpty(_project.Status))
            {
                foreach (ComboBoxItem item in cbStatus.Items)
                {
                    if (item.Content.ToString() == _project.Status)
                    {
                        cbStatus.SelectedItem = item;
                        break;
                    }
                }
            }

            tbName.Focus();
        }

        private void LoadComboBoxes()
        {
            cbSupervisor.ItemsSource = _db.Employees.ToList();
            cbDepartment.ItemsSource = _db.Departments.ToList();
            cbCustomer.ItemsSource = _db.Customers.ToList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название проекта", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbName.Focus();
                return;
            }

            if (cbSupervisor.SelectedItem == null)
            {
                MessageBox.Show("Выберите руководителя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказчика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранный статус из ComboBox
            if (cbStatus.SelectedItem is ComboBoxItem selectedStatus)
            {
                _project.Status = selectedStatus.Content.ToString();
            }

            try
            {
                if (!_isEditMode)
                {
                    _db.Projects.Add(_project);
                }
                _db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
