using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.Models;
using CafeAvalonia.ViewModels;

namespace CafeAvalonia.Views;   

public partial class OrdersWindow : Window
{
    public OrdersWindow(User currentUser)
    {
        InitializeComponent();
        DataContext = new OrdersViewModel(currentUser);
    }
   

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}