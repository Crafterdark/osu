// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor
    {
        [SettingSource("Classic Easy", "Always include two extra lives when using Easy.")]
        public BindableBool UsesClassicEasy { get; } = new BindableBool(true);

        [SettingSource("Classic Hard Rock", "Always include spicy patterns and remove mirror on fruits when using Hard Rock.")]
        public BindableBool UsesClassicHardRock { get; } = new BindableBool(true);

        [SettingSource("Old Legacy Random", "Legacy random couldn't generate upper bounds correctly.")]
        public BindableBool OldLegacyRandom { get; } = new BindableBool(true);

        [SettingSource("Asymmetrical hyperdash generation", "Stable generated asymmetrical hyperdashes during beatmap processing.")]
        public BindableBool AsymmetricalHyperDashGeneration { get; } = new BindableBool(true);

        [SettingSource("Remove original hyperdashes", "Removes the original hyperdashes and keeps the ones from the modified beatmap.")]
        public BindableBool RemoveOriginalHyperDashes { get; } = new BindableBool(true);

        [SettingSource("Missing segment on juice streams", "The last segment of various juice streams didn't start the tiny droplet generation.")]
        public BindableBool MissingSegmentOnJuiceStream { get; } = new BindableBool(true);

        [SettingSource("Incomplete segment on juice streams", "The last segment of various juice streams didn't generate all the tiny droplets.")]
        public BindableBool IncompleteSegmentOnJuiceStream { get; } = new BindableBool(true);

        [SettingSource("Mistimed tiny droplets", "Several juice streams didn't generate the tiny droplets on beat.")]
        public BindableBool MistimedTinyDroplets { get; } = new BindableBool(true);

        public CatchModClassic()
        {
            Func<IReadOnlyList<Mod>, double, double> catchModClassicScoreAdjustments = (modList, currMultiplier) =>
            {
                if (modList.OfType<CatchModEasy>().SingleOrDefault() != null && UsesClassicEasy.Value)
                    currMultiplier *= Math.Sqrt(0.5);

                return currMultiplier;
            };

            if (!ScoreMultiplierAdjustments.Contains(catchModClassicScoreAdjustments))
                ScoreMultiplierAdjustments.Add(catchModClassicScoreAdjustments);
        }

        public override void CheckModsForConditions(IReadOnlyList<Mod> mods)
        {
            foreach (Mod mod in mods)
            {
                switch (mod)
                {
                    case CatchModEasy modEasy:
                        if (UsesClassicEasy.Value)
                            modEasy.ExtraLivesOnGameplay = true;
                        break;
                    case CatchModHardRock modHardRock:
                        if (UsesClassicHardRock.Value)
                        {
                            modHardRock.MirrorFruitsOnGeneration = false;
                            modHardRock.SpicyPatternsOnGeneration = true;
                        }
                        break;
                }
            }
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

            catchBeatmapProcessor.NewTinyGeneration = !MissingSegmentOnJuiceStream.Value || !IncompleteSegmentOnJuiceStream.Value;
            catchBeatmapProcessor.UsesOldLegacyRandom = OldLegacyRandom.Value;
            catchBeatmap.OriginalHyperDashGeneration.Value = !RemoveOriginalHyperDashes.Value;
            catchBeatmap.IsHyperDashGenerationSymmetrical.Value = !AsymmetricalHyperDashGeneration.Value;
        }
    }
}
