using Microsoft.Win32;
using ProjectPath.Modelsdb;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ProjectPath
{
    public partial class AddEditNomenclature : Window
    {
       
        ProjectNewPartsContext _db = new ProjectNewPartsContext();
      
        Modelsdb.Nomenclature _nomenclature;
     
        OpenFileDialog open = new OpenFileDialog();

        public AddEditNomenclature()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Если номенклатура не задана (null), то запись новая
            if (Data.SelectedNomenclature == null)
            {
                tbTitle.Text = "➕ Добавление номенклатуры";
                btnSave.Content = "💾 Добавить";
                
                _nomenclature = new Modelsdb.Nomenclature();
            }
            // Иначе редактирование записи
            else
            {
                tbTitle.Text = "✏️ Редактирование номенклатуры";
                btnSave.Content = "💾 Сохранить";
              
                _nomenclature = _db.Nomenclatures.Find(Data.SelectedNomenclature.NomenclatureId);

              
                tbName.Text = _nomenclature.Name;

               
                for (int i = 0; i < cbType.Items.Count; i++)
                {
                    ComboBoxItem item = cbType.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString() == _nomenclature.Type)
                    {
                        cbType.SelectedIndex = i;
                        break;
                    }
                }

           
                for (int i = 0; i < cbUnitMeasure.Items.Count; i++)
                {
                    ComboBoxItem item = cbUnitMeasure.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString() == _nomenclature.UnitMeasure)
                    {
                        cbUnitMeasure.SelectedIndex = i;
                        break;
                    }
                }

              
                if (!string.IsNullOrEmpty(_nomenclature.Image))
                {
                    LoadImage(_nomenclature.Image);
                }
            }

           
            this.DataContext = _nomenclature;
        }

        private void LoadImage(string fileName)
        {
            try
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image", fileName);
                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgPhoto.Source = bitmap;
                    tbPhotoName.Text = fileName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фото: {ex.Message}");
            }
        }

        private void btnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            open.Filter = "Все файлы | *.*| Файлы *.jpg|*.jpg| Файлы *.png|*.png";
            open.FilterIndex = 2;

            if (open.ShowDialog() == true)
            {
                BitmapImage photoImage = new BitmapImage();
                photoImage.BeginInit();
                photoImage.UriSource = new Uri(open.FileName);
                photoImage.CacheOption = BitmapCacheOption.OnLoad;
                photoImage.EndInit();
                imgPhoto.Source = photoImage;
                tbPhotoName.Text = open.SafeFileName;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения полей
            StringBuilder errors = new StringBuilder();

            if (tbName.Text.Length == 0)
                errors.AppendLine("Введите наименование");

            ComboBoxItem selectedType = cbType.SelectedItem as ComboBoxItem;
            if (selectedType == null || string.IsNullOrEmpty(selectedType.Content.ToString()))
                errors.AppendLine("Выберите тип номенклатуры");

            ComboBoxItem selectedUnit = cbUnitMeasure.SelectedItem as ComboBoxItem;
            if (selectedUnit == null || string.IsNullOrEmpty(selectedUnit.Content.ToString()))
                errors.AppendLine("Выберите единицу измерения");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Заполняем данные
                _nomenclature.Name = tbName.Text;
                _nomenclature.Type = ((ComboBoxItem)cbType.SelectedItem).Content.ToString();
                _nomenclature.UnitMeasure = ((ComboBoxItem)cbUnitMeasure.SelectedItem).Content.ToString();

                // Запоминаем имя фото если оно задано
                if (open.SafeFileName != null && open.SafeFileName.Length != 0)
                {
                    string newNamePhoto = Directory.GetCurrentDirectory() + "\\image\\" + open.SafeFileName;

                    // Создаем папку image если её нет
                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\image"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\image");
                    }

                    File.Copy(open.FileName, newNamePhoto, true);
                    _nomenclature.Image = open.SafeFileName;
                }

                if (Data.SelectedNomenclature == null)
                {
                   
                    _db.Nomenclatures.Add(_nomenclature);
                }

                
                _db.SaveChanges();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}