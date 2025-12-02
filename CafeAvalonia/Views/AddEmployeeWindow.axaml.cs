using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.Models;
using CafeAvalonia.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace CafeAvalonia.Views;

public partial class AddEmployeeWindow : Window
{
    public ObservableCollection<Employee> Employees { get; set; } = new(); // Добавьте коллекцию

    public AddEmployeeWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is AddEmployeeViewModel vm)
        {
            vm.SetWindow(this);
            vm.OnEmployeeSaved = employee =>
            {
                Employees.Add(employee);  // Добавляем в коллекцию
                this.Close(true);
            };
        }
    }
}
