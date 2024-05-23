// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRandom : ModRandom, IApplicableToBeatmapProcessor
    {
        public override LocalisableString Description => "It never gets boring!";

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            Seed.Value ??= RNG.Next();

            ((CatchBeatmapProcessor)beatmapProcessor).RandomPatterns = true;
            ((CatchBeatmapProcessor)beatmapProcessor).CustomSeed = (int)Seed.Value;
        }
    }
}
