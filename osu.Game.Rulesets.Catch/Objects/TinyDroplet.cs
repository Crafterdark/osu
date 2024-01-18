// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class TinyDroplet : Droplet
    {
        /// <summary>
        /// Must be always true for osu!stable beatmaps. Might be false for osu!lazer beatmaps.
        /// </summary>
        public bool IsUsingOldRandom { get; set; } = true;
        public override Judgement CreateJudgement() => new CatchTinyDropletJudgement();
    }
}
