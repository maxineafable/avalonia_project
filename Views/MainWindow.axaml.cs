using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

using AvaloniaProject.Models;
using AvaloniaProject.Services;
using AvaloniaProject.ViewModels;

namespace AvaloniaProject.Views;

public partial class MainWindow : Window
{
    private readonly GameService _gameService;
    
    public MainWindow()
    {
        InitializeComponent();
        _gameService = new GameService();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadGamesAsync();
        }
    }
    
    private void PlayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Game game) return;
        
        if (DataContext is not MainViewModel viewModel) return;

        if (viewModel.CurrentlyPlaying != null && viewModel.CurrentlyPlaying.Id != game.Id)
        {
            TextStatus.Text = $"You're already playing {viewModel.CurrentlyPlaying?.Name}";
            TextStatus.Foreground = Brushes.Red;
            return;
        };
        
        if (viewModel.GameProcess is { HasExited: false } process)
        {
            process.Kill();
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = game.Path,
                UseShellExecute = false,
            };
        
            var startProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };
                
            var startTime = DateTime.UtcNow;
        
            startProcess.Exited += async (_, _) =>
            {
                var elapsedTime = DateTime.UtcNow - startTime;
        
                await viewModel.UpdateGamePlayTime(game.Id, elapsedTime);
                await viewModel.LoadGamesAsync();
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    viewModel.GameProcess = null;
                    viewModel.CurrentlyPlaying = null;
                    
                    button.Content = "Play";
                });
                
                startProcess.Dispose();
            };

            startProcess.Start();
            
            viewModel.GameProcess = startProcess;
            viewModel.CurrentlyPlaying = game;
            
            button.Content = "Stop";
        }
        catch (Exception exception)
        {
            viewModel.GameProcess = null;
            viewModel.CurrentlyPlaying = null;
            
            Console.WriteLine(exception);
        }
    }

    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Name.Text))
        {
            TextStatus.Text = "Please enter a name";
            TextStatus.Foreground = Brushes.Red;
            return;
        }

        switch (GameTypeComboBox.SelectedIndex)
        {
            case 0 when string.IsNullOrWhiteSpace(Path.Text):
                TextStatus.Text = "Please enter a path";
                TextStatus.Foreground = Brushes.Red;
                return;
            case 1 when
                (!int.TryParse(SteamAppId.Text, out int steamAppId) || steamAppId <= 0):
                TextStatus.Text = "Please enter a valid Steam App ID";
                TextStatus.Foreground = Brushes.Red;
                return;
        }

        Console.WriteLine(Name.Text);
        Console.WriteLine(Path.Text);
        Console.WriteLine(GameTypeComboBox.SelectedIndex);
        Console.WriteLine(SteamAppId.Text);
        
        if (DataContext is not MainViewModel viewModel) return;
        
        SaveButton.IsEnabled = false;
        SaveButton.Content = "Saving...";
        
        try
        {
            var gameType = (GameType)GameTypeComboBox.SelectedIndex;

            if (gameType == Models.GameType.Native)
            {
                await viewModel.AddGameAsync(Name.Text, Models.GameType.Native, Path.Text);
            }
            else
            {
                await viewModel.AddGameAsync(Name.Text, Models.GameType.Steam, SteamAppId.Text);
            }
            
            await viewModel.LoadGamesAsync();
            
            TextStatus.Text = "Game saved";
            TextStatus.Foreground = Brushes.Green;
        
            Name.Text = "";
            Path.Text = "";
            SteamAppId.Text = "";
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            TextStatus.Text = "Failed to save game.";
            TextStatus.Foreground = Brushes.Red;
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Content = "Save";
        }
    }

    private async void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // if (DataContext is MainViewModel viewModel)
        // {
        //     await viewModel.LoadGamesAsync();
        // }
        
        // todo: temporary changed this to test steam game opening
        var prePlaytime = _gameService.GetSteamPlaytime();
        TextStatus.Text = prePlaytime.ToString();

        var startInfo = new ProcessStartInfo
        {
            FileName = "xdg-open",
            Arguments = $"steam://rungameid/42700",
            UseShellExecute = true,
        };
    
        var startProcess = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };
    
        startProcess.Start();
        
        var gameStartedSuccessfully = false;
        var maxAttempts = 30;

        for (var i = 0; i < maxAttempts; i++)
        {
            if (_gameService.isSteamAppIdRunning())
            {
                gameStartedSuccessfully = true;
                break;
            }
            await Task.Delay(1000);
        }
        
        if (gameStartedSuccessfully)
        {
            while (_gameService.isSteamAppIdRunning())
            {
                await Task.Delay(3000);
            }
        }
        else
        {
            TextPostPlaytime.Text = "Game failed to run";
            return;
        }
        
        await Task.Delay(4000);
        
        var postPlaytime = _gameService.GetSteamPlaytime();
        var session = postPlaytime - prePlaytime;
        if (session == 0)
        {
            TextPostPlaytime.Text = "less than min";
        }
        else
        {
            TextPostPlaytime.Text = $"Total session: {session.ToString()}";
        }
    }
}