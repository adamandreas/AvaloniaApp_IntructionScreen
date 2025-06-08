using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApp_Play.Views;

public partial class InstructionSection : UserControl
{
    public InstructionSection()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}