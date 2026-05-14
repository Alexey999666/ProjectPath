using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class VwActiveProject
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string Customer { get; set; } = null!;

    public string Supervisor { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime PlannedCompletionDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Comment { get; set; }
}
