using System;
using System.Collections.Generic;

namespace CafeAvalonia.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? FkEmployeeid { get; set; }

    public int? FkDishesid { get; set; }

    public int ClientsCount { get; set; }

    public int TableNumber { get; set; }


    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public virtual Dish? FkDishes { get; set; }

    public virtual Employee? FkEmployee { get; set; }
}
