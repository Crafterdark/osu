// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSuddenDeath : ModFailCondition
    {
        public override string Name => "Sudden Death";
        public override string Acronym => "SD";
        public override IconUsage? Icon => OsuIcon.ModSuddenDeath;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Miss and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModPerfect), typeof(ModMaximumDamage), typeof(ModExtraLives), typeof(ModNoFail), typeof(ModExtremeCustomize) }).ToArray();

        protected override bool GlobalFailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => result.Type.AffectsCombo()
               && !result.IsHit;

        protected override bool LocalFailCondition(HealthProcessor healthProcessor, JudgementResult result) => false;

        public override bool LocalPerformFail() => false;

        public override bool GlobalPerformFail() => true;
    }
}
