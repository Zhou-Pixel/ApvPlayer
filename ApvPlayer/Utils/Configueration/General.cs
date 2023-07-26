using Tomlet.Attributes;

namespace ApvPlayer.Utils.Configueration;

public class General
{
    [TomlProperty("screenshot")]
    public ScreenShotSettings ScreenShot { get; set; } = new();
}