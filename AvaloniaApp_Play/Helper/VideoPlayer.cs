using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Controls;
using UDA.Shared;

namespace UDA.InstructionScreen.Helper;

public class VideoPlayer : IDisposable
{
    public event Action<string>? ErrorOccurred;
    private readonly EventHandler<Tuple<LogType, string>>? _logger;

    private enum PlayerState { Stopped, Playing, Paused }
    private PlayerState _currentState;
    private readonly MediaPlayer _mediaElement;
    private readonly Queue<string> _playList = new();

    public VideoPlayer(MediaPlayer mediaElement, EventHandler<Tuple<LogType, string>>? logger)
    {
        _mediaElement = mediaElement;
        _mediaElement.MediaEnded += OnVideoEnded;
        _mediaElement.MediaOpened += OnVideoLoaded;
        _logger = logger;

        _currentState = PlayerState.Stopped;
        Logger_Method(LogType.Warning, "VideoPlayer initialized.");
    }

    public void Resume(string? directoryPathOfVideos)
    {
        Logger_Method(LogType.Warning, "Video playback resume was triggered.");
        ReloadPlaylist(directoryPathOfVideos);
        PlayVideo();
    }

    public void Pause()
    {  
        _mediaElement.Pause();
        _currentState = PlayerState.Paused;
        Logger_Method(LogType.Warning, "Video playback paused.");
    }

    public void PauseAndReleaseMemory()
    {  
        _mediaElement.Stop();
        _mediaElement.Source = null;
        _currentState = PlayerState.Stopped;
        Logger_Method(LogType.Warning, "Stopped video playback and released memory.");
    }

    public void Stop()
    {
        _mediaElement.Stop();
        _mediaElement.Source = null;
        _currentState = PlayerState.Stopped;
        Logger_Method(LogType.Warning, "Video playback stopped.");
    }

    public void Dispose()
    {
        _mediaElement.MediaEnded -= OnVideoEnded;
        _mediaElement.MediaOpened -= OnVideoLoaded;
        _mediaElement.Stop();
        _mediaElement.Source = null;
        Logger_Method(LogType.Warning, "VideoPlayer disposed.");
    }

    private void ReloadPlaylist(string? videosDirectoryPath)
    {
        if (videosDirectoryPath is null)
        {
            FireNewError("In VideoPlayer, the videosDirectoryPath is null");
            Logger_Method(LogType.Error, "videosDirectoryPath is null.");
            return;
        }

        var playListOrderPath = Path.Combine(videosDirectoryPath, "PlayListOrder.json");
        var newPlayListOrder = !File.Exists(playListOrderPath) ? null : JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(playListOrderPath));

        if (newPlayListOrder?.Count > 0)
        {
            Logger_Method(LogType.Error, "PlayListOrder.json was found, loading list..");
            try
            {
                foreach (var videoFileName in newPlayListOrder)
                {
                    var videoPath = Path.Combine(videosDirectoryPath, videoFileName);
                    if (File.Exists(videoPath) && IsValidVideoFileFormatExists(videoPath)) continue;
                    FireNewError($"The video '{videoPath}' doesn't exist or is in an unsupported format.");
                    Logger_Method(LogType.Error, $"Invalid video file: {videoPath}");
                }

                _playList.Clear();
                _mediaElement.Position = TimeSpan.Zero;

                foreach (var file in newPlayListOrder)
                    _playList.Enqueue(Path.Combine(videosDirectoryPath, file));

                Logger_Method(LogType.Warning, "Playlist loaded successfully.");
            }
            catch (Exception ex)
            {
                FireNewError($"Error reading PlayListOrder.json: {ex.Message}");
                Logger_Method(LogType.Error, $"Error reading PlayListOrder.json: {ex.Message}");
            }
        }
        else
        {
            Logger_Method(LogType.Error, "PlayListOrder.json was not found, loading from path directly");
            var allVideoFiles = Directory.EnumerateFiles(videosDirectoryPath).Where(IsValidVideoFileFormatExists).ToList();

            if (allVideoFiles.Count != 0)
            {
                Logger_Method(LogType.Warning, $"Found {allVideoFiles.Count} videos.");
                _playList.Clear();
                _mediaElement.Position = TimeSpan.Zero;
                foreach (var file in allVideoFiles)
                {
                    Logger_Method(LogType.Warning, $"Enqueuing video {file}");
                    _playList.Enqueue(file);
                }
            }
            else
            {
                FireNewError("No supported video files found in the directory.");
                Logger_Method(LogType.Error, "No supported video files found.");
            }
        }
    }

    private void PlayVideo()
    {
        if (_playList.Count > 0)
        {
            var nextVideo = _playList.Dequeue();
            _mediaElement.Source = new Uri(nextVideo);
            _mediaElement.Play();
            _currentState = PlayerState.Playing;
            _playList.Enqueue(nextVideo);
            Logger_Method(LogType.Warning, $"Playing video: {nextVideo}");
        }
        else
        {
            FireNewError("No videos to play.");
            Logger_Method(LogType.Error, "Attempted to play video, but playlist is empty.");
        }
    }

    private void PlayNextVideo()
    {
        Logger_Method(LogType.Warning, "Playing next video..");
        PlayVideo(); // Using the PlayVideo method to play the next video
    }

    private void OnVideoEnded(object sender, EventArgs e)
    {
        PlayNextVideo();
    }

    private void OnVideoLoaded(object sender, RoutedEventArgs e)
    {
        if (_currentState == PlayerState.Stopped)
        {
            _mediaElement.Position = TimeSpan.Zero;
        }
    }

    private static bool IsValidVideoFileFormatExists(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension is ".mp4" or ".avi" or ".wmv" or ".mpeg" or ".asf" or ".3gp";
    }

    private void Logger_Method(LogType type, string message)
    {
        _logger?.Invoke(this, Tuple.Create(type, message));
    }

    private void FireNewError(string errorMessage)
    {
        ErrorOccurred?.Invoke(errorMessage);
    }
}