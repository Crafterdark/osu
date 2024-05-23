// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModExtremeCustomize : ModExtremeCustomize, IApplicableToHealthProcessor
    {
        [SettingSource("Extra Lives", "Number of extra lives (1 - 999999)", SettingControlType = typeof(SettingsCustomIntegerNumberBox), SettingControlArguments = new object[] { 0, 0, 999999 })]
        public Bindable<int> TotalLives { get; } = new Bindable<int>(0);

        [SettingSource("Maximum Health", "Maximum achievable value of health (1 - 99)", SettingControlType = typeof(SettingsCustomIntegerNumberBox), SettingControlArguments = new object[] { 100, 1, 100 })]
        public Bindable<int> MaxHealth { get; } = new Bindable<int>(100);

        private readonly BindableInt livesFromProcessor = new BindableInt();

        private readonly BindableDouble healthFromProcessor = new BindableDouble();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            livesFromProcessor.BindTo(healthProcessor.Lives);
            healthFromProcessor.BindTo(healthProcessor.Health);

            livesFromProcessor.Value += TotalLives.Value;

            healthFromProcessor.BindValueChanged(e =>
            {
                healthFromProcessor.Value = Math.Min(healthFromProcessor.Value, (double)MaxHealth.Value / 100);
            }
            );
        }
    }
}
