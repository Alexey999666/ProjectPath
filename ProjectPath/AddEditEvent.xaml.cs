using ProjectPath.Modelsdb;
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
    /// Логика взаимодействия для AddEditEvent.xaml
    /// </summary>
    public partial class AddEditEvent : Window
    {
        private ProjectNewPartsContext _db;
        private ProjectAction _event;
        private int _projectId;
        private bool _isEditMode;

        public AddEditEvent(int projectId, ProjectAction? eventAction = null)
        {
            InitializeComponent();
            _db = new ProjectNewPartsContext();
            _projectId = projectId;

            if (eventAction == null)
            {
                _isEditMode = false;
                _event = new ProjectAction();
                _event.ProjectId = projectId;
                _event.DateExecution = DateTime.Now;
                _event.TypeOperation = "Событие";
                _event.Comment = "";
                Title = "Добавление события";
                btnSave.Content = "Добавить";
                btnDelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                _isEditMode = true;
                _event = _db.ProjectActions.Find(eventAction.ProjectActionsId);
                Title = "Редактирование события";
                btnSave.Content = "Изменить";
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (_event.DateExecution != DateTime.MinValue)
            {
                dpDateExecution.SelectedDate = _event.DateExecution.Date;
                tbTime.Text = _event.DateExecution.ToString("HH:mm");
            }
            else
            {
                dpDateExecution.SelectedDate = DateTime.Now;
            }

            tbComment.Text = _event.Comment;


            if (!string.IsNullOrEmpty(_event.TypeOperation))
            {
                foreach (ComboBoxItem item in cbTypeOperation.Items)
                {
                    if (item.Content.ToString() == _event.TypeOperation)
                    {
                        cbTypeOperation.SelectedItem = item;
                        break;
                    }
                }
            }

            cbTypeOperation.Focus();
        }
        // Проверка формата времени
        private bool IsValidTime(string time)
        {
            // Проверка формата ЧЧ:ММ
            if (string.IsNullOrWhiteSpace(time))
                return false;

            
            if (time.Length != 5)
                return false;

          
            if (time[2] != ':')
                return false;

           
            if (!char.IsDigit(time[0]) || !char.IsDigit(time[1]) ||
                !char.IsDigit(time[3]) || !char.IsDigit(time[4]))
                return false;

            // Получаем часы и минуты
            int hours = int.Parse(time.Substring(0, 2));
            int minutes = int.Parse(time.Substring(3, 2));

            // Проверка диапазона
            if (hours < 0 || hours > 23)
                return false;

            if (minutes < 0 || minutes > 59)
                return false;

            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
          
            if (cbTypeOperation.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип события", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

         
            if (!dpDateExecution.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату события", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDateExecution.Focus();
                return;
            }

            
            if (string.IsNullOrWhiteSpace(tbTime.Text))
            {
                MessageBox.Show("Введите время события в формате ЧЧ:ММ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbTime.Focus();
                return;
            }

            // Проверка формата времени
            if (!IsValidTime(tbTime.Text))
            {
                MessageBox.Show("Введите корректное время в формате ЧЧ:ММ\n\nПримеры:\n09:30\n14:00\n23:59",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                tbTime.Focus();
                return;
            }

            try
            {
                DateTime selectedDate = dpDateExecution.SelectedDate.Value;
                string[] timeParts = tbTime.Text.Split(':');
                int hours = int.Parse(timeParts[0]);
                int minutes = int.Parse(timeParts[1]);
                _event.DateExecution = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, hours, minutes, 0);

                var selectedItem = cbTypeOperation.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    _event.TypeOperation = selectedItem.Content.ToString();
                }

                _event.Comment = tbComment.Text;

                if (!_isEditMode)
                {
                    _db.ProjectActions.Add(_event);
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
            var result = MessageBox.Show("Удалить событие?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.ProjectActions.Remove(_event);
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