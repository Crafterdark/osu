﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using System.Linq;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAnotherEasy : ModEasyWithExtraLives, IApplicableToDifficulty, IApplicableToBeatmapProcessor
    {

        //TODO: Add an option for adding extra hyperdashes

        public override string Name => "Another Easy";
        public override string Acronym => "AE";
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModEasy), typeof(CatchModHardRock), typeof(ModAccuracyChallenge), typeof(CatchModDifficultyAdjust) };

        [SettingSource("Easier Edge Patterns", "Extra hyper dashes will generate on difficult edge patterns. [33% of the Catcher plate]")]
        public Bindable<bool> NewHyperdashes { get; } = new BindableBool(true);

        public override string SettingDescription
        {
            get
            {
                string newHyperdashes_string = NewHyperdashes.IsDefault ? string.Empty : string.Empty;

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    newHyperdashes_string,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public new virtual void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public new virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            const float ar_min = 5.0f; //most Cup difficulty are AR5+
            const float ar_max = 9.0f; //most Rain difficulty are AR9-

            difficulty.ApproachRate = Math.Clamp(difficulty.ApproachRate * 2, ar_min, ar_max);
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.AnotherEasyOffsets = true;
            catchProcessor.AnotherEasyNewHyperdashes = NewHyperdashes.Value;
        }

        public override LocalisableString Description => @"Larger fruits, more forgiving HP drain, less accuracy required, frequent hyperdashes and three lives!";
    }
}
