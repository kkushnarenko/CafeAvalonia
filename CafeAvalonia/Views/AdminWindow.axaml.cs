using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.ViewModels;

namespace CafeAvalonia.Views;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
        DataContext = new AdminViewModel();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}