// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModCatcherTest : Mod, IApplicableToCatcher, IApplicableMod
    {
        public override string Name => "Catcher Test";

        public override string Acronym => "CT";

        public override LocalisableString Description => "Testing...";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Conversion;
    }
}
