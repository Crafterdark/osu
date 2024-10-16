﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModEasy : ModEasy, IApplicableToHealthProcessor
    {
        public override double ScoreMultiplier => Math.Sqrt(0.5);
        public override LocalisableString Description => @"Larger fruits, more forgiving HP drain and less accuracy required!";

        //Internal Extra Lives disabled by default
        public bool ExtraLivesOnGameplay { get; set; }

        private CatchModExtraLives internalModExtraLives = new CatchModExtraLives();

        [SettingSource("Affects approach rate")]
        public BindableBool AffectsApproach { get; } = new BindableBool(true);

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            const float ratio = 0.5f;

            if (AffectsApproach.Value)
                difficulty.ApproachRate *= ratio;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            if (ExtraLivesOnGameplay)
                internalModExtraLives.ApplyToHealthProcessor(healthProcessor);
        }
    }
}
