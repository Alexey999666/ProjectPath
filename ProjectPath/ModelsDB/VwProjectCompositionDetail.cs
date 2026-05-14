using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class VwProjectCompositionDetail
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string PartName { get; set; } = null!;

    public string PartType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string UnitMeasure { get; set; } = null!;

    public string ResponsiblePerson { get; set; } = null!;
}
