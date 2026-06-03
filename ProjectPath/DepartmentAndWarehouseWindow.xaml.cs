using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using ProjectPath.Modelsdb;

namespace ProjectPath
{
    public partial class DepartmentAndWarehouseWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer _timer;
        private ProjectNewPartsContext _db;

        public DepartmentAndWarehouseWindow()
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
            tbUserInfo.Text = $"{Data.UserFullName} ({Data.UserRole})";
            LoadMap();
        }

        private void LoadMap()
        {
            try
            {
                MapCanvas.Children.Clear(); // Очищаем всё

                // Загружаем цеха (Department)
                var departments = _db.Departments.ToList();
                foreach (var dept in departments)
                {
                    CreateDepartmentRectangle(dept);
                }

                // Загружаем склады (Warehouse)
                var warehouses = _db.Warehouses.ToList();
                foreach (var warehouse in warehouses)
                {
                    CreateWarehouseRectangle(warehouse);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки карты: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDepartmentRectangle(Department dept)
        {
            int x = dept.DepartmentX;
            int y = dept.DepartmentY;
            int width = dept.DepartmentWidth;
            int height = dept.DepartmentHeight;

            Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113)); // Зеленый для цеха
            rect.Stroke = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            rect.StrokeThickness = 2;
            rect.RadiusX = 8;
            rect.RadiusY = 8;
            rect.Tag = dept; // Сохраняем объект в Tag
            rect.Cursor = Cursors.Hand;

            // ToolTip с данными
            int employeeCount = _db.Employees.Count(e => e.DepartmentId == dept.DepartmentId);
            rect.ToolTip = $"🏭 ЦЕХ\n\n" +
                          $"Название: {dept.Name}\n" +
                          $"Тип: {dept.Type}\n" +
                          $"Сотрудников: {employeeCount}\n" +
                          $"Размер: {width}×{height}\n\n" +
                          $"Двойной клик - редактирование";

            // Обработчики событий
            rect.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
            rect.MouseRightButtonDown += Rectangle_MouseRightButtonDown;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            MapCanvas.Children.Add(rect);
        }

        private void CreateWarehouseRectangle(Warehouse warehouse)
        {
            int x = warehouse.WarehouseX;
            int y = warehouse.WarehouseY;
            int width = warehouse.WarehouseWidth;
            int height = warehouse.WarehouseHeight;

            Rectangle rect = new Rectangle();
            rect.Width = width;
            rect.Height = height;
            rect.Fill = new SolidColorBrush(Color.FromRgb(230, 126, 34)); // Оранжевый для склада
            rect.Stroke = new SolidColorBrush(Color.FromRgb(211, 84, 0));
            rect.StrokeThickness = 2;
            rect.RadiusX = 8;
            rect.RadiusY = 8;
            rect.Tag = warehouse;
            rect.Cursor = Cursors.Hand;

            // Считаем количество номенклатуры на складе
            decimal totalQuantity = _db.StockBalances
                .Where(sb => sb.WarehouseId == warehouse.WarehouseId)
                .Sum(sb => sb.Quantity);

            // ToolTip с данными
            rect.ToolTip = $"🏪 СКЛАД\n\n" +
                          $"Тип склада: {warehouse.Type}\n" +
                          $"Кол-во номенклатуры: {totalQuantity:F2} шт.\n" +
                          $"Размер: {width}×{height}\n\n" +
                          $"Двойной клик - редактирование";

            // Обработчики событий
            rect.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
            rect.MouseRightButtonDown += Rectangle_MouseRightButtonDown;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            MapCanvas.Children.Add(rect);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Двойной клик - редактирование
                Rectangle rect = sender as Rectangle;
                if (rect.Tag is Department dept)
                {
                    EditDepartment(dept);
                }
                else if (rect.Tag is Warehouse warehouse)
                {
                    EditWarehouse(warehouse);
                }
            }
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Предотвращаем открытие контекстного меню Canvas
            Rectangle rect = sender as Rectangle;
            
            if (rect.Tag is Department dept)
            {
                EditDepartment(dept);
            }
            else if (rect.Tag is Warehouse warehouse)
            {
                EditWarehouse(warehouse);
            }
        }

        private void EditDepartment(Department dept)
        {
            if (Data.UserRole == "Сотрудник" || Data.UserRole == "Гость")
            {
                MessageBox.Show("Редактирование может производить только администратор или менеджер", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Data.SelectedDepartment = dept;
            AddEditDepartmentWindow f = new AddEditDepartmentWindow();
            f.Owner = this;
            if (f.ShowDialog() == true)
            {
                // Обновляем контекст, чтобы получить свежие данные
                _db = new ProjectNewPartsContext();
                LoadMap();
            }
        }

        private void EditWarehouse(Warehouse warehouse)
        {
            if (Data.UserRole == "Сотрудник" || Data.UserRole == "Гость")
            {
                MessageBox.Show("Редактирование может производить только администратор или менеджер", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Data.SelectedWarehouse = warehouse;
            AddEditWarehouseWindow f = new AddEditWarehouseWindow();
            f.Owner = this;
            if (f.ShowDialog() == true)
            {
                _db = new ProjectNewPartsContext();
                LoadMap();
            }
        }

        private void MapCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Координаты клика на Canvas
            Point clickPoint = e.GetPosition(MapCanvas);
            Data.TempX = (int)clickPoint.X;
            Data.TempY = (int)clickPoint.Y;
            
            // Контекстное меню откроется автоматически
        }

        private void AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (Data.UserRole == "Сотрудник" || Data.UserRole == "Гость")
            {
                MessageBox.Show("Добавление может производить только администратор или менеджер", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Data.SelectedDepartment = null;
            AddEditDepartmentWindow f = new AddEditDepartmentWindow();
            f.Owner = this;
            if (f.ShowDialog() == true)
            {
                LoadMap();
            }
        }

        private void AddWarehouse_Click(object sender, RoutedEventArgs e)
        {
            if (Data.UserRole == "Сотрудник" || Data.UserRole == "Гость")
            {
                MessageBox.Show("Добавление может производить только администратор или менеджер", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Data.SelectedWarehouse = null;
            AddEditWarehouseWindow f = new AddEditWarehouseWindow();
            f.Owner = this;
            if (f.ShowDialog() == true)
            {
                LoadMap();
            }
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Если окно закрывается не через кнопку "Назад", всё равно показываем главное окно
            if (this.Owner is MainWindow mainWindow && mainWindow.Visibility != Visibility.Visible)
            {
                mainWindow.Visibility = Visibility.Visible;
            }
        }
    }
}