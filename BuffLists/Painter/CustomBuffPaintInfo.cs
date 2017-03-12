// this is an adaptation by Jack of a default hud class, all credits to KillerJohn
namespace Turbo.Plugins.DefaultOverride.BuffLists.Painter
{
    using Turbo.Plugins.Default;

    public class CustomBuffPaintInfo : BuffPaintInfo
    {
        public new CustomBuffRule Rule { get; set; }
        public IBrush BackgroundBrush { get; set; }
    }
}