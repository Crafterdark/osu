// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDodgeFruits : Mod, IApplicableMod
    {
        public override string Name => "Dodge Fruits";
        public override string Acronym => "DF";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => @"Dodge the beat!";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1 : 1;

    }
}
