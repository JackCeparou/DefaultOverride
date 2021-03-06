using System.Globalization;
using Turbo.Plugins.Default;

// this is an adaptation by Jack of a default hud plugin, all credits to KillerJohn
namespace Turbo.Plugins.DefaultOverride.Inventory
{
    public class StashFreeSpacePlugin : BasePlugin, IInGameTopPainter, ICustomizer
    {
        public IFont Font { get; set; }

        public StashFreeSpacePlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            Font = Hud.Render.CreateFont("tahoma", 7, 160, 255, 30, 30, true, false, 128, 0, 0, 0, true);
        }

        public void Customize()
        {
            Hud.TogglePlugin<Turbo.Plugins.Default.StashUsedSpacePlugin>(false);
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (Hud.Render.UiHidden) return;
            if (clipState != ClipState.Inventory) return;

            var uiStash = Hud.Inventory.StashMainUiElement;
            if (!uiStash.Visible) return;

            var selectedPage = Hud.Inventory.SelectedStashPageIndex;
            var selectedTab = Hud.Inventory.SelectedStashTabIndex;

            for (int tabIndex = 0; tabIndex < Hud.Inventory.MaxStashTabCountPerPage; tabIndex++)
            {
                var tabUiElement = Hud.Inventory.GetStashTabUiElement(tabIndex);
                if (!tabUiElement.Visible) continue;

                var freeSpace = 70 - Hud.Inventory.GetStashTabUsedSpace(selectedPage, tabIndex);
                var text = freeSpace.ToString("D", CultureInfo.InvariantCulture);

                var layout = Font.GetTextLayout(text);

                var x = tabUiElement.Rectangle.Left + ((tabUiElement.Rectangle.Width * 0.94f) - layout.Metrics.Width) / 2;
                if (tabIndex != selectedTab) x -= tabUiElement.Rectangle.Width * 0.170f;

                Font.DrawText(layout, x, tabUiElement.Rectangle.Top + (tabUiElement.Rectangle.Height * 0.1f));
            }
        }
    }
}