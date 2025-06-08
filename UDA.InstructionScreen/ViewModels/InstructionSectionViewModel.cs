using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApp_Play.ViewModels;

// Declaring all the properties or controls I need. For now im trying to keep these as controls
// and set the properties in the code-behind. That's more according to the MVVM pattern.

public partial class InstructionSectionViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? imageContent;

    [ObservableProperty]
    private string? overlayText;
}