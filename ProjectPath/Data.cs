using ProjectPath.Modelsdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPath
{
    public static class Data
    {
        public static bool IsLoggedIn { get; set; } = false;
        public static Employee? CurrentUser { get; set; }
        public static string UserRole { get; set; } = "Гость";
        public static string UserFullName { get; set; } = "";
        public static ProjectAction? SelectedEvent { get; set; }

        public static Modelsdb.Nomenclature? SelectedNomenclature { get; set; }
        public static StockBalance? stockBalance { get; set; }

        public static Department? SelectedDepartment { get; set; }
        public static Warehouse? SelectedWarehouse { get; set; }
        public static int TempX { get; set; }
        public static int TempY { get; set; }
    }
}
