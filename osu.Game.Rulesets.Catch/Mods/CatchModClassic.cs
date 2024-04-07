// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor
    {
        public override Type[] IncompatibleMods => new[] {
            typeof(CatchModSpicyPatterns),
        };

        [SettingSource("Hard Rock spicy patterns", "Include spicy patterns when using Hard Rock.")]
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
            catchBeatmap.IsProcessingSymmetricalHyperDash = !AsymmetricalHyperDashGeneration.Value;
        }
    }
}
