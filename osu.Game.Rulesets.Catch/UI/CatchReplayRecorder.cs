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
            : base(target, playfield)
        {
            this.playfield = playfield;
        }

        private int getCatcherDirection(List<CatchAction> actions)
        {
            int finalDirection = 0;

            bool moveLeft = actions.Contains(CatchAction.MoveLeft);
            bool moveRight = actions.Contains(CatchAction.MoveRight);

            if (moveLeft && !moveRight)
                finalDirection = -1;
            else if (moveRight && !moveLeft)
                finalDirection = 1;
            else if (moveLeft && moveRight)
                finalDirection = 2;

            return finalDirection;
        }

        protected override ReplayFrame? GetLastFrameRecordHandler(FrameRecordHandler recordHandler, List<ReplayFrame> replayFrames) => replayFrames.Where(x => ((CatchReplayFrame)x).RecordHandler == recordHandler).LastOrDefault();

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<CatchAction> actions, ReplayFrame previousFrame, FrameRecordHandler recordHandler)
            => new CatchReplayFrame(Time.Current, playfield.Catcher.X, actions.Contains(CatchAction.Dash), getCatcherDirection(actions), (int)recordHandler, previousFrame as CatchReplayFrame);

        protected override bool IsValidAction(CatchAction action) => !playfield.CatcherArea.InvalidCatchActionList.Contains(action);
    }
}
