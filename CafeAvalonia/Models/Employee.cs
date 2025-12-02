using System;
using System.Collections.Generic;

namespace CafeAvalonia.Models;


public partial class Employee
{
    public int Id { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string Status { get; set; } = "Работает";

    public string Photo { get; set; } = null!;

    public string ScanContract { get; set; } = null!;

    public string Speciality { get; set; } = "официант";

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Shiftassignment> Shiftassignments { get; set; } = new List<Shiftassignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
