using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class StockBalance
{
    public int StockBalanceId { get; set; }

    public int WarehouseId { get; set; }

    public int NomenclatureId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
