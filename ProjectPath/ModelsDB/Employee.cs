using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string EmployeeLogin { get; set; } = null!;

    public string EmployeePassword { get; set; } = null!;

    public int EmployeeRoleId { get; set; }

    public string Position { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public decimal Salary { get; set; }

    public int WorkExperience { get; set; }

    public int DepartmentId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Role EmployeeRole { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
