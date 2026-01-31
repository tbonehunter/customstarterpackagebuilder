# Custom Starter Package Builder

A standalone GUI application for configuring [Custom Starter Package](https://www.nexusmods.com/stardewvalley/mods/6829) by aedenthorn for Stardew Valley.

## Features

- **Browse Items by Category**: Seeds, Crops, Resources, Fish, Tools, Craftables, Weapons, Boots, Hats, Trinkets, and more
- **Search Functionality**: Quickly find items by name
- **Select Up to 5 Items**: Choose the items you want in your starter package
- **Quantity Selection**: Set quantities for stackable items (respects each item's MaxStack)
- **Auto-Detection**: Automatically finds your Custom Starter Package mod folder (even in nested organization folders)
- **Direct Export**: Saves directly to CSP's `config.json` format
- **Import Item List**: Import item lists from the included ItemDataExtractor mod

## Requirements

- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Custom Starter Package](https://www.nexusmods.com/stardewvalley/mods/6829) mod by aedenthorn
- [SMAPI](https://smapi.io/) mod loader

## How to Use

1. **Run the application** before launching Stardew Valley
2. **Browse or search** for items you want in your starter package
3. **Click "Add →"** or double-click to add items (up to 5)
4. **Set quantities** for stackable items using the quantity text boxes
5. **Review your selections** in the right panel
6. **Verify the output path** (auto-detected or browse to your CSP mod folder)
7. **Click "Save Configuration"** to save your settings
8. **Launch Stardew Valley** - your starter items will appear when starting a new game!

## Output Format

The tool outputs directly to Custom Starter Package's `config.json`:

```json
{
  "ModEnabled": true,
  "SelectedItems": [
    {
      "Type": "Object",
      "QualifiedItemId": "(O)74",
      "NameOrIndex": "74",
      "Quantity": 1,
      "DisplayName": "Prismatic Shard"
    }
  ]
}
```

## Building from Source

```bash
cd CustomStarterPackageBuilderGUI
dotnet build
dotnet run
```

## Updating the Item Database

### Using ItemDataExtractor (Recommended)

The included **ItemDataExtractor** SMAPI mod extracts all items from your game, including items from other mods.

1. Copy the `ItemDataExtractor` folder to your Stardew Valley `Mods` folder
2. Launch Stardew Valley and load a save
3. Press **F8** to extract all items
4. In the GUI, click **"📥 Import Item List..."** button
5. Select the `extracted-items.json` file from `Mods/ItemDataExtractor/`

The GUI automatically filters duplicate items (like mining node "Stone" variants).

### Manual Method

Edit `Data/items.json` directly. Each item needs:
- `Id`: The item's ID from game data
- `Name`: Display name
- `Type`: Item type (Object, BigCraftable, Tool, Weapon, Boots, Hat, Trinket)
- `Category`: UI category for filtering
- `MaxStack`: Maximum stack size
- `Description`: Optional description

## Mod Support

When you run **ItemDataExtractor** with other mods installed (Json Assets, Content Patcher packs, Stardew Valley Expanded, etc.), it will extract all items including mod-added items. Import the new list into the GUI to configure starter packages with modded items.

## Credits

- **Custom Starter Package** by aedenthorn
- Built with [Avalonia UI](https://avaloniaui.net/)

## License

MIT License - Feel free to modify and redistribute.
