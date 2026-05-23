using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class ProjectComposition
{
    public int ProjectCompositionId { get; set; }

    public int ProjectId { get; set; }

    public int NomenclatureId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
