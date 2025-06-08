using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApp_Play.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? headerContent;

    [ObservableProperty]
    private object? bodyContent;

    [ObservableProperty]
    private object? footerContent;

}
 
