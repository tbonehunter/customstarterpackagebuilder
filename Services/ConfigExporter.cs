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
/// Service for exporting starter package configuration as a Content Patcher pack.
/// </summary>
public class ConfigExporter
{
    private const string ContentPackName = "[CP] Custom Starter Package Config";
    private const string UniqueId = "tbonehunter.CSPConfig";
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Export selected items as a Content Patcher pack in the Mods folder.
    /// </summary>
    /// <param name="selectedItems">The items selected for the starter package.</param>
    /// <param name="modsFolder">The path to the Stardew Valley Mods folder.</param>
    public async Task ExportAsync(IEnumerable<SelectedItem> selectedItems, string modsFolder)
    {
        // Create the content pack folder
        var packFolder = Path.Combine(modsFolder, ContentPackName);
        Directory.CreateDirectory(packFolder);

        // Create manifest.json
        var manifest = new ContentPackManifest
        {
            Name = "Custom Starter Package Config",
            Author = "tbonehunter",
            Version = "1.0.0",
            Description = "Custom starter items configured with Custom Starter Package Builder GUI.",
            UniqueID = UniqueId,
            ContentPackFor = new ContentPackForEntry { UniqueID = "Pathoschild.ContentPatcher" },
            Dependencies = new List<DependencyEntry>
            {
                new DependencyEntry { UniqueID = "aedenthorn.CustomStarterPackage", IsRequired = true }
            }
        };

        var manifestJson = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(packFolder, "manifest.json"), manifestJson);

        // Create content.json with entries
        var entries = new Dictionary<string, CPStarterItemEntry>();
        foreach (var selected in selectedItems)
        {
            var entryKey = $"{UniqueId}/{SanitizeEntryKey(selected.Item.Name)}";
            entries[entryKey] = CPStarterItemEntry.FromSelectedItem(selected);
        }

        var content = new ContentPatcherContent
        {
            Format = "2.0.0",
            Changes = new List<ContentPatcherChange>
            {
                new ContentPatcherChange
                {
                    Action = "EditData",
                    Target = "aedenthorn.CustomStarterPackage/dictionary",
                    Entries = entries
                }
            }
        };

        var contentJson = JsonSerializer.Serialize(content, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(packFolder, "content.json"), contentJson);
    }

    /// <summary>
    /// Sanitize item name for use as an entry key (remove special characters).
    /// </summary>
    private static string SanitizeEntryKey(string name)
    {
        // Replace spaces and special characters with underscores, keep alphanumeric
        var sanitized = new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        // Remove consecutive underscores
        while (sanitized.Contains("__"))
            sanitized = sanitized.Replace("__", "_");
        return sanitized.Trim('_');
    }

    /// <summary>
    /// Get the path to the Content Pack folder.
    /// </summary>
    public static string GetContentPackPath(string modsFolder)
    {
        return Path.Combine(modsFolder, ContentPackName);
    }

    /// <summary>
    /// Check if a Content Pack already exists.
    /// </summary>
    public static bool ContentPackExists(string modsFolder)
    {
        var packFolder = Path.Combine(modsFolder, ContentPackName);
        return Directory.Exists(packFolder) && File.Exists(Path.Combine(packFolder, "content.json"));
    }

    /// <summary>
    /// Try to find Custom Starter Package mod folder (to verify the mod is installed).
    /// </summary>
    public static string? FindCSPModFolder(string modsFolder)
    {
        var possibleNames = new[]
        {
            "CustomStarterPackage",
            "Custom Starter Package",
            "[SMAPI] CustomStarterPackage",
            "aedenthorn.CustomStarterPackage"
        };

        try
        {
            foreach (var dir in Directory.GetDirectories(modsFolder, "*", SearchOption.AllDirectories))
            {
                var dirName = Path.GetFileName(dir);
                
                if (possibleNames.Any(name => string.Equals(dirName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    return dir;
                }
                
                var manifestPath = Path.Combine(dir, "manifest.json");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifestText = File.ReadAllText(manifestPath);
                        if (manifestText.Contains("aedenthorn.CustomStarterPackage", StringComparison.OrdinalIgnoreCase))
                        {
                            return dir;
                        }
                    }
                    catch
                    {
                        // Ignore read errors
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
