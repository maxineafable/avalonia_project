using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvaloniaProject.Models;

public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long TotalPlayTime { get; set; }
    
    [NotMapped]
    public string FormattedPlayTime
    {
        get
        {
            var time = TimeSpan.FromSeconds(TotalPlayTime);

            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";

            return $"{time.Minutes}m {time.Seconds}s";
        }
    }
}