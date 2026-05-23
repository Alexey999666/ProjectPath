using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Project
{
    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public int Supervisor { get; set; }

    public int DepartmentId { get; set; }

    public decimal? EstimatedDesignCost { get; set; }

    public int CustomerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime PlannedCompletionDate { get; set; }

    public DateTime? ActualCompletionDate { get; set; }

    public string? Comment { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<ProjectAction> ProjectActions { get; set; } = new List<ProjectAction>();

    public virtual ICollection<ProjectComposition> ProjectCompositions { get; set; } = new List<ProjectComposition>();

    public virtual Employee SupervisorNavigation { get; set; } = null!;
}
