using Microsoft.EntityFrameworkCore;
using ProjectPath.Modelsdb;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectPath
{
    public partial class Nomenclature : Window
    {
        private System.Windows.Threading.DispatcherTimer _timer;

        public Nomenclature()
        {
            InitializeComponent();

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
            tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";

            if (Data.UserRole != "Администратор" && Data.UserRole != "Менеджер")
            {
                btnAdd.IsEnabled = false;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }

            LoadNomenclature();
        }

        private void LoadNomenclature()
        {
            try
            {
                using (ProjectNewPartsContext _db = new ProjectNewPartsContext())
                {
                    lvNomenclature.ItemsSource = _db.Nomenclatures.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки номенклатуры: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ПОИСК 
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = tbSearch.Text.ToLower();

                // Загружаем данные из базы каждый раз заново
                using (ProjectNewPartsContext _db = new ProjectNewPartsContext())
                {
                    var allNomenclatures = _db.Nomenclatures.ToList();

                    if (string.IsNullOrEmpty(searchText))
                    {
                        lvNomenclature.ItemsSource = allNomenclatures;
                    }
                    else
                    {
                        var filtered = allNomenclatures
                            .Where(n => n.Name.ToLower().Contains(searchText) ||
                                        n.Type.ToLower().Contains(searchText))
                            .ToList();

                        lvNomenclature.ItemsSource = filtered;

                        if (filtered.Count > 0)
                        {
                            var item = filtered.First();
                            lvNomenclature.SelectedItem = item;
                            lvNomenclature.ScrollIntoView(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStockBalances(int nomenclatureId)
        {
            try
            {
                using (var _db = new ProjectNewPartsContext())
                {
                    var stockBalances = _db.StockBalances
                        .Include(sb => sb.Warehouse)
                        .Where(sb => sb.NomenclatureId == nomenclatureId)
                        .ToList();

                    lvStockBalances.ItemsSource = stockBalances;

                    // Подсчёт общей суммы
                    decimal totalQuantity = stockBalances.Sum(sb => sb.Quantity);
                    tbTotalQuantity.Text = $"Всего на складах: {totalQuantity:F2} шт.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки остатков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvNomenclature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedNomenclature = lvNomenclature.SelectedItem as Modelsdb.Nomenclature;

            if (selectedNomenclature != null)
            {
                tbSelectedInfo.Text = $"{selectedNomenclature.Name} ({selectedNomenclature.Type})";
                LoadStockBalances(selectedNomenclature.NomenclatureId);

                // Загружаем изображение для выбранного элемента
                LoadImage(selectedNomenclature);
            }
            else
            {
                tbSelectedInfo.Text = "Выберите номенклатуру";
                lvStockBalances.ItemsSource = null;
                tbTotalQuantity.Text = "Всего на складах: 0 шт.";
                ClearImage();
            }
        }

        private void LoadImage(Modelsdb.Nomenclature nomenclature)
        {
            try
            {
                if (nomenclature?.FullImage != null)
                {
                    ProductImage.Source = nomenclature.FullImage;
                    NoImageText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ClearImage();
                    NoImageText.Text = "Изображение не найдено";
                    NoImageText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отображения изображения: {ex.Message}");
                ClearImage();
                NoImageText.Text = "Ошибка загрузки изображения";
                NoImageText.Visibility = Visibility.Visible;
            }
        }

        private void ClearImage()
        {
            ProductImage.Source = null;
            NoImageText.Visibility = Visibility.Visible;
            NoImageText.Text = "Изображение не выбрано";
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadNomenclature();
            tbSearch.Text = "";
            tbSelectedInfo.Text = "Выберите номенклатуру";
            lvStockBalances.ItemsSource = null;
            tbTotalQuantity.Text = "Всего на складах: 0 шт.";
            ClearImage();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.Visibility = Visibility.Visible;
                        break;
                    }
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Передаем пустую ссылку в глобальный класс
            Data.SelectedNomenclature = null;
            // Открываем форму Добавить/Изменить
            AddEditNomenclature f = new AddEditNomenclature();
            f.Owner = this;
            f.ShowDialog();
            // Загружаем и отображаем информацию
            LoadNomenclature();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем что выбран элемент для редактирования
            if (lvNomenclature.SelectedItem != null)
            {
                // Передаем ссылку выделенной записи в глобальный класс
                Data.SelectedNomenclature = (Modelsdb.Nomenclature)lvNomenclature.SelectedItem;
                // Открываем форму Добавить/Изменить
                AddEditNomenclature f = new AddEditNomenclature();
                f.Owner = this;
                f.ShowDialog();
                // Загружаем и отображаем информацию
                LoadNomenclature();
            }
            else
            {
                MessageBox.Show("Выберите номенклатуру для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = lvNomenclature.SelectedItem as Modelsdb.Nomenclature;
            if (selected == null)
            {
                MessageBox.Show("Выберите номенклатуру для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result;
            result = MessageBox.Show("Удалить запись?", "Удаление записи",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var _db = new ProjectNewPartsContext())
                    {
                        var hasStockBalances = _db.StockBalances.Any(sb => sb.NomenclatureId == selected.NomenclatureId);

                        if (hasStockBalances)
                        {
                            MessageBox.Show($"Невозможно удалить номенклатуру \"{selected.Name}\".\n\n" +
                                            "Она числится на складах.", "Ошибка удаления",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _db.Nomenclatures.Remove(selected);
                        _db.SaveChanges();

                        LoadNomenclature();
                        tbSelectedInfo.Text = "Выберите номенклатуру";
                        lvStockBalances.ItemsSource = null;
                        tbTotalQuantity.Text = "Всего на складах: 0 шт.";
                        ClearImage();
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка удаления", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                lvNomenclature.Focus();
            }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            string mes = "Слева представлен список всей имеющейся номенклатуры, а справа наличие этой номенклатуры на складах.\n\n" +
                        "При выборе номенклатуры справа отображается её изображение и остатки на складах.";
            MessageBox.Show(mes, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Если окно закрывается не через кнопку "Назад", всё равно показываем главное окно
            if (this.Owner is MainWindow mainWindow && mainWindow.Visibility != Visibility.Visible)
            {
                mainWindow.Visibility = Visibility.Visible;
            }
        }

        // Добавьте эти методы в класс Nomenclature

        private void lvStockBalances_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Двойное нажатие - редактирование
            var selected = lvStockBalances.SelectedItem as StockBalance;
            if (selected != null)
            {
                var selectedNomenclature = lvNomenclature.SelectedItem as Modelsdb.Nomenclature;
                if (selectedNomenclature != null)
                {
                    AddEditStockBalance f = new AddEditStockBalance(selectedNomenclature.NomenclatureId, selected);
                    f.Owner = this;
                    if (f.ShowDialog() == true)
                    {
                        // Обновляем данные
                        LoadStockBalances(selectedNomenclature.NomenclatureId);
                    }
                }
            }
        }

        private void lvStockBalances_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Правая кнопка - добавление
            var selectedNomenclature = lvNomenclature.SelectedItem as Modelsdb.Nomenclature;
            if (selectedNomenclature != null)
            {
                AddEditStockBalance f = new AddEditStockBalance(selectedNomenclature.NomenclatureId);
                f.Owner = this;
                if (f.ShowDialog() == true)
                {
                    // Обновляем данные
                    LoadStockBalances(selectedNomenclature.NomenclatureId);
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите номенклатуру", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}