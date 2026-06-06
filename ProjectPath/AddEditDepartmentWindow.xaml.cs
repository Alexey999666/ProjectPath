using ProjectPath.Modelsdb;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectPath
{
    public partial class AddEditDepartmentWindow : Window
    {
        private ProjectNewPartsContext _db;
        private Department _department;
        private bool _isEditMode;

        public AddEditDepartmentWindow()
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();

            if (Data.SelectedDepartment == null)
            {
                _isEditMode = false;
                _department = new Department();
                Title = "Добавление цеха";
                btnSave.Content = "Добавить";
                btnSave.Height = 40;
                btnCancel.Height = 40;
                btnDelete.Visibility = Visibility.Collapsed;

                tbX.Text = Data.TempX.ToString();
                tbY.Text = Data.TempY.ToString();
            }
            else
            {
                _isEditMode = true;
                _department = _db.Departments.Find(Data.SelectedDepartment.DepartmentId);
                Title = "Редактирование цеха";
                btnSave.Content = "Изменить";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _department != null)
            {
                tbName.Text = _department.Name;

                for (int i = 0; i < cbType.Items.Count; i++)
                {
                    ComboBoxItem item = cbType.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString() == _department.Type)
                    {
                        cbType.SelectedIndex = i;
                        break;
                    }
                }

                tbX.Text = _department.DepartmentX.ToString();
                tbY.Text = _department.DepartmentY.ToString();
                tbWidth.Text = _department.DepartmentWidth.ToString();
                tbHeight.Text = _department.DepartmentHeight.ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(tbWidth.Text)) tbWidth.Text = "100";
                if (string.IsNullOrEmpty(tbHeight.Text)) tbHeight.Text = "80";
            }

            cbType.Focus();
        }

        // НОВЫЙ МЕТОД: Проверка пересечения с другими объектами
        private bool CheckForCollision(int x, int y, int width, int height)
        {
            // Получаем все существующие объекты из БД
            var departments = _db.Departments.ToList();
            var warehouses = _db.Warehouses.ToList();

            // Определяем ID текущего объекта (для редактирования)
            int? excludeId = _isEditMode ? _department.DepartmentId : (int?)null;

            // Вызываем метод проверки из Helper класса
            return RectangleCollisionHelper.HasCollision(
                x, y, width, height,
                departments, warehouses,
                excludeId, null  // excludeWarehouseId = null, т.к. проверяем цех
            );
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация имени
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название цеха", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип цеха", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсинг координат и размеров
            if (!int.TryParse(tbX.Text, out int x))
            {
                MessageBox.Show("Введите корректную координату X", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbY.Text, out int y))
            {
                MessageBox.Show("Введите корректную координату Y", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbWidth.Text, out int width))
            {
                MessageBox.Show("Введите корректную ширину", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbHeight.Text, out int height))
            {
                MessageBox.Show("Введите корректную высоту", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка размеров
            if (width < 20 || width > 100)
            {
                MessageBox.Show("Ширина должна быть от 20 до 100", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (height < 20 || height > 100)
            {
                MessageBox.Show("Высота должна быть от 20 до 100", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка границ Canvas
            if (x < 0 || x + width > 2000)
            {
                MessageBox.Show($"Координата X должна быть от 0 до {2000 - width} (с учётом ширины)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (y < 0 || y + height > 1500)
            {
                MessageBox.Show($"Координата Y должна быть от 0 до {1500 - height} (с учётом высоты)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // НОВАЯ ПРОВЕРКА: Пересечение с другими объектами
            if (CheckForCollision(x, y, width, height))
            {
                MessageBox.Show(RectangleCollisionHelper.GetCollisionMessage(),
                    "Конфликт объектов", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _department.Name = tbName.Text;
                _department.Type = ((ComboBoxItem)cbType.SelectedItem).Content.ToString();
                _department.DepartmentX = x;
                _department.DepartmentY = y;
                _department.DepartmentWidth = width;
                _department.DepartmentHeight = height;

                if (!_isEditMode)
                {
                    _db.Departments.Add(_department);
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool hasEmployees = _db.Employees.Any(emp => emp.DepartmentId == _department.DepartmentId);

            if (hasEmployees)
            {
                MessageBox.Show("Невозможно удалить цех, так как в нём есть сотрудники.\n\n" +
                               "Сначала переместите или удалите всех сотрудников из этого цеха.",
                               "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить цех \"{_department.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Departments.Remove(_department);
                    _db.SaveChanges();
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}