# AvaloniaApp_Play

This project demonstrates migrating a simple WPF design to Avalonia using the MVVM pattern.

## Libraries

- **Avalonia.Labs.Gif** – provides `GifImage` to display GIF animations without relying on the WPF `WebBrowser` control.
- **LibVLCSharp.Avalonia** – enables video playback across platforms as a replacement for the WPF `MediaElement`.

All DTO classes remain unchanged to preserve the existing data contract.

## Prerequisites

Building the project requires the **.NET 8 SDK**. The `global.json` file pins
the expected SDK version so make sure `dotnet --version` returns 8.x.
