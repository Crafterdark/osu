﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Catch.Mods.DebugMods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoFail : ModNoFail
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModAutopilot)).ToArray();
    }
}
