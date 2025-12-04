using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CafeAvalonia.ViewModels;

namespace CafeAvalonia.Views;

public partial class ShiftReportWindow : Window
{
    public ShiftReportWindow()
    {
        InitializeComponent();
        DataContext = new ShiftReportViewModel(this);
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}