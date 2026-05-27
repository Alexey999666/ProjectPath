using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string Type { get; set; } = null!;

    public int? WarehouseX { get; set; }

    public int? WarehouseY { get; set; }

    public int? WarehouseWidth { get; set; }

    public int? WarehouseHeight { get; set; }

    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
}
