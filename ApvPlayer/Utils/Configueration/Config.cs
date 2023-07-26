using System;
using System.IO;
using System.Threading.Tasks;
using Tomlet;

namespace ApvPlayer.Utils.Configueration;

public class Config
{
    public History History { get; set; }
    
    public Settings Settings { get; set; }

    private static readonly Lazy<Config> LazyInstance = new (() => new Config());

    private string ConfigPath { get; }

    public static Config Instance { get; } =  LazyInstance.Value;

 
    private Config()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configPath = $"{path}/.apvplayer";
        ConfigPath = configPath;
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }

        if (!File.Exists($"{configPath}/history.toml"))
        {
            using var _ = File.Create($"{configPath}/history.toml");
        }

        if (!File.Exists($"{configPath}/settings.toml"))
        {
            using var _ = File.Create($"{configPath}/settings.toml");
        }
        
        var history = File.ReadAllText($"{ConfigPath}/history.toml");
        History = string.IsNullOrEmpty(history)
            ? new History()
            : TomletMain.To<History>(history);

        var settings = File.ReadAllText($"{ConfigPath}/settings.toml");
        Settings = string.IsNullOrEmpty(settings)
            ? new Settings()
            : TomletMain.To<Settings>(settings);
    }
    
    public void AddRecord(SingleRecord record)
    {
        History.Records.Add(record);
        while (History.Records.Count > 10)
            History.Records.RemoveAt(0);
    }


    public async Task RefleshHistory()
    {
        var str = TomletMain.TomlStringFrom(History);
        await File.WriteAllTextAsync($"{ConfigPath}/history.toml" ,str);
    }
    
    public async Task RefleshSettings()
    {
        var str = TomletMain.TomlStringFrom(Settings);
        await File.WriteAllTextAsync($"{ConfigPath}/settings.toml" ,str);
    }
            
}