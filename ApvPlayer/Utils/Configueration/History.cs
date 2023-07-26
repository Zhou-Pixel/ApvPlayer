using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace ApvPlayer.Utils.Configueration;

public class History
{
    [TomlProperty("records")]
    public List<SingleRecord> Records { get; set; } = new();
}



public class SingleRecord
{
    [TomlProperty("finished")]
    public bool Finished { get; set; }
    
    [TomlProperty("position")]
    public double Position { get; set; }
    
    [TomlProperty("path")]
    public required string Path { get; set; }
    
    [TomlProperty("guid")]
    public required string Guid { get; set; }
    
    [TomlProperty("time")]
    public required DateTime Time { get; set; }
}