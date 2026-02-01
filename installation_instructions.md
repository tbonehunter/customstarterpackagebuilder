# Custom Starter Package Builder GUI

## Installation Instructions

A standalone visual configuration tool for the **Custom Starter Package** mod by aedenthorn.

---

### Requirements

- **Custom Starter Package** mod must be installed  
  Download from: https://www.nexusmods.com/stardewvalley/mods/14152

- **Content Patcher** mod must be installed  
  Download from: https://www.nexusmods.com/stardewvalley/mods/1915

---

### Installation

1. **Extract** this archive to any location on your computer
2. That's it! No additional setup required.

---

### How to Use

1. **Run** `CustomStarterPackageBuilder.exe`  
   Located in the root folder where you extracted the archive.

2. **Browse or Search** for items:
   - Use the category dropdown to filter by type (Objects, Tools, Weapons, etc.)
   - Type in the search box to find items by name

3. **Select Items**:
   - Double-click an item or click "Add" to add it to your starter package
   - Adjust quantities as needed (respects each item's max stack size)
   - Change the max item limit if you want more than 5 items

4. **Export Content Pack**:
   - Click **"Export to Content Pack"**
   - The tool will auto-detect your Stardew Valley Mods folder
   - If not found, use Browse to locate it manually
   - A `[CP] Custom Starter Package Config` folder will be created

5. **Launch Stardew Valley** and start a new game to receive your configured items!

---

### Important Notes

- Run this tool **BEFORE** launching Stardew Valley
- Custom Starter Package loads your items at game startup, so changes won't apply to games already in progress
- Starting a **new save** is required to receive your configured items

---

### Troubleshooting

**"Could not find Custom Starter Package mod"**  
- Ensure Custom Starter Package is installed in your Stardew Valley Mods folder
- Use the Browse button to manually select your Mods folder

**"Content Patcher not found" errors in SMAPI**  
- Ensure Content Patcher mod is installed in your Mods folder

**Items not appearing in new game**  
- Verify both Custom Starter Package and Content Patcher are enabled in SMAPI
- Check that `[CP] Custom Starter Package Config` folder exists in your Mods folder
- Check the SMAPI console for any error messages

---

### Credits

- **aedenthorn** - Original Custom Starter Package mod
- **tbonehunter** - This configuration tool

---

Version 1.1.0
