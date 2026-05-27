using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace ProjectPath.Modelsdb
{
    public partial class Nomenclature
    {
        private BitmapImage _cachedImage;

        public BitmapImage FullImage
        {
            get
            {
                try
                {
                    // Если уже загружено, возвращаем из кэша
                    if (_cachedImage != null)
                        return _cachedImage;

                    string imagePath = GetImagePath();

                    if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                    {
                        return GetDefaultImage();
                    }

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Для возможности использования в разных потоках

                    _cachedImage = bitmap;
                    return _cachedImage;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                    return GetDefaultImage();
                }
            }
        }

        private string GetImagePath()
        {
            if (string.IsNullOrEmpty(this.Image))
                return null;

            string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                                          .Parent.Parent.Parent.FullName;
            return Path.Combine(projectPath, "Image", this.Image);
        }

        private BitmapImage GetDefaultImage()
        {
            try
            {
                string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                                              .Parent.Parent.Parent.FullName;
                string defaultImagePath = Path.Combine(projectPath, "Image", "No_Image_Available.jpg");

                if (File.Exists(defaultImagePath))
                {
                    BitmapImage defaultImage = new BitmapImage();
                    defaultImage.BeginInit();
                    defaultImage.UriSource = new Uri(defaultImagePath);
                    defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                    defaultImage.EndInit();
                    defaultImage.Freeze();
                    return defaultImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения по умолчанию: {ex.Message}");
            }

            return null;
        }
    }
}