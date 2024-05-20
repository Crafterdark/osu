// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModWithExtraLives : Mod, IApplicableHealthFailOverride, IApplicableToHealthProcessor, IApplicableToDifficulty
    {
        public override string Name => "Extra Lives";
        public override string Acronym => "EL";
        public override IconUsage? Icon => FontAwesome.Solid.Heart;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => Math.Sqrt(0.5);
        public override LocalisableString Description => @"More chances before you fail...";
        public override Type[] IncompatibleMods => new[] { typeof(ModAccuracyChallenge), typeof(ModNoFail)};

        [SettingSource("Extra Lives", "Number of extra lives")]
        public Bindable<int> Retries { get; } = new BindableInt(2)
        {
            MinValue = 1,
            MaxValue = 10
        };

        public override string SettingDescription => Retries.IsDefault ? string.Empty : $"{"lives".ToQuantity(Retries.Value)}";

        private int retries;

        private readonly BindableNumber<double> health = new BindableDouble();

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            retries = Retries.Value;
        }

        public bool LocalPerformFail()
        {
            if (retries == 0) return true;

            health.Value = health.MaxValue;
            retries--;

            return false;
        }

        public bool GlobalPerformFail() => false;

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            health.BindTo(healthProcessor.Health);
        }
    }
}
