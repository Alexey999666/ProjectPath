using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class VwWarehouseStock
{
    public int WarehouseId { get; set; }

    public string WarehouseType { get; set; } = null!;

    public string ConditionalPosition { get; set; } = null!;

    public string MaterialName { get; set; } = null!;

    public string MaterialType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string UnitMeasure { get; set; } = null!;

    public string ResponsiblePerson { get; set; } = null!;
}
