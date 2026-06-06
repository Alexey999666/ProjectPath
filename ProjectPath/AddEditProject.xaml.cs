using ProjectPath.Modelsdb;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace ProjectPath
{
    public partial class AddEditProject : Window
    {
        private ProjectNewPartsContext _db;
        private Project _project;
        private bool _isEditMode;
        private Project _originalProject;

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
                _project = _db.Projects
                    .Include(p => p.Customer)
                    .Include(p => p.SupervisorNavigation)
                    .FirstOrDefault(p => p.ProjectId == project.ProjectId);

                if (_project == null)
                {
                    MessageBox.Show("Проект не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                _originalProject = new Project
                {
                    ProjectId = _project.ProjectId,
                    Name = _project.Name,
                    CustomerId = _project.CustomerId,
                    Supervisor = _project.Supervisor,
                    DepartmentId = _project.DepartmentId,
                    Status = _project.Status,
                    StartDate = _project.StartDate,
                    PlannedCompletionDate = _project.PlannedCompletionDate,
                    ActualCompletionDate = _project.ActualCompletionDate
                };

                Title = "Редактирование проекта";
                btnSave.Content = "Сохранить";
            }

            DataContext = _project;
            LoadComboBoxes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            try
            {
                _db.Employees.Load();
                cbSupervisor.ItemsSource = _db.Employees.Local.ToObservableCollection();
                cbSupervisor.DisplayMemberPath = "LastName";
                cbSupervisor.SelectedValuePath = "EmployeeId";

                _db.Departments.Load();
                cbDepartment.ItemsSource = _db.Departments.Local.ToObservableCollection();
                cbDepartment.DisplayMemberPath = "Name";
                cbDepartment.SelectedValuePath = "DepartmentId";

                _db.Customers.Load();
                cbCustomer.ItemsSource = _db.Customers.Local.ToObservableCollection();
                cbCustomer.DisplayMemberPath = "NameOrganization";
                cbCustomer.SelectedValuePath = "CustomerId";

                if (_isEditMode)
                {
                    if (_project.Supervisor > 0)
                        cbSupervisor.SelectedValue = _project.Supervisor;

                    if (_project.DepartmentId > 0)
                        cbDepartment.SelectedValue = _project.DepartmentId;

                    if (_project.CustomerId > 0)
                        cbCustomer.SelectedValue = _project.CustomerId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetChangesLog()
        {
            var changes = new System.Text.StringBuilder();

            if (_originalProject.Name != tbName.Text)
            {
                changes.AppendLine($"• Название: \"{_originalProject.Name}\" → \"{tbName.Text}\"");
            }

            int newCustomerId = (int)cbCustomer.SelectedValue;
            if (_originalProject.CustomerId != newCustomerId)
            {
                var oldCustomer = _db.Customers.Find(_originalProject.CustomerId);
                var newCustomer = _db.Customers.Find(newCustomerId);
                changes.AppendLine($"• Заказчик: \"{oldCustomer?.NameOrganization ?? "—"}\" → \"{newCustomer?.NameOrganization ?? "—"}\"");
            }

            int newSupervisorId = (int)cbSupervisor.SelectedValue;
            if (_originalProject.Supervisor != newSupervisorId)
            {
                var oldSupervisor = _db.Employees.Find(_originalProject.Supervisor);
                var newSupervisor = _db.Employees.Find(newSupervisorId);
                string oldName = oldSupervisor != null ? oldSupervisor.LastName : "—";
                string newName = newSupervisor != null ? newSupervisor.LastName : "—";
                changes.AppendLine($"• Руководитель: \"{oldName}\" → \"{newName}\"");
            }

            int newDepartmentId = (int)cbDepartment.SelectedValue;
            if (_originalProject.DepartmentId != newDepartmentId)
            {
                var oldDepartment = _db.Departments.Find(_originalProject.DepartmentId);
                var newDepartment = _db.Departments.Find(newDepartmentId);
                string oldName = oldDepartment != null ? oldDepartment.Name : "—";
                string newName = newDepartment != null ? newDepartment.Name : "—";
                changes.AppendLine($"• Отдел: \"{oldName}\" → \"{newName}\"");
            }

            string newStatus = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            if ((_originalProject.Status ?? "") != newStatus)
            {
                changes.AppendLine($"• Статус: \"{_originalProject.Status ?? "—"}\" → \"{newStatus}\"");
            }

            if (dpStartDate.SelectedDate.HasValue && _originalProject.StartDate != dpStartDate.SelectedDate.Value)
            {
                changes.AppendLine($"• Дата начала: {_originalProject.StartDate:dd.MM.yyyy} → {dpStartDate.SelectedDate.Value:dd.MM.yyyy}");
            }

            if (dpPlannedDate.SelectedDate.HasValue && _originalProject.PlannedCompletionDate != dpPlannedDate.SelectedDate.Value)
            {
                changes.AppendLine($"• Плановая дата: {_originalProject.PlannedCompletionDate:dd.MM.yyyy} → {dpPlannedDate.SelectedDate.Value:dd.MM.yyyy}");
            }

            string oldActual = _originalProject.ActualCompletionDate?.ToString("dd.MM.yyyy") ?? "—";
            string newActual = dpActualDate.SelectedDate?.ToString("dd.MM.yyyy") ?? "—";
            if (oldActual != newActual)
            {
                changes.AppendLine($"• Фактическая дата: {oldActual} → {newActual}");
            }

            return changes.ToString();
        }

        private void AddHistoryRecord(string typeOperation, string comment)
        {
            try
            {
                var historyRecord = new ProjectAction
                {
                    ProjectId = _project.ProjectId,
                    DateExecution = DateTime.Now,
                    TypeOperation = typeOperation,
                    Comment = comment
                };

                _db.ProjectActions.Add(historyRecord);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка записи истории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //Метод проверки стоимости
        private bool IsValidCost(string costText)
        {

            if (string.IsNullOrWhiteSpace(costText))
                return false;


            string normalized = costText.Replace(',', '.');


            if (!Regex.IsMatch(normalized, @"^\d+\.?\d*$"))
                return false;


            if (normalized.StartsWith("."))
                return false;


            if (normalized.EndsWith("."))
                return false;

            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                tbName.Focus();
                return;
            }


            if (cbSupervisor.SelectedItem == null)
            {
                MessageBox.Show("Выберите руководителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (cbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказчика", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (cbDepartment.SelectedItem == null)
            {
                MessageBox.Show("Выберите отдел", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!dpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату начала проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!dpPlannedDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите плановую дату завершения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (dpStartDate.SelectedDate.Value.Date > dpPlannedDate.SelectedDate.Value.Date)
            {
                MessageBox.Show("Дата начала проекта не может быть позже плановой даты завершения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return;
            }

           
            if (dpActualDate.SelectedDate.HasValue)
            {
                if (dpActualDate.SelectedDate.Value.Date < dpStartDate.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Фактическая дата завершения не может быть раньше даты начала проекта",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpActualDate.Focus();
                    return;
                }
            }


            if (tbCost != null && tbCost.Visibility == Visibility.Visible)
            {

                if (string.IsNullOrWhiteSpace(tbCost.Text))
                {
                    MessageBox.Show("Введите стоимость проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbCost.Focus();
                    return;
                }


                if (!IsValidCost(tbCost.Text))
                {
                    MessageBox.Show("Введите корректную стоимость\n\nПримеры:\n1000\n1000.50\n1000,50",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbCost.Focus();
                    return;
                }


                string normalized = tbCost.Text.Replace(',', '.');
                decimal cost = decimal.Parse(normalized, System.Globalization.CultureInfo.InvariantCulture);


                if (cost <= 0)
                {
                    MessageBox.Show("Стоимость должна быть больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbCost.Focus();
                    return;
                }


                _project.EstimatedDesignCost = cost;
            }

            try
            {
                string changesLog = "";

                if (_isEditMode)
                {
                    changesLog = GetChangesLog();
                }

                _project.Name = tbName.Text;
                _project.Supervisor = (int)cbSupervisor.SelectedValue;
                _project.DepartmentId = (int)cbDepartment.SelectedValue;
                _project.CustomerId = (int)cbCustomer.SelectedValue;

                if (cbStatus.SelectedItem is ComboBoxItem selectedStatus)
                {
                    _project.Status = selectedStatus.Content.ToString();
                }

                _project.StartDate = dpStartDate.SelectedDate.Value;
                _project.PlannedCompletionDate = dpPlannedDate.SelectedDate.Value;
                _project.ActualCompletionDate = dpActualDate.SelectedDate;

                if (!_isEditMode)
                {
                    _db.Projects.Add(_project);
                    _db.SaveChanges();
                    AddHistoryRecord("Создание проекта", $"Создан проект \"{_project.Name}\"");
                }
                else
                {
                    _db.SaveChanges();
                    if (!string.IsNullOrEmpty(changesLog))
                    {
                        AddHistoryRecord("Редактирование проекта", changesLog);
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}