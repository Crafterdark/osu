// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public class ModExtremeCustomize : Mod, IApplicableMod
    {
        public override string Name => "Extreme Customize";

        public override string Acronym => "EC";

        public override LocalisableString Description => "Customize gameplay with extreme values...";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.Tools;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(ModExtraLives), typeof(ModSuddenDeath), typeof(ModPerfect) };
    }
}
