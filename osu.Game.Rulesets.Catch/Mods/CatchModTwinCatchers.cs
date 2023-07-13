// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTwinCatchers : Mod, IApplicableMod
    {
        public override string Name => "Twin Catchers";
        public override string Acronym => "TC";
        public override LocalisableString Description => @"Play as 2 catchers... at the same time!";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModSupport) };

    }
}
