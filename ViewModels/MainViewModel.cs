using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaProject.Data;
using AvaloniaProject.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaProject.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private Process? _gameProcess;
    
    [ObservableProperty]
    private Game? _currentlyPlaying;
    
    public ObservableCollection<Game> Games { get; } = new();
    
    public async Task LoadGamesAsync()
    {
        await using var db = new ApplicationDbContext();

        var games = await db.Games
            .OrderBy(g => g.Name)
            .ToListAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Games.Clear();
            foreach (var game in games)
            {
                Games.Add(game);
            }
        });
    }
    
    public async Task UpdateGamePlayTime(long id, TimeSpan elapsedTime)
    {
        await using var db = new ApplicationDbContext();
        
        var game = await db.Games.FindAsync(id);
        
        if (game == null) return;

        game.TotalPlayTime += (long)elapsedTime.TotalSeconds;
        
        await db.SaveChangesAsync();
    }
    
    public async Task AddGameAsync(string name, string path)
    {
        await using var db = new ApplicationDbContext();

        var game = new Game
        {
            Name = name,
            Path = path,
            TotalPlayTime = 0
        };

        db.Games.Add(game);
        await db.SaveChangesAsync();
    }
}
