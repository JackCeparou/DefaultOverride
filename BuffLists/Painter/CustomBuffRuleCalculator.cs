// this is an adaptation by Jack of a default hud class, all credits to KillerJohn
namespace Turbo.Plugins.DefaultOverride.BuffLists.Painter
{
    using System.Collections.Generic;
    using System.Linq;

    using Turbo.Plugins.Default;

    public class CustomBuffRuleCalculator : BuffRuleCalculator
    {
        // override type
        public new List<CustomBuffRule> Rules { get; private set; }
        public new List<CustomBuffPaintInfo> PaintInfoList { get; private set; }

        public CustomBuffRuleCalculator(IController hud) : base(hud)
        {
            // override type
            Rules = new List<CustomBuffRule>();
            PaintInfoList = new List<CustomBuffPaintInfo>();
        }
        // no choice but to copy
        public new void CalculatePaintInfo(IPlayer player)
        {
            var iconSize = StandardIconSize;

            PaintInfoList.Clear();
            foreach (var rule in Rules)
            {
                GetPaintInfo(player, PaintInfoList, rule, iconSize);
            }
        }
        // no choice but to copy : signature
        public void CalculatePaintInfo(IPlayer player, IEnumerable<CustomBuffRule> customRules)
        {
            var iconSize = StandardIconSize;

            PaintInfoList.Clear();
            foreach (var rule in customRules)
            {
                GetPaintInfo(player, PaintInfoList, rule, iconSize);
            }
        }
        // no choice but to copy : one change + signature
        private void GetPaintInfo(IPlayer player, ICollection<CustomBuffPaintInfo> container, CustomBuffRule rule, float iconSize)
        {
            var buff = player.Powers.GetBuff(rule.PowerSno);
            if (buff == null || !buff.Active)
            {
                return;
            }

            for (var iconIndex = 0; iconIndex < buff.TimeLeftSeconds.Length; iconIndex++)
            {
                var timeLeft = buff.TimeLeftSeconds[iconIndex];
                if (timeLeft < 0) timeLeft = 0;

                if ((rule.IconIndex != null) && (iconIndex != rule.IconIndex.Value)) continue;
                if ((rule.NoIconIndex != null) && (iconIndex == rule.IconIndex.Value)) continue;
                if (buff.IconCounts[iconIndex] < rule.MinimumIconCount) continue;

                var stacks = rule.ShowStacks ? buff.IconCounts[iconIndex] : -1;
                if (!rule.ShowTimeLeft) timeLeft = 0;

                var icon = buff.SnoPower.Icons[iconIndex];

                if (!icon.MergesTooltip || !rule.AllowInGameMergeRules)
                {
                    var id = (buff.SnoPower.Sno << 32) + (uint)iconIndex;
                    if (container.Any(x => x.Id == id)) return;

                    var info = new CustomBuffPaintInfo()
                    {
                        Id = id,
                        SnoPower = buff.SnoPower,
                        Icons = new List<SnoPowerIcon>() { icon },
                        TimeLeft = timeLeft,
                        Elapsed = buff.TimeElapsedSeconds[iconIndex],
                        Stacks = stacks,
                        Rule = rule,
                    };
                    // change
                    info.Texture = info.Rule.ItemSno == 0 ? GetIconTexture(info) : ItemTexture(info);
                    // end change
                    info.Size = iconSize * rule.IconSizeMultiplier;
                    if (info.Texture != null)
                    {
                        container.Add(info);
                    }
                }
                else
                {
                    var id = (buff.SnoPower.Sno << 32) + icon.MergesTooltipIndex;
                    var info = container.FirstOrDefault(x => x.Id == id);
                    if (info != null)
                    {
                        info.Icons.Add(icon);
                    }
                }
            }
        }
        // no choice but to copy : no change
        private ITexture GetIconTexture(CustomBuffPaintInfo info)
        {
            uint textureId = 0;
            if (info.Rule != null)
            {
                textureId = info.SnoPower.NormalIconTextureId;
                if (!info.Rule.UsePowersTexture && info.Icons[0].Exists && (info.Icons[0].TextureId != 0)) textureId = info.Icons[0].TextureId;
            }
            else
            {
                textureId = info.Icons[0].TextureId;
            }
            if (textureId <= 0) return null;

            return Hud.Texture.GetTexture(textureId);
        }
        // new
        private ITexture ItemTexture(CustomBuffPaintInfo info)
        {
            var item = Hud.Inventory.GetSnoItem(info.Rule.ItemSno);
            if (item != null)
            {
                if (info.BackgroundBrush == null)
                {
                    info.BackgroundBrush = item.SetItemBonusesSno == uint.MaxValue
                        ? Hud.Render.CreateBrush(160, 255, 140, 0, 0)
                        : Hud.Render.CreateBrush(160, 50, 220, 50, 0);
                }

                return Hud.Texture.GetItemTexture(item);
            }

            return null;
        }
    }
}