using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ApvPlayer.Views;

public partial class AboutDialog : UserControl
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}