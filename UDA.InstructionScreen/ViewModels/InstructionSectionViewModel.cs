using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApp_Play.ViewModels;

public partial class InstructionSectionViewModel : ObservableObject
{
    [ObservableProperty]
    private object? imageContent;

    [ObservableProperty]
    private string? overlayText;
}