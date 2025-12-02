using System;
using System.Collections.Generic;

namespace CafeAvalonia.Models;

public partial class Shift
{
    public int Id { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateFinis { get; set; }

    public virtual ICollection<Shiftassignment> Shiftassignments { get; set; } = new List<Shiftassignment>();
}
