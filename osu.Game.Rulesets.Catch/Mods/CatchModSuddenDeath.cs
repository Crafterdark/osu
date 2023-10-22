// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModSuddenDeath : ModSuddenDeath
    {
        [SettingSource("Do not fail by droplets", "Do not restart when missing droplets.")]
        public BindableBool DropletsCannotFail { get; } = new BindableBool(false);

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
    => result.Type.AffectsCombo()
       && !result.IsHit && (!DropletsCannotFail.Value || result.HitObject is not Droplet);
    }
}
