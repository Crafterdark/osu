// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultLivesCounter : RollingCounter<int>, ISerialisableDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, HealthProcessor healthProcessor)
        {
            Colour = colours.GreenLighter;
            Current.BindTo(healthProcessor.Lives);
        }

        public bool UsesFixedAnchor { get; set; }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f));
    }
}
