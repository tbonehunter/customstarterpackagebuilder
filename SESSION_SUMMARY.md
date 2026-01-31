# Custom Starter Package Builder GUI - Session Summary

**Date:** January 31, 2026  
**Project Location:** `C:\Users\HP\Documents\CustomStarterPackageBuilderGUI`

---

## Project Overview

A standalone Avalonia GUI application that allows users to configure the **Custom Starter Package** mod (by aedenthorn) **before** launching Stardew Valley. The tool directly outputs to CSP's `config.json` format.

### Why Standalone (Not SMAPI Mod)?
- Custom Starter Package loads its configuration at game startup
- An in-game configuration tool would be too late - items are already given
- This GUI runs before the game, allowing proper pre-game configuration

---

## Technology Stack

- **Framework:** Avalonia 11.1.0 (cross-platform GUI)
- **Target:** .NET 8.0
- **Serialization:** System.Text.Json 8.0.5

---

## Project Structure

```
CustomStarterPackageBuilderGUI/
├── App.axaml / App.axaml.cs          # Avalonia application entry
├── Program.cs                         # Main entry point
├── CustomStarterPackageBuilder.csproj # Project file
├── README.md                          # User documentation
├── Data/
│   └── items.json                     # Item database (1,233 items from game)
├── Models/
│   ├── GameItem.cs                    # Item data model
│   ├── SelectedItem.cs                # Selected item with quantity
│   └── StarterItemData.cs             # CSP config.json format models
├── Services/
│   ├── ConfigExporter.cs              # Exports to CSP config.json + mod folder detection
│   └── ItemDatabase.cs                # Loads/searches items + post-filtering
├── ViewModels/
│   └── MainWindowViewModel.cs         # Main window logic + item import
├── Views/
│   └── MainWindow.axaml / .axaml.cs   # Main UI
└── Tools/
    └── ParseItemFiles.ps1             # Legacy script (superseded by ItemDataExtractor)

ItemDataExtractor/                     # Companion SMAPI mod
├── ItemDataExtractor.csproj
├── manifest.json
└── ModEntry.cs                        # Extracts all game items (F8 key)
```

---

## Key Features

1. **Category Browsing** - Objects, Craftables, Tools, Weapons, Boots, Hats, Trinkets
2. **Search** - Filter items by name (case-insensitive)
3. **5-Item Limit** - Select up to 5 discrete items (per CSP design)
4. **Quantity Input** - Text input for item quantities (respects MaxStack)
5. **Direct Export** - Outputs directly to CSP's config.json format
6. **Auto-Detection** - Recursively searches Mods folder for CSP (supports nested organization)
7. **Import Item List** - One-click import of extracted items from ItemDataExtractor
8. **Post-Filtering** - Automatically removes duplicate "Stone" mining nodes (keeps only ID 390)

---

## Output Format (CSP config.json)

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

### Qualified Item ID Formats:
- **Objects:** `(O)123`
- **Big Craftables:** `(BC)123`
- **Tools:** `(T)ToolName`
- **Weapons:** `(W)123`
- **Boots:** `(B)123`
- **Hats:** `(H)123`
- **Trinkets:** `(TR)TrinketName`

---

## ItemDataExtractor SMAPI Mod

A companion SMAPI mod that extracts all game items including mod-added items.

**Location:** `C:\Users\HP\Documents\ItemDataExtractor` (source)  
**Deployment:** `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\ItemDataExtractor`

### Features:
- Extracts Objects, BigCraftables, Tools, Weapons, Boots, Hats, Trinkets
- Reads from live game data (includes all mod-added items)
- Tools use actual DisplayName (not LocalizedTextString)
- Includes MaxStack and Description for each item
- Press **F8** to export `extracted-items.json`

### Item Categories Extracted:
| Category | Source |
|----------|--------|
| Objects | `Game1.objectData` |
| Craftables | `Game1.bigCraftablesData` |
| Tools | `Game1.toolData` |
| Weapons | `Game1.weaponData` |
| Boots | `Game1.bootsData` |
| Hats | `DataLoader.Hats()` |
| Trinkets | `DataLoader.Trinkets()` |

