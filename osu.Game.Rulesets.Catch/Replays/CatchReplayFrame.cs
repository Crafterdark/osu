
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<CatchAction> Actions = new List<CatchAction>();

        public float Position;
        public bool Dashing;
        public int Direction;
        public FrameRecordHandler RecordHandler;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? position = null, bool dashing = false, int direction = 0, int recordHandler = 0, CatchReplayFrame? lastFrame = null)
            : base(time)
        {
            Position = position ?? -1;
            Dashing = dashing;
            Direction = direction;
            RecordHandler = (FrameRecordHandler)recordHandler;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            if (Direction == 1)
                Actions.Add(CatchAction.MoveRight);
            else if (Direction == -1)
                Actions.Add(CatchAction.MoveLeft);
            else if (Direction == 2)
            {
                Actions.Add(CatchAction.MoveLeft);
                Actions.Add(CatchAction.MoveRight);
            }
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame? lastFrame = null)
        {
            Position = currentFrame.Position.X;
            Dashing = currentFrame.ButtonState == ReplayButtonState.Left1;
            Direction = currentFrame.Direction;
            RecordHandler = (FrameRecordHandler)currentFrame.RecordHandler;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            if (Direction == 1)
                Actions.Add(CatchAction.MoveRight);
            else if (Direction == -1)
                Actions.Add(CatchAction.MoveLeft);
            else if (Direction == 2)
            {
                Actions.Add(CatchAction.MoveLeft);
                Actions.Add(CatchAction.MoveRight);
            }
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(CatchAction.Dash)) state |= ReplayButtonState.Left1;

            return new LegacyReplayFrame(Time, Position, null, state, Direction, (int)RecordHandler);
        }
    }
}
