// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchFramedReplayInputHandler : FramedReplayInputHandler<CatchReplayFrame>
    {
        public CatchFramedReplayInputHandler(Replay replay, CatchPlayfield catchPlayfield)
            : base(replay)
        {
            catchPlayfield.OnReplayJudgement += (o) =>
            {
                CatchReplayFrame? syncFrame;

                syncFrame = (CatchReplayFrame?)Frames?.Find(x => x.Time >= (int)o.StartTime && FrameRecordTypeUtils.IsFrameRecordTypeValidForJudgement(((CatchReplayFrame)x).FrameRecordType));

                if (syncFrame != null)
                    return syncFrame.Position;
                else
                    return catchPlayfield.Catcher.X;
            };
        }

        protected override bool IsImportant(CatchReplayFrame frame) => frame.Actions.Any();

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
            float position = Interpolation.ValueAt(CurrentTime, StartFrame.Position, EndFrame.Position, StartFrame.Time, EndFrame.Time);

            inputs.Add(new CatchReplayState
            {
                PressedActions = CurrentFrame?.Actions ?? new List<CatchAction>(),
                CatcherX = position
            });
        }

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float? CatcherX { get; set; }
        }
    }
}
