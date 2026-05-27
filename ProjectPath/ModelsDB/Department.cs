using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int? DepartmentX { get; set; }

    public int? DepartmentY { get; set; }

    public int? DepartmentWidth { get; set; }

    public int? DepartmentHeight { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
