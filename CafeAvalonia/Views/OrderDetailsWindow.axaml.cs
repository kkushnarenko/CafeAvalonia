using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.ViewModels;

namespace CafeAvalonia.Views;

public partial class OrderDetailsWindow : Window
{
    public OrderDetailsWindow()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}