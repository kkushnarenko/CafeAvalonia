using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.Models;
using CafeAvalonia.ViewModels;

namespace CafeAvalonia.Views;

public partial class ShiftsWindow : Window
{
    public ShiftsWindow(User currentUser)
    {
        InitializeComponent();
        DataContext = new ShiftsViewModel(currentUser, new BdcafeContext());

    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}