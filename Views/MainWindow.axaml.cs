// Views/MainWindow.axaml.cs
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CustomStarterPackageBuilder.Models;
using CustomStarterPackageBuilder.ViewModels;

namespace CustomStarterPackageBuilder.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        
        // Load data when window opens
        Loaded += async (_, _) => await ViewModel.InitializeAsync();
        
        // Set up double-tap handler for item list
        var itemListBox = this.FindControl<ListBox>("ItemListBox");
        if (itemListBox != null)
        {
            itemListBox.DoubleTapped += OnItemDoubleTapped;
        }
    }

    private void OnAddClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.AddSelectedItem();
    }

    private void OnRemoveClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSelectedItem();
    }

    private void OnClearClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.ClearSelection();
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        // Double-click to add item
        ViewModel.AddSelectedItem();
    }

    private void OnQuantityLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is SelectedItem item)
        {
            ViewModel.UpdateQuantity(item, textBox.Text ?? "1");
        }
    }

    private async void OnBrowseClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        // Try to start in the Mods folder or the current output path's directory
        IStorageFolder? suggestedStartLocation = null;
        var currentPath = ViewModel.OutputPath;
        if (!string.IsNullOrEmpty(currentPath))
        {
            var dir = System.IO.Path.GetDirectoryName(currentPath);
            if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
            {
                suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir);
            }
        }
        
        // Fall back to Mods folder if current path doesn't exist
        if (suggestedStartLocation == null)
        {
            var modsFolder = Services.ConfigExporter.FindModsFolder();
            if (modsFolder != null)
            {
                suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(modsFolder);
            }
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Custom Starter Package Config",
            SuggestedFileName = "config.json",
            DefaultExtension = "json",
            SuggestedStartLocation = suggestedStartLocation,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (file != null)
        {
            ViewModel.OutputPath = file.Path.LocalPath;
        }
    }

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
    }

    private async void OnImportItemsClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        // Try to start in the Mods folder where ItemDataExtractor output might be
        IStorageFolder? suggestedStartLocation = null;
        var modsFolder = Services.ConfigExporter.FindModsFolder();
        if (modsFolder != null)
        {
            suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(modsFolder);
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Extracted Items File",
            AllowMultiple = false,
            SuggestedStartLocation = suggestedStartLocation,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            var sourceFile = files[0].Path.LocalPath;
            await ViewModel.ImportItemsAsync(sourceFile);
        }
    }
}
