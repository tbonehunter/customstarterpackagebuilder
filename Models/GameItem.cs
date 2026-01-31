// Models/GameItem.cs
namespace CustomStarterPackageBuilder.Models;

/// <summary>
/// Represents a game item that can be selected for the starter package.
/// </summary>
public class GameItem
{
    /// <summary>The item's unique ID (e.g., "128" for Pufferfish, "ReturnScepter" for tools).</summary>
    public string Id { get; set; } = "";

    /// <summary>The item's display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The item type for Custom Starter Package (Object, BigCraftable, Hat, Tool, etc.).</summary>
    public string Type { get; set; } = "Object";

    /// <summary>The qualified item ID (e.g., "(O)128", "(T)ReturnScepter", "(BC)165").</summary>
    public string QualifiedItemId { get; set; } = "";

    /// <summary>The name or index for Custom Starter Package.</summary>
    public string NameOrIndex { get; set; } = "";

    /// <summary>The category this item belongs to for UI filtering.</summary>
    public string Category { get; set; } = "Miscellaneous";

    /// <summary>Maximum stack size (1 for BigCraftables/tools, up to 999 for most objects).</summary>
    public int MaxStack { get; set; } = 999;

    /// <summary>Description of the item.</summary>
    public string Description { get; set; } = "";
}
