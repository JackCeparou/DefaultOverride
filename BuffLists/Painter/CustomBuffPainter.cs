// this is an adaptation by Jack of a default hud class, all credits to KillerJohn
// only signature changes and add background texture drawing in Paint()
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Turbo.Plugins.DefaultOverride.BuffLists.Painter
{

    public class CustomBuffPainter: ITransparentCollection, ITransparent
    {

        public bool Enabled { get; set; }
        public IController Hud { get; set; }

        public IFont TimeLeftFont { get; set; }
        public IFont StackFont { get; set; }
        public IBrush TimeLeftClockBrush { get; set; }

        public float Opacity { get; set; }

        public bool ShowTooltips { get; set; }
        public bool ShowTimeLeftNumbers { get; set; }
        public bool HasIconBorder { get; set; }

        public CustomBuffPainter(IController hud, bool setDefaultStyle)
        {
            Enabled = true;
            Hud = hud;

            if (setDefaultStyle)
            {
                TimeLeftFont = Hud.Render.CreateFont("tahoma", 8, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
                StackFont = Hud.Render.CreateFont("tahoma", 9, 255, 255, 255, 255, false, false, 255, 0, 0, 0, true);
                TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);
            }

            ShowTooltips = true;
            ShowTimeLeftNumbers = true;
            HasIconBorder = true;
        }

        public void PaintVertical(List<CustomBuffPaintInfo> infoList, float x, float y, float size, float spacing)
        {
            foreach (var info in infoList)
            {
                var s = info.Size;
                Paint(info, new RectangleF(x + (size - s) / 2, y, s, s));
                y += s + spacing;
            }
        }

        public void PaintVerticalCenter(List<CustomBuffPaintInfo> infoList, float x, float y, float height, float size, float spacing)
        {
            var totalHeight = spacing * (infoList.Count - 1);
            foreach (var info in infoList) totalHeight += info.Size;

            y += (height - totalHeight) / 2;

            foreach (var info in infoList)
            {
                var s = info.Size;
                Paint(info, new RectangleF(x + (size - s) / 2, y, s, s));
                y += s + spacing;
            }
        }

        public void PaintHorizontal(List<CustomBuffPaintInfo> infoList, float x, float y, float size, float spacing)
        {
            foreach (var info in infoList)
            {
                var s = info.Size;
                Paint(info, new RectangleF(x, y + (size - s) / 2, s, s));
                x += s + spacing;
            }
        }

        public void PaintHorizontalCenter(List<CustomBuffPaintInfo> infoList, float x, float y, float width, float size, float spacing)
        {
            var totalWidth = spacing * (infoList.Count - 1);
            foreach (var info in infoList) totalWidth += info.Size;

            x += (width - totalWidth) / 2;

            foreach (var info in infoList)
            {
                var s = info.Size;
                Paint(info, new RectangleF(x, y + (size - s) / 2, s, s));
                x += s + spacing;
            }
        }

        protected void Paint(CustomBuffPaintInfo info, RectangleF rect)
        {
            var firstIcon = info.Icons[0];
            var isDebuff = firstIcon.Harmful;
            // change
            if (info.BackgroundBrush != null)
            {
                info.BackgroundBrush.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }
            // end change
            info.Texture.Draw(rect.X, rect.Y, rect.Width, rect.Height, Opacity);

            DrawTimeLeftClock(rect, info.Elapsed, info.TimeLeft);

            if (HasIconBorder)
            {
                (isDebuff ? Hud.Texture.DebuffFrameTexture : Hud.Texture.BuffFrameTexture).Draw(rect.X, rect.Y, rect.Width, rect.Height, Opacity);
            }

            if (Hud.Window.CursorInsideRect(rect.X, rect.Y, rect.Width, rect.Height) && ShowTooltips)
            {
                if (info.Rule == null)
                {
                    var name = info.SnoPower.NameLocalized;
                    if (name == null)
                    {
                        foreach (var icon in info.Icons) name += (name == null ? "" : "\n") + icon.TitleLocalized;
                    }
                    string desc = null;
                    foreach (var icon in info.Icons) desc += (desc == null ? "\n\n" : "\n") + icon.DescriptionLocalized;
                    Hud.Render.SetHint(name + desc);
                }
                else
                {
                    if (firstIcon.Exists)
                    {
                        var name = (!info.Rule.DisableName ? (info.Rule.UsePowersName ? info.SnoPower.NameLocalized : firstIcon.TitleLocalized) : null);
                        var desc = (info.Rule.UsePowersDesc ? info.SnoPower.DescriptionLocalized : (firstIcon.DescriptionLocalized != null ? firstIcon.DescriptionLocalized : ""));
                        Hud.Render.SetHint(name + (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(desc) ? "\n\n" : "") + desc);
                    }
                }
            }

            DrawTimeLeftNumbers(rect, info);

            if (info.Stacks > -1)
            {
                DrawStacks(rect, info.Stacks);
            }
        }

        private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft)
        {
            if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
            {
                var endAngle = Convert.ToInt32((360.0d / (timeLeft + elapsed)) * elapsed);
                var startAngle = 0;
                TimeLeftClockBrush.Opacity = 1 - (float)(0.3f / (timeLeft + elapsed) * elapsed);
                var rad = rect.Width * 0.45f;
                using (var pg = Hud.Render.CreateGeometry())
                {
                    using (var gs = pg.Open())
                    {
                        gs.BeginFigure(rect.Center, FigureBegin.Filled);
                        for (int angle = startAngle; angle <= endAngle; angle++)
                        {
                            var mx = rad * (float)Math.Cos((angle - 90) * Math.PI / 180.0f);
                            var my = rad * (float)Math.Sin((angle - 90) * Math.PI / 180.0f);
                            var vec = new Vector2(rect.Center.X + mx, rect.Center.Y + my);
                            gs.AddLine(vec);
                        }
                        gs.EndFigure(FigureEnd.Closed);
                        gs.Close();
                    }
                    TimeLeftClockBrush.DrawGeometry(pg);
                }
            }
        }

        private void DrawTimeLeftNumbers(RectangleF rect, CustomBuffPaintInfo info)
        {
            if (info.TimeLeft == 0) return;

            if (!ShowTimeLeftNumbers) return;
            if (info.TimeLeftNumbersOverride != null && info.TimeLeftNumbersOverride.Value == false) return;

            var text = "";
            if (info.TimeLeft > 1.0f)
            {
                var mins = Convert.ToInt32(Math.Floor(info.TimeLeft / 60.0d));
                var secs = Math.Floor(info.TimeLeft - mins * 60.0d);
                if (info.TimeLeft >= 60)
                {
                    text = mins.ToString("F0", CultureInfo.InvariantCulture) + ":" + (secs < 10 ? "0" : "") + secs.ToString("F0", CultureInfo.InvariantCulture);
                }
                else text = info.TimeLeft.ToString("F0", CultureInfo.InvariantCulture);
            }
            else text = info.TimeLeft.ToString("F1", CultureInfo.InvariantCulture);

            var layout = TimeLeftFont.GetTextLayout(text);
            TimeLeftFont.DrawText(layout, rect.X + (rect.Width - (float)Math.Ceiling(layout.Metrics.Width)) / 2.0f, rect.Y + (rect.Height - layout.Metrics.Height) / 2);
        }

        private void DrawStacks(RectangleF rect, int stacks)
        {
            var layout = StackFont.GetTextLayout(stacks.ToString());
            StackFont.DrawText(layout, rect.Right - (rect.Width / 8.0f) - (float)Math.Ceiling(layout.Metrics.Width), rect.Bottom - layout.Metrics.Height - (rect.Width / 15.0f));
        }

        public IEnumerable<ITransparent> GetTransparents()
        {
            yield return TimeLeftFont;
            yield return StackFont;
            yield return TimeLeftClockBrush;
            yield return this;
        }
    }

}