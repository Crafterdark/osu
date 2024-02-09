// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI
{
    public enum FrameRecordHandler
    {
        Update = 0,
        Input = 1,
        Judgement = 2,
        Mouse = 3,
        LegacyUpdateJudgement = 4,
        LegacyInputJudgement = 5,
    }
}
