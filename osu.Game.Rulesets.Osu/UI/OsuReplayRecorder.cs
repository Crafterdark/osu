﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuReplayRecorder : ReplayRecorder<OsuAction>
    {
        public OsuReplayRecorder(Score score, OsuPlayfield playfield)
            : base(score, playfield)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<OsuAction> actions, ReplayFrame previousFrame, FrameRecordHandler recordHandler)
            => new OsuReplayFrame(Time.Current, mousePosition, actions.ToArray());
    }
}
