// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor
    {
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
            catchBeatmapProcessor.NewTinyGeneration = !MissingSegmentOnJuiceStream.Value || !IncompleteSegmentOnJuiceStream.Value;
        }
    }
}
