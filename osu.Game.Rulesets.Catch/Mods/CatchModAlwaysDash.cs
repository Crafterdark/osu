// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAlwaysDash : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Always Dash";
        public override string Acronym => "AD";
        public override LocalisableString Description => "The catcher won't stop dashing.";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Moon; //Placeholder
        public override ModType Type => ModType.Fun;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModNoDash) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
        }

    }
}
