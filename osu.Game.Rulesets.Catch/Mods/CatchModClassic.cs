// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor, IApplicableFailOverride, IApplicableToHealthProcessor, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override Type[] IncompatibleMods => new[] {
            typeof(CatchModSpicyPatterns),
            typeof(CatchModExtraLives),
        };

        [SettingSource("Classic Easy", "Include two extra lives when using Easy.")]
        public BindableBool ClassicExtraLives { get; } = new BindableBool(true);

        [SettingSource("Classic Hard Rock", "Include spicy patterns when using Hard Rock.")]
        public BindableBool ClassicSpicyPatterns { get; } = new BindableBool(true);

        [SettingSource("Asymmetrical hyperdash generation", "Stable generated asymmetrical hyperdashes during beatmap processing.")]
        public Bindable<bool> AsymmetricalHyperDashGeneration { get; } = new BindableBool(true);

        [SettingSource("Remove regular hyperdashes", "Removes the original hyperdashes and keeps the ones from the modified beatmap.")]
        public BindableBool RemoveRegularHyperDashes { get; } = new BindableBool(true);

        [SettingSource("Missing segment on juice streams", "The last segment of various juice streams didn't start the tiny droplet generation.")]
        public Bindable<bool> MissingSegmentOnJuiceStream { get; } = new BindableBool(true);

        [SettingSource("Incomplete segment on juice streams", "The last segment of various juice streams didn't generate all the tiny droplets.")]
        public Bindable<bool> IncompleteSegmentOnJuiceStream { get; } = new BindableBool(true);

        [SettingSource("Mistimed tiny droplets", "Several juice streams didn't generate the tiny droplets on beat.")]
        public Bindable<bool> MistimedTinyDroplets { get; } = new BindableBool(true);

        private int retries;

        private const int total_classic_extra_lives = 2;

        private readonly BindableNumber<double> health = new BindableDouble();

        private DrawableCatchRuleset drawableCatchRuleset = null!;

        public CatchModClassic()
        {
            Func<IReadOnlyList<Mod>, double, double> classicEasyDetection = (modList, currMultiplier) =>
            {
                if (modList.OfType<CatchModEasy>().SingleOrDefault().IsNotNull() && ClassicExtraLives.Value)
                    return currMultiplier * Math.Sqrt(0.5);

                return currMultiplier;
            };


            ScoreMultiplierAdjustments.Add(classicEasyDetection);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchModEasy = drawableCatchRuleset.Mods.OfType<CatchModEasy>().SingleOrDefault();

            bool usingExtraLives = ClassicExtraLives.Value;

            retries = usingExtraLives && catchModEasy.IsNotNull() ? total_classic_extra_lives : 0;
        }

        public bool PerformFail()
        {
            if (retries == 0) return true;

            health.Value = health.MaxValue;
            retries--;

            return false;
        }

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            health.BindTo(healthProcessor.Health);
        }

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var catchBeatmapConverter = (CatchBeatmapConverter)beatmapConverter;

            catchBeatmapConverter.NewSegmentOnJuiceStream.Value = !MissingSegmentOnJuiceStream.Value;
            catchBeatmapConverter.CompleteSegmentOnJuiceStream.Value = !IncompleteSegmentOnJuiceStream.Value;
            catchBeatmapConverter.TimedTinyDroplets.Value = !MistimedTinyDroplets.Value;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchBeatmapProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            var catchBeatmap = (CatchBeatmap)beatmapProcessor.Beatmap;

            catchBeatmapProcessor.ClassicSpicyPatterns = ClassicSpicyPatterns.Value;
            catchBeatmapProcessor.NewTinyGeneration = !MissingSegmentOnJuiceStream.Value || !IncompleteSegmentOnJuiceStream.Value;
            catchBeatmap.RegularHyperDashGeneration.Value = !RemoveRegularHyperDashes.Value;
            catchBeatmap.IsProcessingSymmetricalHyperDash.Value = !AsymmetricalHyperDashGeneration.Value;
        }
    }
}
