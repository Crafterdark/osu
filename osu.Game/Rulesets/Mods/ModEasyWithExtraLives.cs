// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasyWithExtraLives : ModEasy, IApplicableToHealthProcessor
    {
        [SettingSource("Extra Lives", "Number of extra lives")]
        public Bindable<int> Lives { get; } = new BindableInt(2)
        {
            MinValue = 0,
            MaxValue = 10
        };

        public override string SettingDescription => Lives.IsDefault ? string.Empty : $"{"lives".ToQuantity(Lives.Value)}";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModAccuracyChallenge)).ToArray();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Lives.Add(Lives.Value);
        }
    }
}
