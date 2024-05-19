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
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModClassic : ModClassic, IApplicableToBeatmapConverter, IApplicableToBeatmapProcessor, IApplicableFailOverride, IApplicableToHealthProcessor, IApplicableToDifficulty
    {
        public override Type[] IncompatibleMods => new[] {
            typeof(CatchModExtraLives),
            typeof(CatchModSpicyPatterns),
        };

        private CatchModExtraLives internalModExtraLives = new CatchModExtraLives();

        private CatchModSpicyPatterns internalModSpicyPatterns = new CatchModSpicyPatterns();

        private BindableBool usesModEasy = new BindableBool();

        private BindableBool usesModHardRock = new BindableBool();

        private BindableBool usesModSuddenDeathOrPerfect = new BindableBool(true);

        [SettingSource("Classic Easy", "Always include two extra lives and remove original hyperdashes when using Easy.")]
        public BindableBool UsesClassicEasy { get; } = new BindableBool(true);

        [SettingSource("Classic Hard Rock", "Always include spicy patterns and remove the horizontal flip on the fruits when using Hard Rock.")]
        public BindableBool UsesClassicHardRock { get; } = new BindableBool(true);

        [SettingSource("Classic Immediate Fail", "Ignore all extra lives when failing with Sudden Death or Perfect.")]
        public BindableBool UsesClassicImmediateFail { get; } = new BindableBool(true);

        [SettingSource("Classic Legacy Random", "Legacy random couldn't generate upper bounds correctly.")]
        public BindableBool UsesClassicLegacyRandom { get; } = new BindableBool(true);

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
            var modEasy = mods.OfType<CatchModEasy>().SingleOrDefault();
            var modHardRock = mods.OfType<CatchModHardRock>().SingleOrDefault();
            var modSuddenDeath = mods.OfType<CatchModSuddenDeath>().SingleOrDefault();
            var modPerfect = mods.OfType<CatchModPerfect>().SingleOrDefault();

            usesModEasy.Value = modEasy != null;
            usesModHardRock.Value = modHardRock != null;
            usesModSuddenDeathOrPerfect.Value = modSuddenDeath != null || modPerfect != null;

            if (modHardRock != null && UsesClassicHardRock.Value)
                modHardRock.MirrorFruitsOnGeneration = false;
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (usesModEasy.Value && UsesClassicEasy.Value)
                internalModExtraLives.ApplyToDifficulty(difficulty);
        }

        public bool PerformFail()
        {
            if (usesModSuddenDeathOrPerfect.Value && UsesClassicImmediateFail.Value)
                return true;
            else if (usesModEasy.Value && UsesClassicEasy.Value)
                return internalModExtraLives.PerformFail();

            return true;
        }

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            if (usesModEasy.Value && UsesClassicEasy.Value)
                internalModExtraLives.ApplyToHealthProcessor(healthProcessor);
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

            if (usesModHardRock.Value && UsesClassicHardRock.Value)
                internalModSpicyPatterns.ApplyToBeatmapProcessor(beatmapProcessor);

            catchBeatmapProcessor.NewTinyGeneration = !MissingSegmentOnJuiceStream.Value || !IncompleteSegmentOnJuiceStream.Value;
            catchBeatmapProcessor.ClassicLegacyRandom = UsesClassicLegacyRandom.Value;
            catchBeatmap.OriginalHyperDashGeneration.Value = usesModEasy.Value ? !UsesClassicEasy.Value : !RemoveOriginalHyperDashes.Value;
            catchBeatmap.IsProcessingSymmetricalHyperDash.Value = !AsymmetricalHyperDashGeneration.Value;
        }
    }
}
