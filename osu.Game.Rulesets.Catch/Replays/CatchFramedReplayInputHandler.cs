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
        public CatchFramedReplayInputHandler(Replay replay, CatchPlayfield playfield)
            : base(replay)
        {
            playfield.OnReplayJudgement += (obj) =>
            {
                CatchReplayFrame? syncFrame;

                //NOTE: WORKAROUND [!!!] This code is only meant to run for Classic (Legacy) replays as a way to minimize the mismatching caused by floating point errors. When a new proper fix for Lazer to Stable maps will be done and the maps will entirely match... then this code must be entirely removed.
                if (playfield.Catcher.IsLegacy)
                    syncFrame = (CatchReplayFrame?)Frames?.Find(x => x.Time >= ((int)obj.StartTime - 1) && FrameRecordHandlerUtils.IsRecordHandlerValidForJudgement(((CatchReplayFrame)x).RecordHandler));
                //This is the correct one
                else
                    syncFrame = (CatchReplayFrame?)Frames?.Find(x => x.Time >= (int)obj.StartTime && FrameRecordHandlerUtils.IsRecordHandlerValidForJudgement(((CatchReplayFrame)x).RecordHandler));

                if (syncFrame != null)
                    playfield.TrackedCatcherPosition = syncFrame.Position;
            };
        }

        protected override bool IsImportant(CatchReplayFrame frame) => frame.Actions.Any();

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
            float position = Interpolation.ValueAt(CurrentTime, StartFrame.Position, EndFrame.Position, StartFrame.Time, EndFrame.Time);

            inputs.Add(new CatchReplayState
            {
                PressedActions = CurrentFrame?.Actions ?? new List<CatchAction>(),
                CatcherX = position,
                Frames = Frames
            });
        }

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float? CatcherX { get; set; }
            public List<ReplayFrame>? Frames { get; set; }
        }
    }
}
