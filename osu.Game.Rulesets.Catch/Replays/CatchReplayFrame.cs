// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Catch.UI;
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
        public CatcherDirection Direction;
        public FrameRecordType FrameRecordType;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? position = null, bool dashing = false, CatcherDirection direction = CatcherDirection.None, FrameRecordType frameRecordType = FrameRecordType.Update, CatchReplayFrame? lastFrame = null)
            : base(time)
        {
            Position = position ?? -1;
            Dashing = dashing;
            Direction = direction;
            FrameRecordType = frameRecordType;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            switch (Direction)
            {
                case CatcherDirection.Left:
                    Actions.Add(CatchAction.MoveLeft);
                    break;
                case CatcherDirection.Right:
                    Actions.Add(CatchAction.MoveRight);
                    break;
                case CatcherDirection.Both:
                    Actions.AddRange(new[] { CatchAction.MoveLeft, CatchAction.MoveRight });
                    break;
            }
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame? lastFrame = null)
        {
            Position = currentFrame.Position.X;
            Dashing = currentFrame.ButtonState == ReplayButtonState.Left1;
            Direction = (CatcherDirection)currentFrame.Direction;
            FrameRecordType = currentFrame.FrameRecordType;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            switch (Direction)
            {
                case CatcherDirection.Left:
                    Actions.Add(CatchAction.MoveLeft);
                    break;
                case CatcherDirection.Right:
                    Actions.Add(CatchAction.MoveRight);
                    break;
                case CatcherDirection.Both:
                    Actions.AddRange(new[] { CatchAction.MoveLeft, CatchAction.MoveRight });
                    break;
            }
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(CatchAction.Dash)) state |= ReplayButtonState.Left1;

            return new LegacyReplayFrame(Time, Position, null, state, (int)Direction, FrameRecordType);
        }
    }
}
