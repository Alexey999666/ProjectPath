using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class ProjectComposition
{
    public int ProjectCompositionId { get; set; }

    public int ProjectId { get; set; }

    public int NomenclatureId { get; set; }

    public decimal Quantity { get; set; }

    public int ResponsiblePerson { get; set; }

    public virtual Nomenclature Nomenclature { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
