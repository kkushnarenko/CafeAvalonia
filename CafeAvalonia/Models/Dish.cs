using System;
using System.Collections.Generic;

namespace CafeAvalonia.Models;

public partial class Dish
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Incridients { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
