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

        private CatcherDirection getCatcherDirection(List<CatchAction> actions)
        {
            bool moveLeft = actions.Contains(CatchAction.MoveLeft);
            bool moveRight = actions.Contains(CatchAction.MoveRight);

            if (moveLeft && !moveRight)
                return CatcherDirection.Left;
            else if (moveRight && !moveLeft)
                return CatcherDirection.Right;
            else if (moveLeft && moveRight)
                return CatcherDirection.Both;

            return CatcherDirection.None;
        }

        protected override ReplayFrame? GetLastFrameRecordType(FrameRecordType frameRecordType, List<ReplayFrame> replayFrames) => replayFrames.Where(x => ((CatchReplayFrame)x).FrameRecordType == frameRecordType).LastOrDefault();

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<CatchAction> actions, FrameRecordType frameRecordType, ReplayFrame previousFrame)
            => new CatchReplayFrame(Time.Current, playfield.Catcher.X, actions.Contains(CatchAction.Dash), getCatcherDirection(actions), frameRecordType, previousFrame as CatchReplayFrame);
    }
}
