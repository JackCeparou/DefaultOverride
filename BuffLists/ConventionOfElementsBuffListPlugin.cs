using System.Collections.Generic;
using System.Linq;
using Turbo.Plugins.Default;

// this is an adaptation by Jack of a default hud plugin, all credits to KillerJohn
namespace Turbo.Plugins.DefaultOverride.BuffLists
{
    // original idea from http://turbohud.freeforums.net/user/8161 and http://turbohud.freeforums.net/user/12675 and http://turbohud.freeforums.net/user/11953
    public class ConventionOfElementsBuffListPlugin : BasePlugin, IInGameTopPainter, ICustomizer
    {
        public bool HideWhenUiIsHidden { get; set; }
        // change
        public bool ShowOnPlayer { get; set; }
        public bool ShowOnOtherPlayers { get; set; }
        public float OffsetZ { get; set; }
        // end change
        public BuffPainter BuffPainter { get; set; }

        private BuffRuleCalculator _ruleCalculator;

        public ConventionOfElementsBuffListPlugin()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            HideWhenUiIsHidden = false;
            // change
            ShowOnPlayer = true;
            ShowOnOtherPlayers = true;
            OffsetZ = 0.0f;
            // end change
            BuffPainter = new BuffPainter(Hud, true)
            {
                Opacity = 1.0f,
                TimeLeftFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true),
            };

            _ruleCalculator = new BuffRuleCalculator(Hud);
            _ruleCalculator.SizeMultiplier = 0.55f;

            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 1, MinimumIconCount = 0, DisableName = true }); // Arcane
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 2, MinimumIconCount = 0, DisableName = true }); // Cold
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 3, MinimumIconCount = 0, DisableName = true }); // Fire
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 4, MinimumIconCount = 0, DisableName = true }); // Holy
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 5, MinimumIconCount = 0, DisableName = true }); // Lightning
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 6, MinimumIconCount = 0, DisableName = true }); // Physical
            _ruleCalculator.Rules.Add(new BuffRule(430674) { IconIndex = 7, MinimumIconCount = 0, DisableName = true }); // Poison
        }

        public void Customize()
        {
            Hud.TogglePlugin<Turbo.Plugins.Default.ConventionOfElementsBuffListPlugin>(false);
        }

        private IEnumerable<BuffRule> GetCurrentRules(HeroClass heroClass)
        {
            for (int i = 1; i <= 7; i++)
            {
                switch (heroClass)
                {
                    case HeroClass.Barbarian: if (i == 1 || i == 4 || i == 7) continue; break;
                    case HeroClass.Crusader: if (i == 1 || i == 2 || i == 7) continue; break;
                    case HeroClass.DemonHunter: if (i == 1 || i == 4 || i == 7) continue; break;
                    case HeroClass.Monk: if (i == 1 || i == 7) continue; break;
                    case HeroClass.WitchDoctor: if (i == 1 || i == 4 || i == 5) continue; break;
                    case HeroClass.Wizard: if (i == 4 || i == 6 || i == 7) continue; break;
                    case HeroClass.Necromancer: if (i == 1 || i == 3 || i == 4 || i == 5) continue; break;
                }
                yield return _ruleCalculator.Rules[i - 1];
            }
        }

        public void PaintTopInGame(ClipState clipState)
        {
            if (clipState != ClipState.BeforeClip) return;
            if (HideWhenUiIsHidden && Hud.Render.UiHidden) return;

            foreach (var player in Hud.Game.Players)
            {
                if (player.ActorId == 0) continue;
                if (!ShowOnOtherPlayers && !player.IsMe) continue;

                var buff = player.Powers.GetBuff(430674);
                if ((buff == null) || (buff.IconCounts[0] <= 0)) continue;

                var classSpecificRules = GetCurrentRules(player.HeroClassDefinition.HeroClass);

                _ruleCalculator.CalculatePaintInfo(player, classSpecificRules);

                if (_ruleCalculator.PaintInfoList.Count == 0) return;
                if (!_ruleCalculator.PaintInfoList.Any(info => info.TimeLeft > 0)) return;

                var highestElementalBonus = player.Offense.HighestElementalDamageBonus;

                for (int i = 0; i < _ruleCalculator.PaintInfoList.Count; i++)
                {
                    var info = _ruleCalculator.PaintInfoList[0];
                    if (info.TimeLeft <= 0)
                    {
                        _ruleCalculator.PaintInfoList.RemoveAt(0);
                        _ruleCalculator.PaintInfoList.Add(info);
                    }
                    else break;
                }

                for (int orderIndex = 0; orderIndex < _ruleCalculator.PaintInfoList.Count; orderIndex++)
                {
                    var info = _ruleCalculator.PaintInfoList[orderIndex];
                    var best = false;
                    switch (info.Rule.IconIndex)
                    {
                        case 1: best = player.Offense.BonusToArcane == highestElementalBonus; break;
                        case 2: best = player.Offense.BonusToCold == highestElementalBonus; break;
                        case 3: best = player.Offense.BonusToFire == highestElementalBonus; break;
                        case 4: best = player.Offense.BonusToHoly == highestElementalBonus; break;
                        case 5: best = player.Offense.BonusToLightning == highestElementalBonus; break;
                        case 6: best = player.Offense.BonusToPhysical == highestElementalBonus; break;
                        case 7: best = player.Offense.BonusToPoison == highestElementalBonus; break;
                    }
                    if (best) info.Size *= 1.35f;
                    if (best && orderIndex > 0)
                    {
                        info.TimeLeft = (orderIndex - 1) * 4 + _ruleCalculator.PaintInfoList[0].TimeLeft;
                    }
                    else info.TimeLeftNumbersOverride = false;
                }

                var portraitRect = player.PortraitUiElement.Rectangle;

                var x = portraitRect.Right;
                var y = portraitRect.Top + portraitRect.Height * 0.51f;

                // change
                if (ShowOnPlayer && (!Hud.Game.IsInTown || (player.IsOnScreen && !player.IsMe)))
                {
                    var screenCoordinate = OffsetZ != 0 ? player.FloorCoordinate.Offset(0, 0, OffsetZ).ToScreenCoordinate() : player.FloorCoordinate.ToScreenCoordinate();
                    x = screenCoordinate.X - _ruleCalculator.StandardIconSize * _ruleCalculator.PaintInfoList.Count / 2f;
                    y = screenCoordinate.Y + _ruleCalculator.StandardIconSize;
                }
                // end change

                BuffPainter.PaintHorizontal(_ruleCalculator.PaintInfoList, x, y, _ruleCalculator.StandardIconSize, 0);
            }
        }
    }
}