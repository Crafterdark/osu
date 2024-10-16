﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Catch.Utils;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmap : Beatmap<CatchHitObject>
    {
        ///<summary>
        /// Whether the hyperdash generation during map processing is fully symmetrical.
        ///</summary>
        public BindableBool IsHyperDashGenerationSymmetrical = new BindableBool(true);

        /// <summary>
        /// Whether the beatmap uses original hyperdash generation.
        /// </summary>
        public BindableBool OriginalHyperDashGeneration = new BindableBool(true);

        /// <summary>
        /// Whether the beatmap uses modified hyperdashes generation.
        /// </summary>
        public BindableBool ModifiedHyperDashGeneration = new BindableBool(true);

        /// <summary>
        /// Whether the beatmap uses a limited catch playfield.
        /// </summary>
        public BindableBool UsesLimitedCatchPlayfield = new BindableBool();

        /// <summary>
        /// The adjusted base dash speed of the catcher without hyperdash status.
        /// </summary>
        public BindableDouble CatcherAdjustedDashSpeed = new BindableDouble(Catcher.BASE_DASH_SPEED);

        public LimitedCatchPlayfieldContainer LimitedCatchPlayfieldContainer { get; set; } = null!;

        public BindableBool HitObjectWithNextTarget = new BindableBool();

        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int fruits = HitObjects.Count(s => s is Fruit);
            int juiceStreams = HitObjects.Count(s => s is JuiceStream);
            int bananaShowers = HitObjects.Count(s => s is BananaShower);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Fruit Count",
                    Content = fruits.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                },
                new BeatmapStatistic
                {
                    Name = @"Juice Stream Count",
                    Content = juiceStreams.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                },
                new BeatmapStatistic
                {
                    Name = @"Banana Shower Count",
                    Content = bananaShowers.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                }
            };
        }

        /// <summary>
        /// Enumerate all <see cref="PalpableCatchHitObject"/>s, sorted by their start times.
        /// </summary>
        /// <remarks>
        /// If multiple objects have the same start time, the ordering is preserved (it is a stable sorting).
        /// </remarks>
        public static IEnumerable<PalpableCatchHitObject> GetPalpableObjects(IEnumerable<HitObject> hitObjects)
        {
            return hitObjects.SelectMany(selectPalpableObjects).OrderBy(h => h.StartTime);

            IEnumerable<PalpableCatchHitObject> selectPalpableObjects(HitObject h)
            {
                if (h is PalpableCatchHitObject palpable)
                    yield return palpable;

                foreach (var nested in h.NestedHitObjects.OfType<PalpableCatchHitObject>())
                    yield return nested;
            }
        }
    }
}
