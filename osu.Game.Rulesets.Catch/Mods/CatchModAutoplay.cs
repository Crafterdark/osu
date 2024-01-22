// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutoplay : ModAutoplay
    {
        [SettingSource("Priority on accuracy over score", "Autoplay main priority will go towards maximum achievable accuracy, instead of maximum achievable score.")]
        public Bindable<bool> PriorityAccuracyOverScore { get; set; } = new Bindable<bool>(true);

        internal CatchAutoGenerator CatchAutoGeneratorObject = null!;

        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            CatchAutoGeneratorObject = new CatchAutoGenerator(beatmap)
            {
                AutoplayPriorityType = PriorityAccuracyOverScore.Value ? CatchAutoGenerator.PriorityType.Accuracy : CatchAutoGenerator.PriorityType.Score
            };

            return new ModReplayData(CatchAutoGeneratorObject.Generate(), new ModCreatedUser { Username = "osu!salad" });
        }
    }
}
