using System;
using System.Collections.Generic;

namespace ProjectPath.Modelsdb;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string NameOrganization { get; set; } = null!;

    public string ContactPerson { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Adress { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
