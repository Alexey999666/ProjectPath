using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectPath.Modelsdb;

namespace ProjectPath
{
    public static class RectangleCollisionHelper
    {
        /// <summary>
        /// Проверяет, пересекается ли новый прямоугольник с существующими
        /// </summary>
        /// <param name="newX">X координата нового прямоугольника</param>
        /// <param name="newY">Y координата нового прямоугольника</param>
        /// <param name="newWidth">Ширина нового прямоугольника</param>
        /// <param name="newHeight">Высота нового прямоугольника</param>
        /// <param name="departments">Список существующих цехов</param>
        /// <param name="warehouses">Список существующих складов</param>
        /// <param name="excludeDepartmentId">ID цеха, который нужно исключить из проверки (для редактирования)</param>
        /// <param name="excludeWarehouseId">ID склада, который нужно исключить из проверки (для редактирования)</param>
        /// <returns>true - есть пересечение, false - нет пересечений</returns>
        public static bool HasCollision(
            int newX, int newY, int newWidth, int newHeight,
            List<Department> departments,
            List<Warehouse> warehouses,
            int? excludeDepartmentId = null,
            int? excludeWarehouseId = null)
        {
            // Создаем прямоугольник для проверки
            Rect newRect = new Rect(newX, newY, newWidth, newHeight);

            // Проверяем пересечение с цехами
            foreach (var dept in departments)
            {
                
                if (excludeDepartmentId.HasValue && dept.DepartmentId == excludeDepartmentId.Value)
                    continue;

                Rect existingRect = new Rect(
                    dept.DepartmentX,
                    dept.DepartmentY,
                    dept.DepartmentWidth,
                    dept.DepartmentHeight
                );

                if (newRect.IntersectsWith(existingRect))
                    return true;
            }

            // Проверяем пересечение со складами
            foreach (var warehouse in warehouses)
            {
              
                if (excludeWarehouseId.HasValue && warehouse.WarehouseId == excludeWarehouseId.Value)
                    continue;

                Rect existingRect = new Rect(
                    warehouse.WarehouseX,
                    warehouse.WarehouseY,
                    warehouse.WarehouseWidth,
                    warehouse.WarehouseHeight
                );

                if (newRect.IntersectsWith(existingRect))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Получает описание конфликта для сообщения пользователю
        /// </summary>
        public static string GetCollisionMessage()
        {
            return "Невозможно сохранить! Объект пересекается с существующим цехом или складом.\n\n" +
                   "Пожалуйста, выберите другое место или измените размеры, чтобы объекты не пересекались.";
        }
    }
}