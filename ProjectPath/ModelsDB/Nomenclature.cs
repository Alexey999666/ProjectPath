using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Nomenclature
{
    public int NomenclatureId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string UnitMeasure { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<ProjectComposition> ProjectCompositions { get; set; } = new List<ProjectComposition>();

    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
}
