using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class ProjectAction
{
    public int ProjectActionsId { get; set; }

    public int ProjectId { get; set; }

    public DateTime DateExecution { get; set; }

    public string TypeOperation { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
