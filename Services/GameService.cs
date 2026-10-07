using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AvaloniaProject.Services;

public class GameService
{
    public int? GetSteamPlaytime()
    {
        var steamUserdataDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "Steam",
            "userdata",
            "111" // todo: temporary changed user id
        );
        
        var filePath = System.IO.Path.Combine(steamUserdataDir, "config", "localconfig.vdf");

        if (!File.Exists(filePath)) return null;

        var content = File.ReadAllText(filePath);

        var appId = "42700";

        var pattern = $@"""{appId}""\s*\{{[^}}]*""Playtime""\s*""(\d+)""";
        
        var match = Regex.Match(content, pattern);

        if (!match.Success) return null;
        
        var playtime = int.Parse(match.Groups[1].Value);
        return playtime;
    }

    public bool isSteamAppIdRunning()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "pgrep",
                Arguments = "-f \"AppId=42700\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            
            using var process = Process.Start(psi);
            Console.WriteLine($"PROCESS START: {process}");
            if (process == null) return false;
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            Console.WriteLine($"OUTPUT PROCESS: {output}");
            
            return !string.IsNullOrWhiteSpace(output);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}