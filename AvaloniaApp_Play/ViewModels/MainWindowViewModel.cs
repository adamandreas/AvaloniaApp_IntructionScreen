using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Labs.Gif; 

namespace AvaloniaApp_Play.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? imageContent;

    private int height = 20;

    public MainWindowViewModel()
    { 
        StartAlternating();  
    }

    // Hello world
    private async void StartAlternating()
    {
        var image1 = new Bitmap("Assets/avalonia-logo.ico");
        var image2 = new Bitmap("Assets/alex.png");
        var image3 = new GifImage { Source = new Uri("avares://AvaloniaApp_Play/Assets/200w.gif") };
        const int time = 1000;  
        
        
        while(true){
            ImageContent = image1;
            await Task.Delay(time);
            ImageContent = image2;
            await Task.Delay(time);
            ImageContent = image3;
            await Task.Delay(time);
        } 
    }
}
 
