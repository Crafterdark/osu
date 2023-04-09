// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModEasy : ModEasyWithExtraLives
    {

        public override Type[] IncompatibleMods => new[] { typeof(CatchModEasy2) };
        public override LocalisableString Description => @"Larger fruits, more forgiving HP drain, less accuracy required, and three lives!";
    }
}
