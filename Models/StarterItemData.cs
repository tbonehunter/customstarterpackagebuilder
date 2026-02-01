// Models/StarterItemData.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomStarterPackageBuilder.Models;

/// <summary>
/// Content Patcher manifest.json format.
/// </summary>
public class ContentPackManifest
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("Author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("Version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("UniqueID")]
    public string UniqueID { get; set; } = "";

    [JsonPropertyName("ContentPackFor")]
    public ContentPackForEntry? ContentPackFor { get; set; }

    [JsonPropertyName("Dependencies")]
    public List<DependencyEntry>? Dependencies { get; set; }
}

public class ContentPackForEntry
{
    [JsonPropertyName("UniqueID")]
    public string UniqueID { get; set; } = "";
}

public class DependencyEntry
{
    [JsonPropertyName("UniqueID")]
    public string UniqueID { get; set; } = "";

    [JsonPropertyName("IsRequired")]
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Content Patcher content.json format.
/// </summary>
public class ContentPatcherContent
{
    [JsonPropertyName("Format")]
    public string Format { get; set; } = "2.0.0";

    [JsonPropertyName("Changes")]
    public List<ContentPatcherChange> Changes { get; set; } = new();
}

public class ContentPatcherChange
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "EditData";

    [JsonPropertyName("Target")]
    public string Target { get; set; } = "";

    [JsonPropertyName("Entries")]
    public Dictionary<string, CPStarterItemEntry> Entries { get; set; } = new();
}

/// <summary>
/// The format Custom Starter Package expects for each item in the CP dictionary.
/// </summary>
public class CPStarterItemEntry
{
    [JsonPropertyName("NameOrIndex")]
    public string NameOrIndex { get; set; } = "";

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "Object";

    [JsonPropertyName("MinAmount")]
    public int MinAmount { get; set; } = 1;

    [JsonPropertyName("MaxAmount")]
    public int MaxAmount { get; set; } = 1;

    [JsonPropertyName("ChancePercent")]
    public int ChancePercent { get; set; } = 100;

    [JsonPropertyName("MinQuality")]
    public int MinQuality { get; set; } = 0;

    [JsonPropertyName("MaxQuality")]
    public int MaxQuality { get; set; } = 0;

    /// <summary>
    /// Create a CPStarterItemEntry from a SelectedItem.
    /// </summary>
    public static CPStarterItemEntry FromSelectedItem(SelectedItem selected)
    {
        return new CPStarterItemEntry
        {
            NameOrIndex = selected.Item.NameOrIndex,
            Type = MapItemType(selected.Item.Type),
            MinAmount = selected.Quantity,
            MaxAmount = selected.Quantity,
            ChancePercent = 100,
            MinQuality = 0,
            MaxQuality = 0
        };
    }

    /// <summary>
    /// Map our item types to CSP's expected types.
    /// </summary>
    private static string MapItemType(string ourType)
    {
        return ourType switch
        {
            "Object" => "Object",
            "BigCraftable" => "BigCraftable",
            "Tool" => "Tool",
            "Weapon" => "MeleeWeapon",
            "Boots" => "Boots",
            "Hat" => "Hat",
            "Trinket" => "Object", // Try as Object for now
            _ => "Object"
        };
    }
}
