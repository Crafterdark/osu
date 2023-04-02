// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModCatchTheMania : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmapProcessor
    {
        public override string Name => "Catch the Mania";
        public override string Acronym => "CM";
        public override LocalisableString Description => "The Playfield is swapped with osu!mania lanes.";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Moon; //Placeholder
        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(CatchModAlwaysDash), typeof(CatchModClassic), typeof(CatchModNoDash), typeof(CatchModRelax), typeof(CatchModTeleport), typeof(CatchModWraparound) };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
        }

    }
}
