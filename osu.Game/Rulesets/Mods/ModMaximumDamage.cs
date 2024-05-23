// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModMaximumDamage : ModFailCondition
    {
        public override string Name => "Maximum Damage";
        public override string Acronym => "MD";
        public override IconUsage? Icon => FontAwesome.Solid.SkullCrossbones;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Miss once and your health goes down...";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModPerfect), typeof(ModSuddenDeath) }).ToArray();

        [SettingSource("Affects accuracy", "Accuracy losses deal maximum damage.")]
        public virtual BindableBool AffectsAccuracy { get; } = new BindableBool();

        [SettingSource("Immediate life loss", "Lose a life after receiving maximum damage.")]
        public virtual BindableBool LifeLossImmediate { get; } = new BindableBool();

        protected override bool LocalFailCondition(HealthProcessor healthProcessor, JudgementResult result)
    => LifeLossImmediate.Value && judgeResultForFailCondition(healthProcessor, result);

        protected override bool GlobalFailCondition(HealthProcessor healthProcessor, JudgementResult result) => false;

        protected bool SuddenDeathCondition(HealthProcessor healthProcessor, JudgementResult result)
            => result.Type.AffectsCombo()
               && !result.IsHit;

        protected bool PerfectCondition(HealthProcessor healthProcessor, JudgementResult result)
    => (isRelevantResult(result.Judgement.MinResult) || isRelevantResult(result.Judgement.MaxResult) || isRelevantResult(result.Type))
       && result.Type != result.Judgement.MaxResult;

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();

        private bool judgeResultForFailCondition(HealthProcessor healthProcessor, JudgementResult result) => AffectsAccuracy.Value ? PerfectCondition(healthProcessor, result) : SuddenDeathCondition(healthProcessor, result);

        public override void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            base.ApplyToHealthProcessor(healthProcessor);

            healthProcessor.NewJudgement += (r) =>
            {
                if (judgeResultForFailCondition(healthProcessor, r))
                {
                    healthProcessor.Health.Value = healthProcessor.Health.MinValue;
                }
            };
        }
    }
}
