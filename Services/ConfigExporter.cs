// Services/ConfigExporter.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using CustomStarterPackageBuilder.Models;

namespace CustomStarterPackageBuilder.Services;

/// <summary>
/// Service for exporting starter package configuration to Custom Starter Package's config.json format.
/// </summary>
public class ConfigExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Export selected items directly to Custom Starter Package's config.json file.
    /// </summary>
    /// <param name="selectedItems">The items selected for the starter package.</param>
    /// <param name="configFilePath">The path to the config.json file.</param>
    public async Task ExportAsync(IEnumerable<SelectedItem> selectedItems, string configFilePath)
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(configFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create the config object
        var config = new CSPConfig
        {
            ModEnabled = true,
            SelectedItems = selectedItems.Select(SelectedItemData.FromSelectedItem).ToList()
        };

        // Write the config file
        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(configFilePath, json);
    }

    /// <summary>
    /// Try to find Custom Starter Package's config.json file.
    /// Searches recursively through nested folders since SMAPI supports nested mod organization.
    /// </summary>
    public static string? FindCSPConfigPath()
    {
        var modsFolder = FindModsFolder();
        if (modsFolder == null)
            return null;

        // Known folder names for Custom Starter Package
        var possibleNames = new[]
        {
            "CustomStarterPackage",
            "Custom Starter Package",
            "[SMAPI] CustomStarterPackage",
            "aedenthorn.CustomStarterPackage"
        };

        // Search recursively through all subdirectories (SMAPI supports nested mod folders)
        try
        {
            foreach (var dir in Directory.GetDirectories(modsFolder, "*", SearchOption.AllDirectories))
            {
                var dirName = Path.GetFileName(dir);
                
                // Check if folder name matches known names
                if (possibleNames.Any(name => string.Equals(dirName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    return Path.Combine(dir, "config.json");
                }
                
                // Also check manifest.json for the UniqueID
                var manifestPath = Path.Combine(dir, "manifest.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifestText = File.ReadAllText(manifestPath);
                        if (manifestText.Contains("aedenthorn.CustomStarterPackage", StringComparison.OrdinalIgnoreCase))
                        {
                            return Path.Combine(dir, "config.json");
                        }
                    }
                    catch
                    {
                        // Ignore read errors on individual manifests
                    }
                }
            }
        }
        catch
        {
            // Ignore errors when searching
        }

        return null;
    }

    /// <summary>
    /// Try to find the Stardew Valley Mods folder.
    /// </summary>
    public static string? FindModsFolder()
    {
        // Common installation paths
        var possiblePaths = new[]
        {
            // Steam (Windows) - C: drive
            @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods",
            @"C:\Program Files\Steam\steamapps\common\Stardew Valley\Mods",
            
            // Steam (Windows) - D: drive
            @"D:\Steam\steamapps\common\Stardew Valley\Mods",
            @"D:\SteamLibrary\steamapps\common\Stardew Valley\Mods",
            @"D:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods",
            @"D:\Program Files\Steam\steamapps\common\Stardew Valley\Mods",
            @"D:\Games\Steam\steamapps\common\Stardew Valley\Mods",
            @"D:\Games\Stardew Valley\Mods",
            
            // Steam (Windows) - E: drive
            @"E:\Steam\steamapps\common\Stardew Valley\Mods",
            @"E:\SteamLibrary\steamapps\common\Stardew Valley\Mods",
            @"E:\Games\Stardew Valley\Mods",
            
            // GOG (Windows)
            @"C:\GOG Games\Stardew Valley\Mods",
            @"C:\Program Files (x86)\GOG Galaxy\Games\Stardew Valley\Mods",
            @"D:\GOG Games\Stardew Valley\Mods",
            @"D:\Games\GOG\Stardew Valley\Mods",
            
            // Xbox/Microsoft Store (Windows)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "Programs", "ModifiableWindowsApps", "Stardew Valley", "Mods"),
            
            // Steam (macOS)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "Steam", "steamapps", "common", "Stardew Valley", "Contents", "MacOS", "Mods"),
            
            // Steam (Linux)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".steam", "steam", "steamapps", "common", "Stardew Valley", "Mods"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local", "share", "Steam", "steamapps", "common", "Stardew Valley", "Mods"),
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
                return path;
        }

        return null;
    }
}
