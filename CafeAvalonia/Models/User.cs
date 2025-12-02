using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafeAvalonia.Models;

public partial class User
{
    public int Id { get; set; }

    
    public string Login { get; set; } = null!;

   
    public string Email { get; set; } = null!;
  
    public string Password { get; set; } = null!;

    public int? FkEmployeeId { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
