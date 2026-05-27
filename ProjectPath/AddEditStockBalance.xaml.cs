using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectPath.Modelsdb;

namespace ProjectPath
{
    public partial class AddEditStockBalance : Window
    {
        private ProjectNewPartsContext _db;
        private StockBalance _stockBalance;
        private int _nomenclatureId;
        private bool _isEditMode;

        public AddEditStockBalance(int nomenclatureId, StockBalance? stockBalance = null)
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();
            _nomenclatureId = nomenclatureId;

            if (stockBalance == null)
            {
                _isEditMode = false;
                _stockBalance = new StockBalance();
                _stockBalance.NomenclatureId = nomenclatureId;
                _stockBalance.Quantity = 0;
                Title = "Добавление остатков";
                btnSave.Content = "Добавить";
                btnDelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                _isEditMode = true;
                _stockBalance = _db.StockBalances.Find(stockBalance.StockBalanceId);
                Title = "Редактирование остатков";
                btnSave.Content = "Изменить";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем склады в ComboBox
            var warehouses = _db.Warehouses.ToList();
            cbWarehouse.ItemsSource = warehouses;

            if (_isEditMode && _stockBalance != null)
            {
                // Устанавливаем выбранный склад
                cbWarehouse.SelectedValue = _stockBalance.WarehouseId;
                tbQuantity.Text = _stockBalance.Quantity.ToString();
            }
            else
            {
                if (cbWarehouse.Items.Count > 0)
                    cbWarehouse.SelectedIndex = 0;
                tbQuantity.Text = "0";
            }

            cbWarehouse.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбора склада
            if (cbWarehouse.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка количества
            if (!decimal.TryParse(tbQuantity.Text, out decimal quantity))
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _stockBalance.WarehouseId = (int)cbWarehouse.SelectedValue;
                _stockBalance.Quantity = quantity;

                if (!_isEditMode)
                {
                    _db.StockBalances.Add(_stockBalance);
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
            var result = MessageBox.Show("Удалить запись об остатках?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.StockBalances.Remove(_stockBalance);
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