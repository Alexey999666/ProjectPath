using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public int Capacity { get; set; }

    public string ConditionalPosition { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
}
