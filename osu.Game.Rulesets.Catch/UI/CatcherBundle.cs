// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherBundle
    {
        public Catcher Catcher { get; set; }
        public CatchComboDisplay ComboDisplay;
        public CatcherTrailDisplay CatcherTrailDisplay;
        public DroppedObjectContainer DroppedObjectContainer;

        public bool CanCatch;

        public CatcherBundle(BeatmapDifficulty beatmapDifficulty)
        {
            DroppedObjectContainer = new DroppedObjectContainer();
            Catcher = new Catcher(DroppedObjectContainer, beatmapDifficulty)
            {
                X = CatchPlayfield.CENTER_X
            };
            CatcherTrailDisplay = new CatcherTrailDisplay();
            ComboDisplay = new CatchComboDisplay
            {
                RelativeSizeAxes = Axes.None,
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Bottom = 350f },
                X = CatchPlayfield.CENTER_X
            };

        }
    }
}
