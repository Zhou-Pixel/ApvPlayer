using System;
using Avalonia.Controls;

namespace ApvPlayer.ViewModels;

public class Track
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public required string Type { get; set; }
    
    public string? Lang { get; set; }
    
    public MenuItem? Item { get; set; }

    protected bool Equals(Track other)
    {
        return Id == other.Id;
    }
}