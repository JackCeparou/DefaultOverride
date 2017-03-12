// this is an adaptation by Jack of a default hud class, all credits to KillerJohn
namespace Turbo.Plugins.DefaultOverride.BuffLists.Painter
{
    using Turbo.Plugins.Default;

    public class CustomBuffRule : BuffRule
    {
        public uint ItemSno { get; set; }

        public CustomBuffRule(uint powerSno, uint itemSno = 0) : base(powerSno)
        {
            ItemSno = itemSno;
        }
    }

}