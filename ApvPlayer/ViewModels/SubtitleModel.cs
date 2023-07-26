using System.Windows.Input;

namespace ApvPlayer.ViewModels;

public class SubtitleModel
{
    public object? Header { set; get; }
    public object? Icon { set; get; }
    public ICommand? Command { set; get; }
    public long? Id { set; get; }
}