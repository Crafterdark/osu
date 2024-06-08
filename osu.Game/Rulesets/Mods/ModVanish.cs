// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModVanish : Mod, IApplicableToScoreProcessor, IUpdatableByPlayfield
    {
        public override string Name => "Vanish";
        public override string Acronym => "VH";
        public override LocalisableString Description => "Where are the notes?";
        public override double ScoreMultiplier => 1.12;
        public override IconUsage? Icon => FontAwesome.Solid.LowVision;
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModHidden), typeof(ModFlashlight) }).ToArray();

        /// <summary>
        /// Slightly higher than the cutoff.
        /// </summary>
        protected const float MIN_ALPHA = 0.0002f;

        protected const float TRANSITION_DURATION = 100;

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        protected float ComboBasedAlpha;

        [SettingSource(
            "Vanish at combo",
            "The combo count at which the objects completely vanish",
            SettingControlType = typeof(SettingsSlider<int, VanishComboSlider>)
        )]
        public virtual BindableInt VanishComboCount { get; } = new BindableInt(25)
        {
            MinValue = 0,
            MaxValue = 50,
        };

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (VanishComboCount.Value == 0) return;

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                ComboBasedAlpha = Math.Max(MIN_ALPHA, 1 - (float)combo.NewValue / VanishComboCount.Value);
            }, true);
        }

        public virtual void Update(Playfield playfield)
        {
            // AlwaysPresent required for notes.
            playfield.HitObjectContainer.AlwaysPresent = true;
            playfield.HitObjectContainer.Alpha = (float)Interpolation.Lerp(playfield.HitObjectContainer.Alpha, ComboBasedAlpha, Math.Clamp(playfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));
        }
    }

    public partial class VanishComboSlider : RoundedSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always vanish" : base.TooltipText;
    }
}
