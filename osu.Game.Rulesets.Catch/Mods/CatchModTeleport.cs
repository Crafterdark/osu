// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModTeleport : Mod, IApplicableMod
    {
        public override string Name => "Teleport";
        public override string Acronym => "TT";
        public override LocalisableString Description => @"Hyperdashing at superluminal speed.";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModSupport), typeof(CatchModRelax) };

    }
}
