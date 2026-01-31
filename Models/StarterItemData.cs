// Models/StarterItemData.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomStarterPackageBuilder.Models;

/// <summary>
/// The format Custom Starter Package expects for each item in config.json.
/// </summary>
public class SelectedItemData
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "Object";

    [JsonPropertyName("QualifiedItemId")]
    public string QualifiedItemId { get; set; } = "";

    [JsonPropertyName("NameOrIndex")]
    public string NameOrIndex { get; set; } = "";

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Create a SelectedItemData from a SelectedItem.
    /// </summary>
    public static SelectedItemData FromSelectedItem(SelectedItem selected)
    {
        return new SelectedItemData
        {
            Type = selected.Item.Type,
            QualifiedItemId = selected.Item.QualifiedItemId,
            NameOrIndex = selected.Item.NameOrIndex,
            Quantity = selected.Quantity,
            DisplayName = selected.Item.Name
        };
    }
}

/// <summary>
/// The Custom Starter Package config.json format.
/// </summary>
public class CSPConfig
{
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    [JsonPropertyName("SelectedItems")]
    public List<SelectedItemData> SelectedItems { get; set; } = new();
}
