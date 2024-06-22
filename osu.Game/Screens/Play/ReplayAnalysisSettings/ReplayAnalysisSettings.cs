// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Replays;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;

namespace osu.Game.Screens.Play.ReplayAnalysis
{
    public partial class ReplayAnalysisSettings : PlayerSettingsGroup
    {
        public ReplayAnalysisSettings(MasterGameplayClockContainer clock, List<ReplayFrame> frames) :
            base("Replay Analysis")
        {
            PlayfieldClock = clock;
            Frames = frames;
        }

        protected FillFlowContainer TextFieldFillFlowContainer = null!;

        public MasterGameplayClockContainer PlayfieldClock;

        public List<ReplayFrame> Frames;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        TextFieldFillFlowContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 8),
                            Children = new Drawable[]
                            {
                                //The replay analysis info should be added here.
                            }
                        }
                    }
                }
            };
        }
    }
}
