// Services/ItemDatabase.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using CustomStarterPackageBuilder.Models;

namespace CustomStarterPackageBuilder.Services;

/// <summary>
/// Service for loading and managing the game item database.
/// </summary>
public class ItemDatabase
{
    private List<GameItem> _allItems = new();
    private readonly Dictionary<string, List<GameItem>> _itemsByCategory = new();

    /// <summary>All available items.</summary>
    public IReadOnlyList<GameItem> AllItems => _allItems;

    /// <summary>All available categories.</summary>
    public IReadOnlyList<string> Categories { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Load items from the embedded JSON database.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            // Try to load from Data folder first
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            var filePath = Path.Combine(exePath, "Data", "items.json");
            
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                _allItems = JsonSerializer.Deserialize<List<GameItem>>(json) ?? new();
            }
            else
            {
                // Try embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "CustomStarterPackageBuilder.Data.items.json";

                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();
                    _allItems = JsonSerializer.Deserialize<List<GameItem>>(json) ?? new();
                }
                else
                {
                    // Load built-in defaults if nothing else works
                    LoadBuiltInDefaults();
                }
            }

            // Apply post-filter to remove duplicate mining node items
            // Keep only Stone with ID 390, exclude all others named exactly "Stone"
            ApplyPostFilter();

            OrganizeByCategory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading item database: {ex.Message}");
            LoadBuiltInDefaults();
            OrganizeByCategory();
        }
    }

    /// <summary>
    /// Apply post-filtering to remove unwanted duplicate items.
    /// Removes items named exactly "Stone" except for ID 390 (the actual stone resource).
    /// This filters out mining node variants that have the same display name.
    /// </summary>
    private void ApplyPostFilter()
    {
        _allItems = _allItems
            .Where(item => 
                // Keep the item if:
                // 1. Its name is NOT exactly "Stone", OR
                // 2. Its ID is "390" (the real Stone item)
                !string.Equals(item.Name, "Stone", StringComparison.Ordinal) || item.Id == "390")
            .ToList();
    }

    /// <summary>
    /// Get items filtered by category.
    /// </summary>
    public IEnumerable<GameItem> GetItemsByCategory(string category)
    {
        if (category == "All")
            return _allItems;

        return _itemsByCategory.TryGetValue(category, out var items) 
            ? items 
            : Enumerable.Empty<GameItem>();
    }

    /// <summary>
    /// Search items by name (case-insensitive).
    /// </summary>
    public IEnumerable<GameItem> SearchItems(string searchText, string? category = null)
    {
        var items = category == null || category == "All" 
            ? _allItems 
            : GetItemsByCategory(category);

        if (string.IsNullOrWhiteSpace(searchText))
            return items;

        return items.Where(i => i.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }

    private void OrganizeByCategory()
    {
        _itemsByCategory.Clear();

        foreach (var item in _allItems)
        {
            if (!_itemsByCategory.ContainsKey(item.Category))
                _itemsByCategory[item.Category] = new List<GameItem>();

            _itemsByCategory[item.Category].Add(item);
        }

        // Sort categories alphabetically, but put "All" first
        var sortedCategories = _itemsByCategory.Keys.OrderBy(c => c).ToList();
        sortedCategories.Insert(0, "All");
        Categories = sortedCategories;
    }

    /// <summary>
    /// Load built-in default items (subset of vanilla items for testing).
    /// This will be replaced by the full items.json file.
    /// </summary>
    private void LoadBuiltInDefaults()
    {
        _allItems = new List<GameItem>
        {
            // Seeds
            new() { Id = "472", Name = "Parsnip Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "473", Name = "Bean Starter", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "474", Name = "Cauliflower Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "475", Name = "Potato Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "476", Name = "Garlic Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "477", Name = "Kale Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "478", Name = "Rhubarb Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "479", Name = "Melon Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "480", Name = "Tomato Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "481", Name = "Blueberry Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "482", Name = "Pepper Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "483", Name = "Wheat Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "484", Name = "Radish Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "485", Name = "Red Cabbage Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "486", Name = "Starfruit Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "487", Name = "Corn Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "488", Name = "Eggplant Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "489", Name = "Artichoke Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "490", Name = "Pumpkin Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "491", Name = "Bok Choy Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "492", Name = "Yam Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "493", Name = "Cranberry Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "494", Name = "Beet Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "495", Name = "Spring Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "496", Name = "Summer Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "497", Name = "Fall Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "498", Name = "Winter Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "499", Name = "Ancient Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "745", Name = "Strawberry Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            new() { Id = "802", Name = "Cactus Seeds", Type = "Object", Category = "Seeds", MaxStack = 999 },
            
            // Crops / Produce
            new() { Id = "24", Name = "Parsnip", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "188", Name = "Green Bean", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "190", Name = "Cauliflower", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "192", Name = "Potato", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "248", Name = "Garlic", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "250", Name = "Kale", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "252", Name = "Rhubarb", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "254", Name = "Melon", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "256", Name = "Tomato", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "258", Name = "Blueberry", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "260", Name = "Hot Pepper", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "262", Name = "Wheat", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "264", Name = "Radish", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "266", Name = "Red Cabbage", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "268", Name = "Starfruit", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "270", Name = "Corn", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "272", Name = "Eggplant", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "274", Name = "Artichoke", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "276", Name = "Pumpkin", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "278", Name = "Bok Choy", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "280", Name = "Yam", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "282", Name = "Cranberries", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "284", Name = "Beet", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "400", Name = "Strawberry", Type = "Object", Category = "Crops", MaxStack = 999 },
            new() { Id = "454", Name = "Ancient Fruit", Type = "Object", Category = "Crops", MaxStack = 999 },
            
            // Resources
            new() { Id = "388", Name = "Wood", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "390", Name = "Stone", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "382", Name = "Coal", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "378", Name = "Copper Ore", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "380", Name = "Iron Ore", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "384", Name = "Gold Ore", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "386", Name = "Iridium Ore", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "334", Name = "Copper Bar", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "335", Name = "Iron Bar", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "336", Name = "Gold Bar", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "337", Name = "Iridium Bar", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "709", Name = "Hardwood", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "771", Name = "Fiber", Type = "Object", Category = "Resources", MaxStack = 999 },
            new() { Id = "92", Name = "Sap", Type = "Object", Category = "Resources", MaxStack = 999 },
            
            // Fish
            new() { Id = "128", Name = "Pufferfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "129", Name = "Anchovy", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "130", Name = "Tuna", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "131", Name = "Sardine", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "132", Name = "Bream", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "136", Name = "Largemouth Bass", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "137", Name = "Smallmouth Bass", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "138", Name = "Rainbow Trout", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "139", Name = "Salmon", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "140", Name = "Walleye", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "141", Name = "Perch", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "142", Name = "Carp", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "143", Name = "Catfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "144", Name = "Pike", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "145", Name = "Sunfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "146", Name = "Red Mullet", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "147", Name = "Herring", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "148", Name = "Eel", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "149", Name = "Octopus", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "150", Name = "Red Snapper", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "151", Name = "Squid", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "154", Name = "Sea Cucumber", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "155", Name = "Super Cucumber", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "156", Name = "Ghostfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "158", Name = "Stonefish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "159", Name = "Crimsonfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "160", Name = "Angler", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "161", Name = "Ice Pip", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "162", Name = "Lava Eel", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "163", Name = "Legend", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "164", Name = "Sandfish", Type = "Object", Category = "Fish", MaxStack = 999 },
            new() { Id = "165", Name = "Scorpion Carp", Type = "Object", Category = "Fish", MaxStack = 999 },
            
            // Tools
            new() { Id = "Axe", Name = "Axe", Type = "Tool", Category = "Tools", MaxStack = 1 },
            new() { Id = "Hoe", Name = "Hoe", Type = "Tool", Category = "Tools", MaxStack = 1 },
            new() { Id = "Pickaxe", Name = "Pickaxe", Type = "Tool", Category = "Tools", MaxStack = 1 },
            new() { Id = "WateringCan", Name = "Watering Can", Type = "Tool", Category = "Tools", MaxStack = 1 },
            new() { Id = "FishingRod", Name = "Bamboo Pole", Type = "FishingRod", Category = "Tools", MaxStack = 1 },
            new() { Id = "Scythe", Name = "Scythe", Type = "Tool", Category = "Tools", MaxStack = 1 },
            
            // Big Craftables
            new() { Id = "9", Name = "Chest", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "130", Name = "Big Chest", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "12", Name = "Keg", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "13", Name = "Furnace", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "15", Name = "Preserves Jar", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "16", Name = "Cheese Press", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "17", Name = "Loom", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "19", Name = "Oil Maker", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "20", Name = "Recycling Machine", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "21", Name = "Crystalarium", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "24", Name = "Mayonnaise Machine", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "25", Name = "Seed Maker", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "10", Name = "Scarecrow", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            new() { Id = "8", Name = "Bee House", Type = "BigCraftable", Category = "Craftables", MaxStack = 1 },
            
            // Food/Cooking
            new() { Id = "194", Name = "Fried Egg", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "195", Name = "Omelet", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "196", Name = "Salad", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "197", Name = "Cheese Cauliflower", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "198", Name = "Baked Fish", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "199", Name = "Parsnip Soup", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "200", Name = "Vegetable Medley", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "201", Name = "Complete Breakfast", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "202", Name = "Fried Calamari", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "203", Name = "Strange Bun", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "204", Name = "Lucky Lunch", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "205", Name = "Fried Mushroom", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "206", Name = "Pizza", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "207", Name = "Bean Hotpot", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "208", Name = "Glazed Yams", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "211", Name = "Pancakes", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "212", Name = "Salmon Dinner", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "213", Name = "Fish Taco", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "214", Name = "Crispy Bass", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "215", Name = "Pepper Poppers", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "216", Name = "Bread", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "218", Name = "Tom Kha Soup", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "219", Name = "Trout Soup", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "220", Name = "Chocolate Cake", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "221", Name = "Pink Cake", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "222", Name = "Rhubarb Pie", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "223", Name = "Cookie", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "224", Name = "Spaghetti", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "225", Name = "Fried Eel", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "226", Name = "Spicy Eel", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "227", Name = "Sashimi", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "228", Name = "Maki Roll", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "229", Name = "Tortilla", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "230", Name = "Red Plate", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "231", Name = "Eggplant Parmesan", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "232", Name = "Rice Pudding", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "233", Name = "Ice Cream", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "234", Name = "Blueberry Tart", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "235", Name = "Autumn's Bounty", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "236", Name = "Pumpkin Soup", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "237", Name = "Super Meal", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "238", Name = "Cranberry Sauce", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "239", Name = "Stuffing", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "240", Name = "Farmer's Lunch", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "241", Name = "Survival Burger", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "242", Name = "Dish O' The Sea", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "243", Name = "Miner's Treat", Type = "Object", Category = "Food", MaxStack = 999 },
            new() { Id = "244", Name = "Roots Platter", Type = "Object", Category = "Food", MaxStack = 999 },
            
            // Misc
            new() { Id = "787", Name = "Battery Pack", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
            new() { Id = "432", Name = "Truffle Oil", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
            new() { Id = "430", Name = "Truffle", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
            new() { Id = "428", Name = "Cloth", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
            new() { Id = "395", Name = "Coffee", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
            new() { Id = "253", Name = "Triple Shot Espresso", Type = "Object", Category = "Miscellaneous", MaxStack = 999 },
        };
    }
}
