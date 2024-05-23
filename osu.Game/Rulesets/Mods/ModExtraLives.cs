// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModExtraLives : Mod, IApplicableToHealthProcessor
    {
        public override string Name => "Extra Lives";
        public override string Acronym => "EL";
        public override IconUsage? Icon => FontAwesome.Solid.Heart;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => Math.Sqrt(0.5);
        public override LocalisableString Description => @"More chances before you fail...";
        public override Type[] IncompatibleMods => new[] { typeof(ModSuddenDeath), typeof(ModPerfect), typeof(ModExtremeCustomize) };

        [SettingSource("Extra Lives", "Number of extra lives")]
        public BindableNumber<int> Lives { get; } = new BindableInt(2)
        {
            MinValue = 1,
            MaxValue = 10
        };

        public override string SettingDescription => Lives.IsDefault ? string.Empty : $"{"lives".ToQuantity(Lives.Value)}";

        private readonly BindableInt livesFromProcessor = new BindableInt();

        private readonly BindableDouble healthFromProcessor = new BindableDouble();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthFromProcessor.BindTo(healthProcessor.Health);
            livesFromProcessor.BindTo(healthProcessor.Lives);

            livesFromProcessor.Value += Lives.Value;
        }
    }
}
