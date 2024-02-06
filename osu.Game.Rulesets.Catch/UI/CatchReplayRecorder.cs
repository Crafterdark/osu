// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<CatchAction> actions, ReplayFrame previousFrame, TypeCaller typeCaller)
        {
            //If the current frame is a judgement then don't store the previous frame, otherwise store it
            int isUpdate = 0;

            if (typeCaller == TypeCaller.Update)
            {
                HasJudgement = false;
                isUpdate = 1;
            }

            return new CatchReplayFrame(Time.Current, playfield.Catcher.X, actions.Contains(CatchAction.Dash), isUpdate, previousFrame as CatchReplayFrame);
        }
    }
}
