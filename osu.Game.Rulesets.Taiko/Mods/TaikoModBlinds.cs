// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModBlinds : ModBlinds, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToHealthProcessor, IApplicableToScoreProcessor
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(Blinds = new DrawableBlinds(drawableRuleset.Playfield, drawableRuleset.Beatmap));
        }

        protected float ComboBasedAlpha;

        public const int COMBO_VALUE_BLINDS = 50;

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (BlindsFullOpaque.Value)
                return;

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                ComboBasedAlpha = Math.Min(1, 0.50f + 0.50f * ((float)combo.NewValue / COMBO_VALUE_BLINDS));

                if (Blinds != null)
                    Blinds.Alpha = ComboBasedAlpha;
            }, true);
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += health => { Blinds.AnimateClosedness(0.6f + Math.Min(0.4f, (float)health.NewValue)); };
        }
    }
}
