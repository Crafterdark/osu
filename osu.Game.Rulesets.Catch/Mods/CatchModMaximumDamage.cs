// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModMaximumDamage : ModMaximumDamage
    {
        [SettingSource("Affects accuracy", "Tiny droplets deal maximum damage.")]
        public override BindableBool AffectsAccuracy { get; } = new BindableBool();
    }
}
