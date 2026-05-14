using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class VwPartsLocation
{
    public int NomenclatureId { get; set; }

    public string PartName { get; set; } = null!;

    public string PartType { get; set; } = null!;

    public string UnitMeasure { get; set; } = null!;

    public string CurrentLocation { get; set; } = null!;

    public string? LocationName { get; set; }

    public decimal? AvailableQuantity { get; set; }

    public string? ResponsiblePerson { get; set; }
}
