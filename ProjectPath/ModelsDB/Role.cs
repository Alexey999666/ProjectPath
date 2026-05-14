using System;
using System.Collections.Generic;

namespace ProjectPath.ModelsDB;

public partial class Role
{
    public int UserRoleId { get; set; }

    public string Role1 { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
