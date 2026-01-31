// Models/SelectedItem.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CustomStarterPackageBuilder.Models;

/// <summary>
/// Represents an item selected for inclusion in the starter package.
/// </summary>
public class SelectedItem : INotifyPropertyChanged
{
    private int _quantity = 1;

    /// <summary>The underlying game item data.</summary>
    public GameItem Item { get; }

    /// <summary>The quantity of this item to include in the starter package.</summary>
    public int Quantity
    {
        get => _quantity;
        set
        {
            int newValue = Math.Clamp(value, 1, Item.MaxStack);
            if (_quantity != newValue)
            {
                _quantity = newValue;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Display text for the item (name with quantity if stackable).</summary>
    public string DisplayText => Item.MaxStack > 1 
        ? $"{Item.Name} x{Quantity}" 
        : Item.Name;

    public SelectedItem(GameItem item, int quantity = 1)
    {
        Item = item;
        Quantity = quantity;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        // Also notify DisplayText changed when Quantity changes
        if (propertyName == nameof(Quantity))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
        }
    }
}
