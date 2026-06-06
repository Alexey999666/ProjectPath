using ProjectPath.Modelsdb;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectPath
{
    public partial class AddEditWarehouseWindow : Window
    {
        private ProjectNewPartsContext _db;
        private Warehouse _warehouse;
        private bool _isEditMode;

        public AddEditWarehouseWindow()
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();

            if (Data.SelectedWarehouse == null)
            {
                _isEditMode = false;
                _warehouse = new Warehouse();
                Title = "Добавление склада";
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
                _warehouse = _db.Warehouses.Find(Data.SelectedWarehouse.WarehouseId);
                Title = "Редактирование склада";
                btnSave.Content = "Изменить";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _warehouse != null)
            {
                for (int i = 0; i < cbType.Items.Count; i++)
                {
                    ComboBoxItem item = cbType.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString() == _warehouse.Type)
                    {
                        cbType.SelectedIndex = i;
                        break;
                    }
                }

                tbX.Text = _warehouse.WarehouseX.ToString();
                tbY.Text = _warehouse.WarehouseY.ToString();
                tbWidth.Text = _warehouse.WarehouseWidth.ToString();
                tbHeight.Text = _warehouse.WarehouseHeight.ToString();
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
            int? excludeId = _isEditMode ? _warehouse.WarehouseId : (int?)null;

            // Вызываем метод проверки из Helper класса
            return RectangleCollisionHelper.HasCollision(
                x, y, width, height,
                departments, warehouses,
                null, excludeId  // excludeDepartmentId = null, т.к. проверяем склад
            );
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбора типа
            if (cbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип склада", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка координат X и Y
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

            // Проверка размеров
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
                _warehouse.Type = ((ComboBoxItem)cbType.SelectedItem).Content.ToString();
                _warehouse.WarehouseX = x;
                _warehouse.WarehouseY = y;
                _warehouse.WarehouseWidth = width;
                _warehouse.WarehouseHeight = height;

                if (!_isEditMode)
                {
                    _db.Warehouses.Add(_warehouse);
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
            bool hasStock = _db.StockBalances.Any(sb => sb.WarehouseId == _warehouse.WarehouseId);

            if (hasStock)
            {
                MessageBox.Show("Невозможно удалить склад, так как на нём есть остатки материалов.\n\n" +
                               "Сначала удалите или переместите все остатки с этого склада.",
                               "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить склад тип \"{_warehouse.Type}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Warehouses.Remove(_warehouse);
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