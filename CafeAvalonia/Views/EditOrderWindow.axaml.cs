using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CafeAvalonia.Views;

public partial class EditOrderWindow : Window
{
    public EditOrderWindow()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}