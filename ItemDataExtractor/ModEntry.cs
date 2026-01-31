// ModEntry.cs - Item Data Extractor for Custom Starter Package Builder
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;

namespace ItemDataExtractor;

/// <summary>
/// Data structure for exported items.
/// </summary>
public class ExportedItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Category { get; set; } = "";
    public string QualifiedItemId { get; set; } = "";
    public string NameOrIndex { get; set; } = "";
    public int MaxStack { get; set; } = 1;
    public string Description { get; set; } = "";
}

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        Monitor.Log("Item Data Extractor loaded. Press F8 to export item data.", LogLevel.Info);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.F8)
        {
            ExportItemData();
        }
    }

    private void ExportItemData()
    {
        Monitor.Log("Extracting item data...", LogLevel.Info);
        
        var items = new List<ExportedItem>();
        
        try
        {
            // Extract Objects (O)
            ExtractObjects(items);
            
            // Extract Big Craftables (BC)
            ExtractBigCraftables(items);
            
            // Extract Tools (T)
            ExtractTools(items);
            
            // Extract Weapons (W)
            ExtractWeapons(items);
            
            // Extract Boots (B)
            ExtractBoots(items);
            
            // Extract Hats (H)
            ExtractHats(items);
            
            // Extract Rings (from Objects, but separate category)
            // Rings are already in Objects but we can identify them
            
            // Extract Trinkets (TR) - if available in game version
            ExtractTrinkets(items);
            
            // Sort by category then name
            items = items.OrderBy(i => i.Category).ThenBy(i => i.Name).ToList();
            
            // Save to file
            var outputPath = Path.Combine(Helper.DirectoryPath, "extracted-items.json");
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(outputPath, json);
            
            Monitor.Log($"Exported {items.Count} items to: {outputPath}", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage($"Exported {items.Count} items to mod folder!", HUDMessage.achievement_type));
        }
        catch (Exception ex)
        {
            Monitor.Log($"Error exporting item data: {ex}", LogLevel.Error);
            Game1.addHUDMessage(new HUDMessage("Error exporting items. Check SMAPI log.", HUDMessage.error_type));
        }
    }

    private void ExtractObjects(List<ExportedItem> items)
    {
        var objectData = Game1.objectData;
        if (objectData == null) return;
        
        foreach (var kvp in objectData)
        {
            var id = kvp.Key;
            var data = kvp.Value;
            
            // Determine category based on object type/category
            string category = GetObjectCategory(data);
            
            // Create a temporary item to get accurate stack info
            int maxStack = 999;
            try
            {
                var item = ItemRegistry.Create($"(O){id}");
                if (item != null)
                {
                    maxStack = item.maximumStackSize();
                }
            }
            catch { }
            
            items.Add(new ExportedItem
            {
                Id = id,
                Name = data.Name ?? id,
                Type = "Object",
                Category = category,
                QualifiedItemId = $"(O){id}",
                NameOrIndex = id,
                MaxStack = maxStack,
                Description = data.Description ?? ""
            });
        }
        
        Monitor.Log($"Extracted {objectData.Count} objects", LogLevel.Debug);
    }

    private string GetObjectCategory(ObjectData data)
    {
        // Use the game's category system
        var categoryNum = data.Category;
        
        return categoryNum switch
        {
            -2 => "Gems",
            -4 => "Fish",
            -5 => "Animal Products",
            -6 => "Animal Products",
            -7 => "Cooking",
            -8 => "Crafting",
            -9 => "Big Craftables",
            -12 => "Minerals",
            -14 => "Meat",
            -15 => "Metal Resources",
            -16 => "Building Resources",
            -17 => "Sell at Pierre's",
            -18 => "Animal Products",
            -19 => "Fertilizer",
            -20 => "Junk",
            -21 => "Bait",
            -22 => "Tackle",
            -23 => "Sell at Fish Shop",
            -24 => "Decor",
            -25 => "Cooking",
            -26 => "Artisan Goods",
            -27 => "Artisan Goods",
            -28 => "Monster Loot",
            -74 => "Seeds",
            -75 => "Vegetables",
            -79 => "Fruit",
            -80 => "Flowers",
            -81 => "Forage",
            -95 => "Rings",
            -96 => "Rings",
            -97 => "Boots",
            -98 => "Weapons",
            -99 => "Tools",
            0 => "Miscellaneous",
            _ => data.Type?.Contains("Ring") == true ? "Rings" : "Objects"
        };
    }

    private void ExtractBigCraftables(List<ExportedItem> items)
    {
        var bcData = Game1.bigCraftableData;
        if (bcData == null) return;
        
        foreach (var kvp in bcData)
        {
            var id = kvp.Key;
            var data = kvp.Value;
            
            int maxStack = 1;
            try
            {
                var item = ItemRegistry.Create($"(BC){id}");
                if (item != null)
                {
                    maxStack = item.maximumStackSize();
                }
            }
            catch { }
            
            items.Add(new ExportedItem
            {
                Id = id,
                Name = data.Name ?? id,
                Type = "BigCraftable",
                Category = "Craftables",
                QualifiedItemId = $"(BC){id}",
                NameOrIndex = id,
                MaxStack = maxStack,
                Description = data.Description ?? ""
            });
        }
        
        Monitor.Log($"Extracted {bcData.Count} big craftables", LogLevel.Debug);
    }

    private void ExtractTools(List<ExportedItem> items)
    {
        var toolData = Game1.toolData;
        if (toolData == null) return;
        
        foreach (var kvp in toolData)
        {
            var id = kvp.Key;
            var data = kvp.Value;
            
            // Get the actual display name by creating the item
            string name = id;
            string description = data.Description ?? "";
            try
            {
                var item = ItemRegistry.Create($"(T){id}");
                if (item != null)
                {
                    name = item.DisplayName;
                    description = item.getDescription();
                }
            }
            catch { }
            
            items.Add(new ExportedItem
            {
                Id = id,
                Name = name,
                Type = "Tool",
                Category = "Tools",
                QualifiedItemId = $"(T){id}",
                NameOrIndex = id,
                MaxStack = 1,
                Description = description
            });
        }
        
        Monitor.Log($"Extracted {toolData.Count} tools", LogLevel.Debug);
    }

    private void ExtractWeapons(List<ExportedItem> items)
    {
        var weaponData = Game1.weaponData;
        if (weaponData == null) return;
        
        foreach (var kvp in weaponData)
        {
            var id = kvp.Key;
            var data = kvp.Value;
            
            items.Add(new ExportedItem
            {
                Id = id,
                Name = data.Name ?? id,
                Type = "Weapon",
                Category = "Weapons",
                QualifiedItemId = $"(W){id}",
                NameOrIndex = id,
                MaxStack = 1,
                Description = data.Description ?? ""
            });
        }
        
        Monitor.Log($"Extracted {weaponData.Count} weapons", LogLevel.Debug);
    }

    private void ExtractBoots(List<ExportedItem> items)
    {
        // Boots are stored in Data/Boots
        try
        {
            var bootsData = Helper.GameContent.Load<Dictionary<string, string>>("Data/Boots");
            if (bootsData == null) return;
            
            foreach (var kvp in bootsData)
            {
                var id = kvp.Key;
                var fields = kvp.Value.Split('/');
                var name = fields.Length > 0 ? fields[0] : id;
                var description = fields.Length > 1 ? fields[1] : "";
                
                items.Add(new ExportedItem
                {
                    Id = id,
                    Name = name,
                    Type = "Boots",
                    Category = "Boots",
                    QualifiedItemId = $"(B){id}",
                    NameOrIndex = id,
                    MaxStack = 1,
                    Description = description
                });
            }
            
            Monitor.Log($"Extracted {bootsData.Count} boots", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Could not extract boots: {ex.Message}", LogLevel.Warn);
        }
    }

    private void ExtractHats(List<ExportedItem> items)
    {
        // Hats are stored in Data/hats
        try
        {
            var hatsData = Helper.GameContent.Load<Dictionary<string, string>>("Data/hats");
            if (hatsData == null) return;
            
            foreach (var kvp in hatsData)
            {
                var id = kvp.Key;
                var fields = kvp.Value.Split('/');
                var name = fields.Length > 0 ? fields[0] : id;
                var description = fields.Length > 1 ? fields[1] : "";
                
                items.Add(new ExportedItem
                {
                    Id = id,
                    Name = name,
                    Type = "Hat",
                    Category = "Hats",
                    QualifiedItemId = $"(H){id}",
                    NameOrIndex = id,
                    MaxStack = 1,
                    Description = description
                });
            }
            
            Monitor.Log($"Extracted {hatsData.Count} hats", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Could not extract hats: {ex.Message}", LogLevel.Warn);
        }
    }

    private void ExtractTrinkets(List<ExportedItem> items)
    {
        // Trinkets were added in 1.6
        try
        {
            var trinketsData = Helper.GameContent.Load<Dictionary<string, object>>("Data/Trinkets");
            if (trinketsData == null) return;
            
            foreach (var kvp in trinketsData)
            {
                var id = kvp.Key;
                
                // Try to create the item to get its display name
                string name = id;
                string description = "";
                try
                {
                    var item = ItemRegistry.Create($"(TR){id}");
                    if (item != null)
                    {
                        name = item.DisplayName;
                        description = item.getDescription();
                    }
                }
                catch { }
                
                items.Add(new ExportedItem
                {
                    Id = id,
                    Name = name,
                    Type = "Trinket",
                    Category = "Trinkets",
                    QualifiedItemId = $"(TR){id}",
                    NameOrIndex = id,
                    MaxStack = 1,
                    Description = description
                });
            }
            
            Monitor.Log($"Extracted {trinketsData.Count} trinkets", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Could not extract trinkets (may not be available in this game version): {ex.Message}", LogLevel.Debug);
        }
    }
}
