// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class ManiaReplayRecorder : ReplayRecorder<ManiaAction>
    {
        public ManiaReplayRecorder(Score score, ManiaPlayfield playfield)
            : base(score, playfield)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<ManiaAction> actions, ReplayFrame previousFrame, FrameRecordHandler recordHandler)
            => new ManiaReplayFrame(Time.Current, actions.ToArray());
    }
}
