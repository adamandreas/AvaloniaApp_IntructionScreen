using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Labs.Gif; 

namespace AvaloniaApp_Play.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public InstructionSectionViewModel HeaderSection { get; } = new();
    public InstructionSectionViewModel BodySection { get; } = new();
    public InstructionSectionViewModel FooterSection { get; } = new();

    public MainWindowViewModel()
    {
        HeaderSection.ImageContent = new Bitmap("Assets/alex.png");
        HeaderSection.OverlayText = "Header";

        BodySection.ImageContent = new Bitmap("Assets/alex.png");
        BodySection.OverlayText = "Scan your document";

        FooterSection.ImageContent = new Bitmap("Assets/alex.png");
        FooterSection.OverlayText = "Footer Info";
    }

    // Example1: the Hello world of this project
    // private async void StartAlternating()
    // {
    //     var image1 = new Bitmap("Assets/avalonia-logo.ico");
    //     var image2 = new Bitmap("Assets/alex.png");
    //     var image3 = new GifImage { Source = new Uri("avares://UDA.InstructionScreen/Assets/200w.gif") };
    //     const int time = 1000;  
    //     
    //     // change height of image1
    //     
    //     while(true){
    //         ImageContent = image1;
    //         await Task.Delay(time);
    //         
    //         // change height of image1
    //         ImageContent = image1;
    //         await Task.Delay(time);
    //         
    //         // change height of image1
    //         ImageContent = image1;
    //         await Task.Delay(time);
    //     } 
    // }
}
 
