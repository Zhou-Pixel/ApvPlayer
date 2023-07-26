using Tomlet.Attributes;

namespace ApvPlayer.Utils.Configueration;

public class ScreenShotSettings
{
    [TomlProperty("screenshot_with_subtitle")]
    public bool ScreenShotWithSubtitle { get; set; } = true;
}