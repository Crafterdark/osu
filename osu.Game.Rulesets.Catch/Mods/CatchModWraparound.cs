// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModWraparound : Mod, IApplicableMod
    {
        public override string Name => "Wraparound";
        public override string Acronym => "WD";
        public override LocalisableString Description => @"Exit the gameplay area from a side and return from the other!";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModRelax) };
    }
}
