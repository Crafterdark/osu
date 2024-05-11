// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModBlinds : ModBlinds, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToHealthProcessor, IApplicableToScoreProcessor
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(Blinds = new DrawableBlinds(drawableRuleset.Playfield, drawableRuleset.Beatmap));
        }

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(CatchModDodge)).ToArray();

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
            healthProcessor.Health.ValueChanged += health => { Blinds.AnimateClosedness((float)health.NewValue); };
        }
    }
}
