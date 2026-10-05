using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

using AvaloniaProject.Models;
using AvaloniaProject.ViewModels;

namespace AvaloniaProject.Views;

public partial class MainWindow : Window
{
    
    public MainWindow()
    {
        InitializeComponent();
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
        if (string.IsNullOrWhiteSpace(Name.Text) || string.IsNullOrWhiteSpace(Path.Text))
        {
            TextStatus.Text = "Please fill in a name and path";
            TextStatus.Foreground = Brushes.Red;
            return;
        }
        
        if (DataContext is not MainViewModel viewModel) return;

        SaveButton.IsEnabled = false;
        SaveButton.Content = "Saving...";
        
        try
        {
            await viewModel.AddGameAsync(Name.Text, Path.Text);
            await viewModel.LoadGamesAsync();
            
            TextStatus.Text = "Game saved";
            TextStatus.Foreground = Brushes.Green;

            Name.Text = "";
            Path.Text = "";
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
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadGamesAsync();
        }
    }
}