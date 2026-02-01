// ViewModels/MainWindowViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CustomStarterPackageBuilder.Models;
using CustomStarterPackageBuilder.Services;

namespace CustomStarterPackageBuilder.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ItemDatabase _itemDatabase = new();
    private readonly ConfigExporter _exporter = new();
    
    private string _searchText = "";
    private string _selectedCategory = "All";
    private GameItem? _selectedItem;
    private SelectedItem? _selectedSelectedItem;
    private string _statusMessage = "Ready";
    private bool _isLoading;
    private string _outputPath = "";
    private int _maxSelectedItems = 5;

    public MainWindowViewModel()
    {
        SelectedItems = new ObservableCollection<SelectedItem>();
        FilteredItems = new ObservableCollection<GameItem>();
        Categories = new ObservableCollection<string>();
        
        // Try to find Stardew Valley Mods folder
        var modsFolder = ConfigExporter.FindModsFolder();
        if (modsFolder != null)
        {
            OutputPath = modsFolder;
        }
        else
        {
            // Fallback to Documents folder
            OutputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "StardewValleyMods");
        }
    }

    #region Properties

    public ObservableCollection<SelectedItem> SelectedItems { get; }
    public ObservableCollection<GameItem> FilteredItems { get; }
    public ObservableCollection<string> Categories { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                UpdateFilteredItems();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                UpdateFilteredItems();
            }
        }
    }

    public GameItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddItem));
            }
        }
    }

    public SelectedItem? SelectedSelectedItem
    {
        get => _selectedSelectedItem;
        set
        {
            if (_selectedSelectedItem != value)
            {
                _selectedSelectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRemoveItem));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    public string OutputPath
    {
        get => _outputPath;
        set
        {
            if (_outputPath != value)
            {
                _outputPath = value;
                OnPropertyChanged();
            }
        }
    }

    public int MaxSelectedItems
    {
        get => _maxSelectedItems;
        set
        {
            // Ensure minimum of 1 and reasonable maximum
            var newValue = Math.Max(1, Math.Min(value, 999));
            if (_maxSelectedItems != newValue)
            {
                _maxSelectedItems = newValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddItem));
                OnPropertyChanged(nameof(RemainingSlots));
                OnPropertyChanged(nameof(SelectionCountText));
                OnPropertyChanged(nameof(MaxItemsText));
            }
        }
    }

    public string MaxItemsText
    {
        get => _maxSelectedItems.ToString();
        set
        {
            if (int.TryParse(value, out int parsed))
            {
                MaxSelectedItems = parsed;
            }
        }
    }

    public bool CanAddItem => SelectedItem != null && 
                              SelectedItems.Count < MaxSelectedItems &&
                              !SelectedItems.Any(s => s.Item.Id == SelectedItem.Id && s.Item.Type == SelectedItem.Type);

    public bool CanRemoveItem => SelectedSelectedItem != null;

    public bool CanSave => SelectedItems.Count > 0 && !string.IsNullOrWhiteSpace(OutputPath);

    public int RemainingSlots => MaxSelectedItems - SelectedItems.Count;

    public string SelectionCountText => $"{SelectedItems.Count} / {MaxSelectedItems} items selected";

    #endregion

    #region Methods

    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading item database...";

        try
        {
            await _itemDatabase.LoadAsync();

            Categories.Clear();
            foreach (var category in _itemDatabase.Categories)
            {
                Categories.Add(category);
            }

            SelectedCategory = "All";
            UpdateFilteredItems();

            StatusMessage = $"Loaded {_itemDatabase.AllItems.Count} items. Select up to {MaxSelectedItems} items for your starter package.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading items: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void AddSelectedItem()
    {
        if (SelectedItem == null || SelectedItems.Count >= MaxSelectedItems)
            return;

        // Check if already selected
        if (SelectedItems.Any(s => s.Item.Id == SelectedItem.Id && s.Item.Type == SelectedItem.Type))
        {
            StatusMessage = $"{SelectedItem.Name} is already in your selection.";
            return;
        }

        var selected = new SelectedItem(SelectedItem);
        SelectedItems.Add(selected);
        
        StatusMessage = $"Added {SelectedItem.Name} to starter package.";
        
        OnPropertyChanged(nameof(CanAddItem));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(RemainingSlots));
        OnPropertyChanged(nameof(SelectionCountText));
    }

    public void RemoveSelectedItem()
    {
        if (SelectedSelectedItem == null)
            return;

        var itemName = SelectedSelectedItem.Item.Name;
        SelectedItems.Remove(SelectedSelectedItem);
        SelectedSelectedItem = null;

        StatusMessage = $"Removed {itemName} from starter package.";
        
        OnPropertyChanged(nameof(CanAddItem));
        OnPropertyChanged(nameof(CanRemoveItem));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(RemainingSlots));
        OnPropertyChanged(nameof(SelectionCountText));
    }

    public void ClearSelection()
    {
        SelectedItems.Clear();
        SelectedSelectedItem = null;
        
        StatusMessage = "Cleared all selections.";
        
        OnPropertyChanged(nameof(CanAddItem));
        OnPropertyChanged(nameof(CanRemoveItem));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(RemainingSlots));
        OnPropertyChanged(nameof(SelectionCountText));
    }

    public async Task SaveAsync()
    {
        if (SelectedItems.Count == 0)
        {
            StatusMessage = "No items selected. Please add at least one item.";
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            StatusMessage = "Please specify the Stardew Valley Mods folder.";
            return;
        }

        // Verify the Mods folder exists
        if (!Directory.Exists(OutputPath))
        {
            StatusMessage = $"Mods folder not found: {OutputPath}";
            return;
        }

        // Check if Custom Starter Package mod is installed
        var cspFolder = ConfigExporter.FindCSPModFolder(OutputPath);
        if (cspFolder == null)
        {
            StatusMessage = "Custom Starter Package mod not found in Mods folder. Please install it first.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Creating Content Patcher pack...";

        try
        {
            await _exporter.ExportAsync(SelectedItems, OutputPath);
            var packPath = ConfigExporter.GetContentPackPath(OutputPath);
            StatusMessage = $"Created Content Patcher pack with {SelectedItems.Count} items at: {packPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Import an extracted items JSON file and reload the item database.
    /// </summary>
    public async Task ImportItemsAsync(string sourceFilePath)
    {
        IsLoading = true;
        StatusMessage = "Importing items...";

        try
        {
            // Get the target path for items.json
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            var targetPath = Path.Combine(exePath, "Data", "items.json");

            // Ensure Data directory exists
            var dataDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            // Copy the file
            File.Copy(sourceFilePath, targetPath, overwrite: true);

            // Clear current selection (items may no longer exist)
            SelectedItems.Clear();
            OnPropertyChanged(nameof(SelectedItems));
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(SelectionCountText));
            OnPropertyChanged(nameof(RemainingSlots));

            // Reload the item database
            await _itemDatabase.LoadAsync();

            // Refresh categories
            Categories.Clear();
            foreach (var cat in _itemDatabase.Categories)
            {
                Categories.Add(cat);
            }
            SelectedCategory = "All";

            // Update filtered items
            UpdateFilteredItems();

            StatusMessage = $"Imported {_itemDatabase.AllItems.Count} items from: {Path.GetFileName(sourceFilePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing items: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdateQuantity(SelectedItem item, string quantityText)
    {
        if (int.TryParse(quantityText, out int quantity))
        {
            item.Quantity = quantity;
            OnPropertyChanged(nameof(SelectedItems));
        }
    }

    private void UpdateFilteredItems()
    {
        FilteredItems.Clear();

        var items = _itemDatabase.SearchItems(SearchText, SelectedCategory);
        foreach (var item in items.OrderBy(i => i.Name))
        {
            FilteredItems.Add(item);
        }

        StatusMessage = $"Showing {FilteredItems.Count} items" + 
                       (string.IsNullOrEmpty(SearchText) ? "" : $" matching \"{SearchText}\"");
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
