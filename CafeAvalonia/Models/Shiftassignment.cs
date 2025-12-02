using System;
using System.Collections.Generic;

namespace CafeAvalonia.Models;

public partial class Shiftassignment
{
    public int Id { get; set; }

    public int? FkShiftsid { get; set; }

    public int? FkEmployeeid { get; set; }

    public virtual Employee? FkEmployee { get; set; }

    public virtual Shift? FkShifts { get; set; }
}
