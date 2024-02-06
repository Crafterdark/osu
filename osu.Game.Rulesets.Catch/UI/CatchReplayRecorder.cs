// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class CatchReplayRecorder : ReplayRecorder<CatchAction>
    {
        private readonly CatchPlayfield playfield;

        public CatchReplayRecorder(Score target, CatchPlayfield playfield)
            : base(target)
        {
            this.playfield = playfield;

            playfield.NewResult += (d, r) =>
            {
                HasJudgement = true;
            };
        }

        protected override ReplayFrame? GetLastFrameRecordHandler(FrameRecordHandler recordHandler, List<ReplayFrame> replayFrames) => replayFrames.Where(x => ((CatchReplayFrame)x).RecordHandler == recordHandler).LastOrDefault();

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<CatchAction> actions, ReplayFrame previousFrame, FrameRecordHandler recordHandler)
        {
            if (recordHandler == FrameRecordHandler.Judgement)
            {
                //Reset judgement status for the next frame
                HasJudgement = false;
            }

            int direction = actions.Contains(CatchAction.MoveLeft) ? -1 : (actions.Contains(CatchAction.MoveRight) ? 1 : 0);

            return new CatchReplayFrame(Time.Current, playfield.Catcher.X, actions.Contains(CatchAction.Dash), direction, (int)recordHandler, previousFrame as CatchReplayFrame);
        }
    }
}
