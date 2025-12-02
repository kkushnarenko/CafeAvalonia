using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeAvalonia.Views;

public partial class EmployeeDetailsWindow : Window
{
    public EmployeeDetailsWindow()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}