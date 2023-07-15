// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBias : Mod, IApplicableToHitObject
    {
        public override string Name => "Bias";
        public override string Acronym => "BS";
        public override LocalisableString Description => @"Fruits are biased towards the catcher.";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;

        public void ApplyToHitObject(HitObject hitObject)
        {
        }
    }
}