---

## Item Database

**Current Items:** 1,233 (after Stone filtering)

### Post-Filter Logic (ItemDatabase.cs):
- Removes items named exactly "Stone" (mining nodes)
- Keeps Stone with ID "390" (the actual resource item)
- Items like "Mystic Stone", "Frosty Stone" etc. are preserved

---

## Auto-Detection of CSP Mod Folder

The app **recursively** searches for Custom Starter Package in:

### Search Paths:
- `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\`
- `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\`
- `C:\Program Files\Steam\steamapps\common\Stardew Valley\Mods\`
- Various D:, E: drive locations
- GOG installation paths
- Xbox/Microsoft Store path

### Detection Method:
1. Recursively scans all subdirectories (supports nested mod organization)
2. Looks for folders named: `CustomStarterPackage`, `Custom Starter Package`, etc.
3. Falls back to checking `manifest.json` for UniqueID `aedenthorn.CustomStarterPackage`

---

## Building & Running

```powershell
# GUI Application
cd "C:\Users\HP\Documents\CustomStarterPackageBuilderGUI"
dotnet build
dotnet run

# ItemDataExtractor Mod
cd "C:\Users\HP\Documents\ItemDataExtractor"
dotnet build
# DLL auto-copies to Mods folder
```

---

## Workflow for Users

1. Install CustomStarterPackage mod
2. Install ItemDataExtractor mod (optional, for mod support)
3. Run Stardew Valley, load a save, press F8 (if using extractor)
4. Run Custom Starter Package Builder GUI
5. Click "Import Item List..." if you extracted items
6. Browse/search for items
7. Add up to 5 items with quantities
8. Save Configuration
9. Play Stardew Valley!

---

## Related Projects in Workspace

- **ItemDataExtractor** (SMAPI mod - companion tool)
  - Source: `C:\Users\HP\Documents\ItemDataExtractor`
  - Deployed: `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\ItemDataExtractor`
  - Extracts all game items including mod-added items
  
- **CustomStarterPackageBuilder** (SMAPI mod - abandoned approach)
  - Location: `C:\Users\HP\Documents\CustomStarterPackageBuilder`
  - Was attempting in-game configuration but CSP loads at startup
  
- **CustomStarterPackage** (the target mod by aedenthorn)
  - Location: `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\Dont use mods\CustomStarterPackage`
  
- **CJBItemSpawner** (reference for UI design)
  - Location: `C:\Users\HP\Downloads\CJBItemSpawner`

---

## Items NOT Included (by design)

- Clothing (selectable in-game character creator)
- Flooring
- Wallpaper

These could be added later by extending ItemDataExtractor.

---

## Completed Features

- [x] Category browsing and search
- [x] 5-item selection limit with quantity input
- [x] Direct export to CSP config.json format
- [x] Auto-detection of CSP mod (recursive search for nested folders)
- [x] D: drive and multiple Steam library path support
- [x] Browse button opens file picker starting in Mods folder
- [x] ItemDataExtractor SMAPI mod for accurate game data
- [x] Tool names fix (DisplayName instead of LocalizedTextString)
- [x] Stone item post-filtering (removes mining nodes, keeps resource)
- [x] Import Item List button for easy item database updates

---

## Session Notes

1. Started with SMAPI mod approach, discovered timing issue (CSP loads at startup)
2. Pivoted to standalone Avalonia GUI
3. User chose Avalonia over WPF for cross-platform support
4. Initially had partial item list, then parsed user's complete text files
5. Fixed output format from Content Patcher to direct CSP config.json
6. Created ItemDataExtractor SMAPI mod for accurate game data (MaxStack, descriptions)
7. Fixed tool names showing "[LocalizedTextString...]" - now uses actual DisplayName
8. Implemented recursive mod folder search (SMAPI supports nested mod organization)
9. Added Import Item List button for one-click item database updates
10. Stone filtering moved to GUI post-filter to preserve items like "Mystic Stone"

---

*Last Updated: January 31, 2026*
